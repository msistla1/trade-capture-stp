namespace TradeCaptureSystem.Domain.Enums;

/// <summary>
/// Represents the possible states in the trade capture workflow
/// </summary>
public enum TcrState
{
    Received,
    ValidationInProgress,
    DuplicateCheckInProgress,
    ReadyForProcessing,
    Created,
    Updated,
    Rejected,
    Saved,
    Failed
}
