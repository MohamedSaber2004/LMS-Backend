using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using MediatR;

namespace LMS.Application.Features.Countries.Queries.GetAllCountriesPagginated
{
    public class GetAllCountriesPagginatedQuery : IRequest<Result<PagginatedResult<CountryDto>>>
    {
        public int PageNumber { get; set; } = PagginatedResult<CountryDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<CountryDto>.DefaultPageSize;
    }
}
