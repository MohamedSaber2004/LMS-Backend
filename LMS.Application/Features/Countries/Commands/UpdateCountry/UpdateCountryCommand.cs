using LMS.Application.Common.Models;
using LMS.Application.Features.Countries.DTOs;
using MediatR;

namespace LMS.Application.Features.Countries.Commands.UpdateCountry
{
    public class UpdateCountryCommand : IRequest<Result<CountryDto>>
    {
        public Guid Id { get; set; }
        public string CountryEnName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
