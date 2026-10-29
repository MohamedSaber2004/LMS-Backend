using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using MediatR;

namespace LMS.Application.Features.Countries.Queries.GetAllCountries
{
    public class GetAllCountriesQuery : IRequest<Result<IReadOnlyCollection<CountryDto>>>
    {
    }
}
