using LMS.Domain.Entities;
using LMS.Domain.Repositories.Interfaces;
using LMS.Infrastructure.Repositories.Implementations.Base;
using LMS.Persistence;

namespace LMS.Infrastructure.Repositories.Implementations
{
    public class UserRefreshTokenRepository : GenericRepository<UserRefreshToken, Guid>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(LmsDbContext context) : base(context)
        {
        }
    }
}
