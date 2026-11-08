using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Commands.SetTeacherVacationMode
{
    public class SetTeacherVacationModeCommand : IRequest<Result<TeacherVacationModeResultDto>>
    {
        public bool Enabled { get; set; }
    }
}
