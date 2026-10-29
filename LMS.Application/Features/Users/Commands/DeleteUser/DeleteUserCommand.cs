using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
