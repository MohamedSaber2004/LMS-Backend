using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using MediatR;

namespace LMS.Application.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQuery : IRequest<Result<CountryDto>>
    {
        public Guid Id { get; set; }
    }
}
