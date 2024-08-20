using NUnit.Framework;
using ScrapServer.Core;
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

        // Act
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(container, WoodBlock, slot: 0),
            Is.EqualTo((1, ContainerService.Transaction.OperationResult.Success))
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void CollectToSlot_SlotIndexOutOfRange_ThrowsException()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1);

        // Act
        using var transaction = service.BeginTransaction();

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(container, WoodBlock, slot: 1),
            Throws.TypeOf<ContainerService.Transaction.SlotIndexOutOfRangeException>()
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

        // Act
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, ConcreteBlock, slot: 0);

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(container, WoodBlock, slot: 0),
            Is.EqualTo((0, ContainerService.Transaction.OperationResult.NotStackable))
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void CollectToSlot_NotEnoughSpace_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 1);

        // Act
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock, slot: 0);

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(container, WoodBlock, slot: 0),
            Is.EqualTo((0, ContainerService.Transaction.OperationResult.NotEnoughSpace))
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void CollectToSlot_NotEnoughSpaceForAll_ReturnsZero()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 6);

        // Act
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(
                container,
                WoodBlock with { Quantity = 3 },
                slot: 0,
                mustCollectAll: true
            ),
            Is.EqualTo((0, ContainerService.Transaction.OperationResult.NotEnoughSpaceForAll))
        );

        // Cleanup
        transaction.EndTransaction();
    }

    [Test]
    public void CollectToSlot_MustCollectAllFalse_ReturnsSuccess()
    {
        // Arrange
        var service = this.CreateService();
        var container = service.CreateContainer(size: 1, maximumStackSize: 6);

        // Act
        using var transaction = service.BeginTransaction();
        transaction.CollectToSlot(container, WoodBlock with { Quantity = 4 }, slot: 0);

        // Assert
        Assert.That(
            () => transaction.CollectToSlot(
                container,
                WoodBlock with { Quantity = 3 },
                slot: 0,
                mustCollectAll: false
            ),
            Is.EqualTo((2, ContainerService.Transaction.OperationResult.Success))
        );

        // Cleanup
        transaction.EndTransaction();
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
}
