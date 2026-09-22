using EmerisAcademicSuccess.Application.Interfaces;

namespace EmerisAcademicSuccess.Infrastructure.Services;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.Now;
}