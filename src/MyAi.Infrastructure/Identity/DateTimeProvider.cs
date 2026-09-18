using MyAi.Application.Common.Interfaces;

namespace MyAi.Infrastructure.Identity;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
