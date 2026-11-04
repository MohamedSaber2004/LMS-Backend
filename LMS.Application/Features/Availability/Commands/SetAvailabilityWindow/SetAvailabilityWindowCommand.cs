using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Availability.Commands.SetAvailabilityWindow
{
    public class SetAvailabilityWindowCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }

        public bool Open { get; set; }
    }
}
