using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Availability.Commands.SetVacationMode
{
    public class SetVacationModeCommand : IRequest<Result<string>>
    {
        public bool Enabled { get; set; }
    }
}
