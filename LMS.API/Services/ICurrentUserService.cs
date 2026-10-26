namespace LMS.API.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
        string? IpAddress { get; }
        int? UserTypes { get; }
        string CorrelationId { get; }
        string? Email { get; }
        string? FullName { get; }
        string? Device { get; }
    }
}
