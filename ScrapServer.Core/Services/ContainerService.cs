using ScrapServer.Core.NetObjs;
using ScrapServer.Core.Utils;
using ScrapServer.Networking.Data;
using static ScrapServer.Core.NetObjs.Container;

namespace ScrapServer.Core;

public class ContainerService
{
    /// <summary>
    /// The singleton instance of the container service.
    /// </summary>
    public static readonly ContainerService Instance = new();

    /// <summary>
    /// The dictionary of containers, indexed by their unique ID.
    /// </summary>
    public Dictionary<uint, Container> Containers = [];

    /// <summary>
    /// The unique ID provider for containers.
    /// </summary>
    public readonly UniqueIdProvider UniqueIdProvider = new();

    /// <summary>
    /// The current transaction, null if no transaction is in progress.
    /// </summary>
    private volatile Transaction? CurrentTransaction;

    /// <summary>
    /// New transactions should be created using <see cref="BeginTransaction"/>.
    /// </summary>
    /// <param name="containerService"></param>
    public class Transaction(ContainerService containerService) : IDisposable
    {
        private readonly Dictionary<uint, Container> modified = [];

        private Container GetOrCloneContainer(Container container)
        {
            return modified.TryGetValue(container.Id, out Container? found) ? found : container.Clone();
        }

        /// <summary>
        /// Calculates the remaining space in a container slot for an item stack.
        /// </summary>
        /// <param name="containerTo">The container to calculate the remaining space in</param>
        /// <param name="slotTo">The slot to calculate the remaining space in</param>
        /// <param name="itemStack">The item stack to calculate the remaining space for if it were to be combined with the item stack in the slot</param>
        /// <returns></returns>
        private ushort GetRemainingSpace(Container containerTo, ushort slotTo, ItemStack itemStack)
        {
            var currentItemStackInSlot = containerTo.Items[slotTo];

            if (!currentItemStackInSlot.IsStackableWith(itemStack))
            {
                return 0;
            }

            int max = containerService.GetMaximumStackSize(containerTo, itemStack.Uuid);
            int remainingSpace = max - currentItemStackInSlot.Quantity;
            
            if (remainingSpace <= 0)
            {
                return 0;
            }

            return (ushort)remainingSpace;
        }

        /// <summary>
        /// Collects items into a specific slot of a container.
        /// </summary>
        /// <remarks>
        /// This implementation is designed to return sensible results and does not match `sm.container.collectToSlot` exactly.
        /// </remarks>
        /// <param name="container">The container to collect the items into</param>
        /// <param name="itemStack">The item stack, including quantity, to collect</param>
        /// <param name="slot">The slot to collect the items into</param>
        /// <param name="mustCollectAll">
        ///     If <see langword="true" />, only collect items if the full <paramref name="itemStack"/> fits in the remaining space of the slot.
        ///     If <see langword="false" />, collect as many items that fit into the remaining space, and do so without overflowing into other slots.
        /// </param>
        /// <returns>A tuple containing the number of items collected and the result of the operation</returns>
        /// <exception cref="SlotIndexOutOfRangeException">If the slot index is out of range</exception>
        public (ushort Collected, OperationResult Result) CollectToSlot(
            Container container,
            ItemStack itemStack,
            ushort slot,
            bool mustCollectAll = true
        )
        {
            var containerCopyOnWrite = this.GetOrCloneContainer(container);

            if (slot < 0 || slot >= containerCopyOnWrite.Items.Length)
            {
                throw new SlotIndexOutOfRangeException($"Slot {slot} is out of range [0, {containerCopyOnWrite.Items.Length})");
            }

            var currentItemStackInSlot = containerCopyOnWrite.Items[slot];

            if (!currentItemStackInSlot.IsStackableWith(itemStack))
            {
                return (0, OperationResult.NotStackable);
            }

            int remainingSpace = this.GetRemainingSpace(containerCopyOnWrite, slot, itemStack);
            if (remainingSpace <= 0)
            {
                return (0, OperationResult.NotEnoughSpace);
            }

            if (mustCollectAll && remainingSpace < itemStack.Quantity)
            {
                return (0, OperationResult.NotEnoughSpaceForAll);
            }

            int quantityToCollect = Math.Min(remainingSpace, itemStack.Quantity);

            containerCopyOnWrite.Items[slot] = ItemStack.Combine(currentItemStackInSlot, itemStack with {
                Quantity = (ushort)quantityToCollect
            });

            modified[container.Id] = containerCopyOnWrite;

            return ((ushort)quantityToCollect, OperationResult.Success);
        }

