using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.ScheduleLessonSession
{
    public class ScheduleLessonSessionCommand : IRequest<Result<LessonAdminDto>>
    {
        public Guid LessonId { get; set; }
        public Guid InstructorId { get; set; }
        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
        public int SeatCapacity { get; set; } = 50;
    }
}
