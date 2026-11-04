using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Availability.Commands.SetAvailabilityDay
{
    public class SetAvailabilityDayCommand : IRequest<Result<string>>
    {
        public DateOnly Date { get; set; }

        public bool Open { get; set; }
    }
}