        /// <summary>
        /// Moves items from one slot to another slot in the same or different container.
        /// </summary>
        /// <param name="containerFrom">The container to move the items from</param>
        /// <param name="slotFrom">The slot to move the items from</param>
        /// <param name="containerTo">The container to move the items to</param>
        /// <param name="slotTo">The slot to move the items to</param>
        /// <param name="quantity">The quantity of items to move</param>
        /// <param name="mustMoveAll">
        ///     If <see langword="true" />, only move items if the full <paramref name="quantity"/> fits in the remaining space of the slot.
        ///     If <see langword="false" />, move as many items that fit into the remaining space, and do so without overflowing into other slots.
        /// </param>
        /// <returns>A tuple containing the number of items moved and the result of the operation</returns>
        /// <exception cref="SlotIndexOutOfRangeException">If the slot index is out of range</exception>
        public (ushort Moved, OperationResult Result) Move(
            Container containerFrom,
            ushort slotFrom,
            Container containerTo,
            ushort slotTo,
            ushort quantity,
            bool mustMoveAll = true
        )
        {
            var containerFromCopyOnWrite = this.GetOrCloneContainer(containerFrom);
            var containerToCopyOnWrite = containerFrom.Equals(containerTo)
                ? containerFromCopyOnWrite
                : this.GetOrCloneContainer(containerTo);

            if (slotFrom < 0 || slotFrom >= containerFromCopyOnWrite.Items.Length)
            {
                throw new SlotIndexOutOfRangeException($"Slot {slotFrom} of source container is out of range [0, {containerFromCopyOnWrite.Items.Length})");
            }

            if (slotTo < 0 || slotTo >= containerToCopyOnWrite.Items.Length)
            {
                throw new SlotIndexOutOfRangeException($"Slot {slotTo} of destination container is out of range [0, {containerToCopyOnWrite.Items.Length})");
            }

            var itemStackFrom = containerFromCopyOnWrite.Items[slotFrom];
            var itemStackTo = containerToCopyOnWrite.Items[slotTo];


            if (!itemStackFrom.IsStackableWith(itemStackTo))
            {
                return (0, OperationResult.NotStackable);
            }

            int quantityToMove = Math.Min(quantity, itemStackFrom.Quantity);
            if (quantityToMove <= 0)
            {
                return (0, OperationResult.Success);
            }

            if (containerFrom == containerTo && slotFrom == slotTo)
            {
                if (mustMoveAll && itemStackFrom.Quantity < quantity)
                {
                    return (0, OperationResult.NotEnoughSpaceForAll);
                }
                else
                {
                    return ((ushort)quantityToMove, OperationResult.Success);
                }
            }

            var remainingSpace = this.GetRemainingSpace(containerToCopyOnWrite, slotTo, itemStackFrom);
            quantityToMove = Math.Min(quantityToMove, remainingSpace);

            if (mustMoveAll && quantityToMove < quantity)
            {
                return (0, OperationResult.NotEnoughSpaceForAll);
            }

            containerFromCopyOnWrite.Items[slotFrom] = itemStackFrom with {
                Quantity = (ushort)(itemStackFrom.Quantity - quantityToMove)
            };
            containerToCopyOnWrite.Items[slotTo] = ItemStack.Combine(
                itemStackTo,
                itemStackFrom with { Quantity = (ushort)quantityToMove }
            );

            if (containerFromCopyOnWrite.Items[slotFrom].Quantity == 0)
            {
                containerFromCopyOnWrite.Items[slotFrom] = ItemStack.Empty;
            }

            modified[containerFrom.Id] = containerFromCopyOnWrite;
            modified[containerTo.Id] = containerToCopyOnWrite;

            return ((ushort)quantityToMove, OperationResult.Success);
        }

