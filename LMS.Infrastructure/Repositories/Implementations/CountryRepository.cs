using LMS.Domain.Entities;
using LMS.Domain.Repositories.Interfaces;
using LMS.Infrastructure.Repositories.Implementations.Base;
using LMS.Persistence;

namespace LMS.Infrastructure.Repositories.Implementations
{
    public class CountryRepository : GenericRepository<Country, Guid>, ICountryRepository
    {
        public CountryRepository(LmsDbContext context) : base(context)
        {
        }
    }
}
