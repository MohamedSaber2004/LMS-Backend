using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Countries.Commands.DeleteCountry
{
    public class DeleteCountryCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
