using ScrapServer.Core.NetObjs;
using ScrapServer.Core.Utils;
using static ScrapServer.Core.NetObjs.Container;

namespace ScrapServer.Core;


public class ContainerService
{
    public static readonly ContainerService Instance = new();

    public Dictionary<uint, Container> Containers = [];

    public readonly UniqueIdProvider UniqueIdProvider = new();

    private volatile bool IsInTransaction = false;

    public class Transaction(ContainerService containerService) : IDisposable
    {
        private readonly Dictionary<uint, Container> modified = [];

        /// <summary>
        /// Collects items into a specific slot of a container.
        /// </summary>
        /// <param name="container">The container to collect the items into</param>
        /// <param name="itemStack">The item stack, including quantity, to collect</param>
        /// <param name="mustCollectAll">
        ///     If true, only collect items if the full item stack fits in the remaining space of the slot.
        ///     If false, collect as many items that fit into the remaining space, and do so without overflowing into other slots.
        /// </param>
        /// <returns>A tuple containing the number of items collected and the result of the operation</returns>
        public (ushort Collected, OperationResult Result) CollectToSlot(Container container, ItemStack itemStack, ushort slot, bool mustCollectAll = true)
        {
            var containerCopyOnWrite = modified.TryGetValue(container.Id, out Container? found) ? found : container.Clone();

            if (slot < 0 || slot > containerCopyOnWrite.Items.Length)
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

        public void EndTransaction()
        {
            if (!containerService.IsInTransaction)
            {
                throw new InvalidOperationException("Transaction was not started");
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

            containerService.IsInTransaction = false;
        }

        public void AbortTransaction()
        {
            if (!containerService.IsInTransaction)
            {
                throw new InvalidOperationException("Transaction was not started");
            }

            this.modified.Clear();

            containerService.IsInTransaction = false;
        }

        public void Dispose()
        {
            if (containerService.IsInTransaction)
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

    public Transaction BeginTransaction()
    {
        if (this.IsInTransaction)
        {
            throw new InvalidOperationException("Cannot start a transaction while one is already in progress");
        }

        this.IsInTransaction = true;

        return new Transaction(this);
    }

    public ushort GetMaximumStackSize(Container container, Guid uuid)
    {
        // TODO: Return the minimum of the maximum stack size of the item and the container

        return container.MaximumStackSize;
    }

    public Container CreateContainer(ushort size, ushort maximumStackSize = ushort.MaxValue)
    {
        var id = UniqueIdProvider.GetNextId();
        var container = new Container(id, size, maximumStackSize);

        Containers[id] = container;

        return container;
    }
}
