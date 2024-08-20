using ScrapServer.Core.NetObjs;
using ScrapServer.Core.Utils;
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

        /// <summary>
        /// Collects items into a specific slot of a container.
        /// 
        /// This implementation is designed to return sensible results and does not match `sm.container.collectToSlot` exactly.
        /// </summary>
        /// <param name="container">The container to collect the items into</param>
        /// <param name="itemStack">The item stack, including quantity, to collect</param>
        /// <param name="mustCollectAll">
        ///     If true, only collect items if the full item stack fits in the remaining space of the slot.
        ///     If false, collect as many items that fit into the remaining space, and do so without overflowing into other slots.
        /// </param>
        /// <returns>A tuple containing the number of items collected and the result of the operation</returns>
        /// <exception cref="SlotIndexOutOfRangeException">If the slot index is out of range</exception>
        public (ushort Collected, OperationResult Result) CollectToSlot(Container container, ItemStack itemStack, ushort slot, bool mustCollectAll = true)
        {
            var containerCopyOnWrite = modified.TryGetValue(container.Id, out Container? found) ? found : container.Clone();

            if (slot < 0 || slot >= containerCopyOnWrite.Items.Length)
            {
                throw new SlotIndexOutOfRangeException($"Slot {slot} is out of range [0, {containerCopyOnWrite.Items.Length})");
            }

            var currentItemStackInSlot = containerCopyOnWrite.Items[slot];

            if (!currentItemStackInSlot.IsStackableWith(itemStack))
            {
                return (0, OperationResult.NotStackable);
            }

            int max = containerService.GetMaximumStackSize(container, itemStack.Uuid);
            int remainingSpace = max - currentItemStackInSlot.Quantity;

            int quantityToCollect = Math.Min(remainingSpace, itemStack.Quantity);
            if (quantityToCollect <= 0)
            {
                return (0, OperationResult.NotEnoughSpace);
            }

            if (mustCollectAll && quantityToCollect < itemStack.Quantity)
            {
                return (0, OperationResult.NotEnoughSpaceForAll);
            }

            containerCopyOnWrite.Items[slot] = ItemStack.Combine(itemStack, currentItemStackInSlot);

            modified[container.Id] = containerCopyOnWrite;

            return ((ushort)quantityToCollect, OperationResult.Success);
        }

        /// <summary>
        /// Ends the transaction and applies the changes to the containers.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the transaction is not the current transaction</exception>
        public void EndTransaction()
        {
            if (containerService.CurrentTransaction != this)
            {
                throw new InvalidOperationException("Attempted to end a transaction that is not the current transaction");
            }

            foreach (var (id, container) in modified)
            {
                if (!containerService.Containers.TryGetValue(id, out Container? target))
                {
                    throw new InvalidOperationException($"Container with ID {id} was not found");
                }
                
                Array.Copy(container.Items, target.Items, container.Items.Length);

                target.Filter.Clear();
                target.Filter.UnionWith(container.Filter);
            }

            containerService.CurrentTransaction = null;
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
