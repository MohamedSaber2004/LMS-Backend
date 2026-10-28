using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.DeleteAccount
{
    public class DeleteAccountCommand : IRequest<Result<string>>
    {
        public DeleteAccountCommand() { }

        public DeleteAccountCommand(string password)
        {
            Password = password;
        }

        public string Password { get; set; } = string.Empty;
    }
}
