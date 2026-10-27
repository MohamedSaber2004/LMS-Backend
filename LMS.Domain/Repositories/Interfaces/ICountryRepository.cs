using LMS.Domain.Entities;
using LMS.Domain.Repositories.Interfaces.Base;

namespace LMS.Domain.Repositories.Interfaces
{
    public interface ICountryRepository : IGenericRepository<Country, Guid>
    {
    }
}
