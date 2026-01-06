using FluentAssertions;
using TradeCaptureSystem.Domain.Entities;
using TradeCaptureSystem.Domain.Rules;
using Xunit;

namespace TradeCaptureSystem.Tests.Unit.Rules;

public class RequiredFieldsRuleTests
{
    private readonly RequiredFieldsRule _rule;

    public RequiredFieldsRuleTests()
    {
        _rule = new RequiredFieldsRule();
    }

    [Fact]
    public void Validate_WithValidTrade_ShouldReturnSuccess()
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
        var result = _rule.Validate(trade);

        // Assert
        result.IsSuccess.Should().BeTrue();
        trade.ValidationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Goldman Sachs", "AAPL", 100, 150.50)]
    [InlineData("TRD001", "", "AAPL", 100, 150.50)]
    [InlineData("TRD001", "Goldman Sachs", "", 100, 150.50)]
    public void Validate_WithMissingRequiredFields_ShouldReturnFailure(
        string tradeId, string counterparty, string instrument, decimal quantity, decimal price)
    {
        // Arrange
        var trade = new Trade(
            tradeId,
            counterparty,
            instrument,
            quantity,
            price,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        var result = _rule.Validate(trade);

        // Assert
        result.IsFailure.Should().BeTrue();
        trade.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_WithZeroQuantity_ShouldReturnFailure()
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
        var result = _rule.Validate(trade);

        // Assert
        result.IsFailure.Should().BeTrue();
        trade.ValidationErrors.Should().Contain(e => e.Contains("Quantity"));
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldReturnFailure()
    {
        // Arrange
        var trade = new Trade(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            0,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        // Act
        var result = _rule.Validate(trade);

        // Assert
        result.IsFailure.Should().BeTrue();
        trade.ValidationErrors.Should().Contain(e => e.Contains("Price"));
    }
}
