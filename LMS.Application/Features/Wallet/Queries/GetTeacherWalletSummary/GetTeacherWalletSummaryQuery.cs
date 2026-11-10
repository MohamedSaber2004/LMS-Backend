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

namespace LMS.Application.Features.Wallet.Queries.GetTeacherWalletSummary
{
    public class GetTeacherWalletSummaryQuery : IRequest<Result<TeacherWalletSummaryDto>>
    {
    }

    public class GetTeacherWalletSummaryQueryHandler : IRequestHandler<GetTeacherWalletSummaryQuery, Result<TeacherWalletSummaryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetTeacherWalletSummaryQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<TeacherWalletSummaryDto>> Handle(
            GetTeacherWalletSummaryQuery request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == _currentUserService.UserId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var wallet = await _context.TeacherWallets
                .FirstOrDefaultAsync(w => w.InstructorId == instructor.Id, cancellationToken);

            if (wallet == null)
            {
                wallet = TeacherWallet.Create(instructor.Id);
                await _context.TeacherWallets.AddAsync(wallet, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

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
                _localizer[LocalizationKeys.Wallets.SummaryRetrieved].Value);
        }
    }
}

