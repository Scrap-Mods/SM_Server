using NUnit.Framework;
using ScrapServer.Core;

namespace ScrapServer.CoreTests.Services;

[TestFixture]
public class ContainerServiceTests
{
    private ContainerService CreateService()
    {
        return new ContainerService();
    }

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
}
