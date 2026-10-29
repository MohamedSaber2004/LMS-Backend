using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using MediatR;

namespace LMS.Application.Features.Countries.Commands.AddCountry
{
    public class AddCountryCommand : IRequest<Result<CountryDto>>
    {
        public string CountryEnName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
