using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Availability.Commands.Admin.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