        /// <summary>
        /// Ends the transaction and applies the changes to the containers.
        /// </summary>
        /// <returns>A list of tuples containing the updated containers and their network updates</returns>
        /// <exception cref="InvalidOperationException">If the transaction is not the current transaction</exception>
        public IEnumerable<(Container, UpdateContainer)> EndTransaction()
        {
            if (containerService.CurrentTransaction != this)
            {
                throw new InvalidOperationException("Attempted to end a transaction that is not the current transaction");
            }

            List<(Container, UpdateContainer)> updates = [];

            foreach (var (id, container) in modified)
            {
                if (!containerService.Containers.TryGetValue(id, out Container? target))
                {
                    throw new InvalidOperationException($"Container with ID {id} was not found");
                }

                var update = container.CreateNetworkUpdate(target);
                
                Array.Copy(container.Items, target.Items, container.Items.Length);

                target.Filter.Clear();
                target.Filter.UnionWith(container.Filter);

                updates.Add((target, update));
            }

            containerService.CurrentTransaction = null;

            return updates;
        }

        /// <summary>
        /// Aborts the transaction and discards the changes.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the transaction is not the current transaction</exception>
        public void AbortTransaction()
        {
            if (containerService.CurrentTransaction != this)
            {
                throw new InvalidOperationException("Attempted to abort a transaction that is not the current transaction");
            }

            containerService.CurrentTransaction = null;
        }

        public void Dispose()
        {
            if (containerService.CurrentTransaction != null)
            {
                this.AbortTransaction();
                Console.WriteLine("Transaction was not committed or rolled back, aborting...");

                // Cannot throw an exception here, or it will be silently swallowed
                // https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1065#dispose-methods
            }
            GC.SuppressFinalize(this);
        }

        public class SlotIndexOutOfRangeException(string message) : Exception(message)
        {
        }

        public enum OperationResult
        {
            Success,
            NotStackable,
            NotEnoughSpace,
            NotEnoughSpaceForAll,
        }
    }

    /// <summary>
    /// Creates a new transaction and sets it as the current transaction.
    /// </summary>
    /// <returns>The transaction</returns>
    /// <exception cref="InvalidOperationException">If a transaction is already in progress</exception>
    public Transaction BeginTransaction()
    {
        if (this.CurrentTransaction != null)
        {
            throw new InvalidOperationException("Cannot start a transaction while one is already in progress");
        }

        return this.CurrentTransaction = new Transaction(this);
    }

    /// <summary>
    /// Gets the maximum stack size of an item in a container.
    /// </summary>
    /// <param name="container">The container</param>
    /// <param name="uuid">The UUID of the item</param>
    /// <returns>The maximum stack size</returns>
    public ushort GetMaximumStackSize(Container container, Guid uuid)
    {
        // TODO: Return the minimum of the maximum stack size of the item and the container

        return container.MaximumStackSize;
    }

    /// <summary>
    /// Creates a new container with a unique ID and adds it to the list of containers.
    /// </summary>
    /// <param name="size">The amount of slots in the container</param>
    /// <param name="maximumStackSize">The maximum stack size of items in the container</param>
    /// <returns>The created container</returns>
    public Container CreateContainer(ushort size, ushort maximumStackSize = ushort.MaxValue)
    {
        var id = UniqueIdProvider.GetNextId();
        var container = new Container(id, size, maximumStackSize);

        Containers[id] = container;

        return container;
    }

    /// <summary>
    /// Removes a container from the list of containers.
    /// </summary>
    /// <param name="id">The ID of the container to remove</param>
    /// <returns>true if the container was removed; otherwise, false</returns>
    public bool RemoveContainer(uint id)
    {
        return Containers.Remove(id);
    }
}
