using NUnit.Framework;
using ScrapServer.Core;
using static ScrapServer.Core.ContainerService.Transaction;
using static ScrapServer.Core.NetObjs.Container;

namespace ScrapServer.CoreTests.Services;

[TestFixture]
public class ContainerServiceTests
{
    private ContainerService CreateService()
    {
        return new ContainerService();
    }

    private readonly ItemStack WoodBlock = new(Guid.Parse("df953d9c-234f-4ac2-af5e-f0490b223e71"), ItemStack.NoInstanceId, (ushort)1);
    private readonly ItemStack ConcreteBlock = new(Guid.Parse("a6c6ce30-dd47-4587-b475-085d55c6a3b4"), ItemStack.NoInstanceId, (ushort)1);

    [Test]
    public void BeginTransaction_CorrectUsage_ReturnsTransaction()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(transaction, Is.InstanceOf<ContainerService.Transaction>());

        transaction.EndTransaction();
    }

    [Test]
    public void BeginTransaction_DoubleBegin_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(() => service.BeginTransaction(), Throws.InvalidOperationException);

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void EndTransaction_EndPreviousTransaction_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        using var transactionA = service.BeginTransaction();
        transactionA.EndTransaction();
        using var transactionB = service.BeginTransaction();

        // Assert
        Assert.That(() => transactionA.EndTransaction(), Throws.InvalidOperationException);

        // Cleanup
        transactionB.EndTransaction();
    }

    [Test]
    public void AbortTransaction_AbortPreviousTransaction_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        using var transactionA = service.BeginTransaction();
        transactionA.EndTransaction();
        using var transactionB = service.BeginTransaction();

        // Assert
        Assert.That(() => transactionA.AbortTransaction(), Throws.InvalidOperationException);

        // Cleanup
        transactionB.EndTransaction();
    }

    [Test]
    public void Dispose_ForgettingToEndOrAbort_AbortsAutomatically()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        using (var transaction = service.BeginTransaction())
        {
            // Transaction should be aborted automatically
        }

        // Assert
        Assert.That(() => service.BeginTransaction(), Throws.Nothing);
    }

    [Test]
    public void Dispose_ExceptionThrownInTransaction_AbortsAutomatically()
    {
        // Arrange
        var service = this.CreateService();

        // Act
        Assert.That(() =>
        {
            using (var transaction = service.BeginTransaction())
            {
                throw new Exception("Test exception");
            }
        }, Throws.Exception);

        // Assert
        Assert.That(() => service.BeginTransaction(), Throws.Nothing);
    }

    [Test]
    public void CollectToSlot_WoodToEmptySlot_ReturnsSuccess()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();

        // Act
        var (collected, result) = transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That((collected, result), Is.EqualTo((1, OperationResult.Success)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock }));
    }

    [Test]
    public void CollectToSlot_SlotIndexOutOfRange_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(container, WoodBlock, slot: 1),
            Throws.TypeOf<SlotIndexOutOfRangeException>()
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void CollectToSlot_NotStackable_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, ConcreteBlock, slot: 0);

        // Act
        var (collected, result) = transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That((collected, result), Is.EqualTo((0, OperationResult.NotStackable)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { ConcreteBlock }));
    }

    [Test]
    public void CollectToSlot_NotEnoughSpace_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);

        // Act
        var (collected, result) = transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That((collected, result), Is.EqualTo((0, OperationResult.NotEnoughSpace)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock }));
    }

    [Test]
    public void CollectToSlot_NotEnoughSpaceForAll_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 6);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);

        // Act
        var (collected, result) = transaction.CollectToSlot(
            container,
            WoodBlock with { Quantity = 3 },
            slot: 0,
            mustCollectAll: true
        );
        transaction.EndTransaction();

        // Assert
        Assert.That((collected, result), Is.EqualTo((0, OperationResult.NotEnoughSpaceForAll)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 4 } }));
    }

    [Test]
    public void CollectToSlot_MustCollectAllFalse_ReturnsSuccess()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 6);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);

        // Act
        var (collected, result) = transaction.CollectToSlot(
            container,
            WoodBlock with { Quantity = 3 },
            slot: 0,
            mustCollectAll: false
        );
        transaction.EndTransaction();

        // Assert
        Assert.That((collected, result), Is.EqualTo((2, OperationResult.Success)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 6 } }));
    }

    [Test]
    public void EndTransaction_NormalUsage_UpdatesItems()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);

        // Act
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock }));
    }

    [Test]
    public void EndTransaction_ContainerRemovedBeforeEnd_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);

        // Act
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);
        service.RemoveContainer(container.Id);

        // Assert
        Assert.That(() => transaction.EndTransaction(), Throws.InvalidOperationException);

        // Cleanup
        transaction.AbortTransaction();
    }

    [Test]
    public void Move_SlotIndexOutOfRange_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1);
        var containerTo = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(
            () => transaction.Move(containerFrom, slotFrom: 1, containerTo, slotTo: 0, quantity: 1),
            Throws.TypeOf<SlotIndexOutOfRangeException>()
        );
        Assert.That(
            () => transaction.Move(containerFrom, slotFrom: 0, containerTo, slotTo: 1, quantity: 1),
            Throws.TypeOf<SlotIndexOutOfRangeException>()
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void Move_NotStackable_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 2);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.CollectToSlot(container, ConcreteBlock, slot: 1);

        // Act
        var (moved, result) = transaction.Move(container, slotFrom: 0, container, slotTo: 1, quantity: 1);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((0, OperationResult.NotStackable)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock, ConcreteBlock }));
    }

    [Test]
    public void Move_FromEmptySlot_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 2);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);

        // Act
        var (moved, result) = transaction.Move(container, slotFrom: 1, container, slotTo: 0, quantity: 1);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((0, OperationResult.Success)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock, ItemStack.Empty }));
    }

    [Test]
    public void Move_NotEnoughSpace_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 2, maximumStackSize: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.CollectToSlot(container, WoodBlock, slot: 1);

        // Act
        var (moved, result) = transaction.Move(container, slotFrom: 0, container, slotTo: 1, quantity: 1);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((0, OperationResult.NotEnoughSpaceForAll)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock, WoodBlock }));
    }

    [Test]
    public void Move_SameSlotMustMoveAllTrue_ReturnsNotEnoughSpaceForAll()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 6);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);

        // Act
        var (moved, result) = transaction.Move(container, slotFrom: 0, container, slotTo: 0, quantity: 5, mustMoveAll: true);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((0, OperationResult.NotEnoughSpaceForAll)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 4 } }));
    }

    [Test]
    public void Move_SameSlotMustMoveAllFalse_ReturnsSuccess()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 6);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);

        // Act
        var (moved, result) = transaction.Move(container, slotFrom: 0, container, slotTo: 0, quantity: 5, mustMoveAll: false);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((4, OperationResult.Success)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 4 } }));
    }

    [Test]
    public void Move_MergeItemStacksSameContainer_ReturnsSuccess()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 2, maximumStackSize: 6);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 2 }, slot: 1);

        // Act
        var (moved, result) = transaction.Move(container, slotFrom: 1, container, slotTo: 0, quantity: 2);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((2, OperationResult.Success)));
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 6 }, ItemStack.Empty }));
    }

    [Test]
    public void Move_MergeItemStacksDifferentContainers_ReturnsSuccess()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1, maximumStackSize: 6);
        var containerTo = service.CreateContainer(size: 1, maximumStackSize: 6);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, WoodBlock with { Quantity = 4 }, slot: 0);
        transaction.CollectToSlot(containerTo, WoodBlock with { Quantity = 2 }, slot: 0);

        // Act
        var (moved, result) = transaction.Move(containerFrom, slotFrom: 0, containerTo, slotTo: 0, quantity: 2);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((2, OperationResult.Success)));
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 2 } }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 4 } }));
    }

    [Test]
    public void Move_EntireStack_SetsFromToEmpty()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1);
        var containerTo = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, WoodBlock with { Quantity = 4 }, slot: 0);

        // Act
        var (moved, result) = transaction.Move(containerFrom, slotFrom: 0, containerTo, slotTo: 0, quantity: 4);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((4, OperationResult.Success)));
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { ItemStack.Empty }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 4 } }));
    }

    [Test]
    public void Move_VeryLargeStackMustCollectAllTrue_DoesNotOverflow()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1);
        var containerTo = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, WoodBlock with { Quantity = ushort.MaxValue }, slot: 0);
        transaction.CollectToSlot(containerTo, WoodBlock with { Quantity = 1 }, slot: 0);

        // Act
        var (moved, result) = transaction.Move(containerFrom, slotFrom: 0, containerTo, slotTo: 0, quantity: ushort.MaxValue);
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((0, OperationResult.NotEnoughSpaceForAll)));
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = ushort.MaxValue } }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 1 } }));
    }

    [Test]
    public void Move_VeryLargeStackMustCollectAllFalse_DoesNotOverflow()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1);
        var containerTo = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, WoodBlock with { Quantity = ushort.MaxValue }, slot: 0);
        transaction.CollectToSlot(containerTo, WoodBlock with { Quantity = 1 }, slot: 0);

        // Act
        var (moved, result) = transaction.Move(
            containerFrom,
            slotFrom: 0,
            containerTo,
            slotTo: 0,
            quantity: ushort.MaxValue,
            mustMoveAll: false
        );
        transaction.EndTransaction();

        // Assert
        Assert.That((moved, result), Is.EqualTo((ushort.MaxValue - 1, OperationResult.Success)));
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 1 } }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = ushort.MaxValue } }));
    }

    [Test]
    public void Swap_SlotIndexOutOfRange_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1);
        var containerTo = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(
            () => transaction.Swap(containerFrom, slotFrom: 1, containerTo, slotTo: 0),
            Throws.TypeOf<SlotIndexOutOfRangeException>()
        );
        Assert.That(
            () => transaction.Swap(containerFrom, slotFrom: 0, containerTo, slotTo: 1),
            Throws.TypeOf<SlotIndexOutOfRangeException>()
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void Swap_ToMaximumStackSizeTooSmall_ReturnsFalse()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1, maximumStackSize: 2);
        var containerTo = service.CreateContainer(size: 1, maximumStackSize: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, WoodBlock with { Quantity = 2 }, slot: 0);
        transaction.CollectToSlot(containerTo, ConcreteBlock, slot: 0);

        // Act
        var result = transaction.Swap(containerFrom, slotFrom: 0, containerTo, slotTo: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 2 } }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { ConcreteBlock }));
    }

    [Test]
    public void Swap_FromMaximumStackSizeTooSmall_ReturnsFalse()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1, maximumStackSize: 1);
        var containerTo = service.CreateContainer(size: 1, maximumStackSize: 2);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, ConcreteBlock, slot: 0);
        transaction.CollectToSlot(containerTo, WoodBlock with { Quantity = 2 }, slot: 0);

        // Act
        var result = transaction.Swap(containerFrom, slotFrom: 0, containerTo, slotTo: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That(result, Is.False);
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { ConcreteBlock }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { WoodBlock with { Quantity = 2 } }));
    }

    [Test]
    public void Swap_SameSlot_ReturnsTrue()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);

        // Act
        var result = transaction.Swap(container, slotFrom: 0, container, slotTo: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { WoodBlock }));
    }

    [Test]
    public void Swap_SameContainer_ReturnsTrue()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 2);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);
        transaction.CollectToSlot(container, ConcreteBlock, slot: 1);

        // Act
        var result = transaction.Swap(container, slotFrom: 0, container, slotTo: 1);
        transaction.EndTransaction();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(container.Items, Is.EqualTo(new ItemStack[] { ConcreteBlock, WoodBlock }));
    }

    [Test]
    public void Swap_DifferentContainers_ReturnsTrue()
    {
        // Arrange
        var service = this.CreateService();
        var containerFrom = service.CreateContainer(size: 1);
        var containerTo = service.CreateContainer(size: 1);
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(containerFrom, WoodBlock, slot: 0);
        transaction.CollectToSlot(containerTo, ConcreteBlock, slot: 0);

        // Act
        var result = transaction.Swap(containerFrom, slotFrom: 0, containerTo, slotTo: 0);
        transaction.EndTransaction();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(containerFrom.Items, Is.EqualTo(new ItemStack[] { ConcreteBlock }));
        Assert.That(containerTo.Items, Is.EqualTo(new ItemStack[] { WoodBlock }));
    }
}
