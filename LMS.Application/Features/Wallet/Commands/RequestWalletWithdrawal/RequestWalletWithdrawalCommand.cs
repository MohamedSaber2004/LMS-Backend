using FluentValidation;
using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Wallet.Commands.RequestWalletWithdrawal
{
    public class RequestWalletWithdrawalCommand : IRequest<Result<TeacherWalletSummaryDto>>
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class RequestWalletWithdrawalCommandValidator : AbstractValidator<RequestWalletWithdrawalCommand>
    {
        public RequestWalletWithdrawalCommandValidator(IStringLocalizer<Messages> localizer)
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage(localizer[LocalizationKeys.Wallets.WithdrawalAmountMustBePositive].Value);
        }
    }

    public class RequestWalletWithdrawalCommandHandler : IRequestHandler<RequestWalletWithdrawalCommand, Result<TeacherWalletSummaryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public RequestWalletWithdrawalCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<TeacherWalletSummaryDto>> Handle(
            RequestWalletWithdrawalCommand request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == _currentUserService.UserId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var wallet = await _context.TeacherWallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.InstructorId == instructor.Id, cancellationToken);

            if (wallet == null)
            {
                wallet = TeacherWallet.Create(instructor.Id);
                await _context.TeacherWallets.AddAsync(wallet, cancellationToken);
            }

            if (request.Amount > wallet.AvailableBalance)
            {
                throw new BadRequestException(LocalizationKeys.Wallets.WithdrawalAmountExceedsBalance);
            }

            wallet.WithdrawEarnings(request.Amount, request.Note);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = new TeacherWalletSummaryDto
            {
                WalletId = wallet.Id,
                InstructorId = instructor.Id,
                AvailableBalance = wallet.AvailableBalance,
                PendingBalance = wallet.PendingBalance,
                TotalEarned = wallet.TotalEarned,
                CommissionRate = instructor.CommissionRate
            };

            return Result<TeacherWalletSummaryDto>.Success(
                dto,
                _localizer[LocalizationKeys.Wallets.WithdrawalSuccess].Value);
        }
    }
}

