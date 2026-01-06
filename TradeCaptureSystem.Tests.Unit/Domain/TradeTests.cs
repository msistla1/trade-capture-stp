using FluentAssertions;
using TradeCaptureSystem.Domain.Entities;
using TradeCaptureSystem.Domain.Enums;
using Xunit;

namespace TradeCaptureSystem.Tests.Unit.Domain;

public class TradeTests
{
    [Fact]
    public void Trade_WhenCreated_ShouldHaveReceivedState()
    {
        // Arrange & Act
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Assert
        trade.CurrentState.Should().Be(TcrState.Received);
        trade.TradeId.Should().Be("TRD001");
        trade.Counterparty.Should().Be("Goldman Sachs");
        trade.Instrument.Should().Be("AAPL");
    }

    [Fact]
    public void HasRequiredFields_WhenAllFieldsPresent_ShouldReturnTrue()
    {
        // Arrange
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        var result = trade.HasRequiredFields();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasRequiredFields_WhenQuantityIsZero_ShouldReturnFalse()
    {
        // Arrange
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            0,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        var result = trade.HasRequiredFields();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateState_ShouldUpdateCurrentStateAndTimestamp()
    {
        // Arrange
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        trade.UpdateState(TcrState.ValidationInProgress);

        // Assert
        trade.CurrentState.Should().Be(TcrState.ValidationInProgress);
        trade.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsDuplicate_ShouldSetIsDuplicateToTrue()
    {
        // Arrange
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        trade.MarkAsDuplicate();

        // Assert
        trade.IsDuplicate.Should().BeTrue();
    }

    [Fact]
    public void AddValidationError_ShouldAddErrorToList()
    {
        // Arrange
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        trade.AddValidationError("Price is invalid");

        // Assert
        trade.ValidationErrors.Should().Contain("Price is invalid");
        trade.ValidationErrors.Should().HaveCount(1);
    }
}
