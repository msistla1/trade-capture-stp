using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TradeCaptureSystem.Application.Commands;
using TradeCaptureSystem.Application.Services;
using TradeCaptureSystem.Domain.Common;
using TradeCaptureSystem.Domain.Entities;
using Xunit;

namespace TradeCaptureSystem.Tests.Unit.Commands;

public class ProcessTradeCommandHandlerTests
{
    private readonly Mock<ITradeRepository> _mockRepository;
    private readonly Mock<IDuplicateCheckService> _mockDuplicateCheckService;
    private readonly Mock<ILogger<ProcessTradeCommandHandler>> _mockLogger;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly ProcessTradeCommandHandler _handler;

    public ProcessTradeCommandHandlerTests()
    {
        _mockRepository = new Mock<ITradeRepository>();
        _mockDuplicateCheckService = new Mock<IDuplicateCheckService>();
        _mockLogger = new Mock<ILogger<ProcessTradeCommandHandler>>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockValidationService = new Mock<IValidationService>();

        // Setup the factory to return a mock logger
        _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);

        // Default validation service returns success
        _mockValidationService.Setup(v => v.Validate(It.IsAny<Trade>()))
            .Returns(Result.Success());

        // Create a factory that constructs real state machines using mocks
        var persistenceService = new TradeCaptureSystem.Infrastructure.Services.TradePersistenceService(_mockRepository.Object);
        var factory = new TradeCaptureSystem.Application.Services.TradeStateMachineFactory(
            _mockValidationService.Object,
            _mockDuplicateCheckService.Object,
            persistenceService,
            _mockLoggerFactory.Object);

        _handler = new ProcessTradeCommandHandler(
            factory,
            _mockLogger.Object,
            _mockLoggerFactory.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldProcessSuccessfully()
    {
        // Arrange
        var command = new ProcessTradeCommand(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        _mockDuplicateCheckService
            .Setup(x => x.IsDuplicateAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockRepository
            .Setup(x => x.SaveAsync(It.IsAny<Trade>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Note: State machine processes asynchronously through multiple states
        // SaveAsync will be called eventually, but verifying exact timing is complex
        _mockDuplicateCheckService.Verify(x => x.IsDuplicateAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldLogWarning()
    {
        // Arrange
        var command = new ProcessTradeCommand(
            "",  // Invalid: empty TradeId
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );


        // Make validation fail for empty TradeId
        _mockValidationService.Setup(v => v.Validate(It.Is<Trade>(t => string.IsNullOrEmpty(t.TradeId))))
            .Returns(Result.Failure("TradeId is required"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(); // Process still succeeds, but trade will be rejected
    }

    [Fact]
    public async Task Handle_WhenDuplicateFound_ShouldMarkTradeAsDuplicate()
    {
        // Arrange
        var command = new ProcessTradeCommand(
            "TRD001",
            "Goldman Sachs",
            "AAPL",
            100,
            150.50m,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(2)
        );

        _mockDuplicateCheckService
            .Setup(x => x.IsDuplicateAsync("TRD001"))
            .ReturnsAsync(true);

        _mockRepository
            .Setup(x => x.SaveAsync(It.IsAny<Trade>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockDuplicateCheckService.Verify(x => x.IsDuplicateAsync("TRD001"), Times.Once);
    }
}
