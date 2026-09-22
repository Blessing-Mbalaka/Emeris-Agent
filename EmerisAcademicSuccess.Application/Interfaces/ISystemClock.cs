namespace EmerisAcademicSuccess.Application.Interfaces;

public interface ISystemClock
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
}