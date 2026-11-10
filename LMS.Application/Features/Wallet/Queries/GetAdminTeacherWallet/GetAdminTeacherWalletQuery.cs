using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Wallet.Queries.GetAdminTeacherWallet
{
    public class GetAdminTeacherWalletQuery : IRequest<Result<TeacherWalletSummaryDto>>
    {
        public Guid InstructorId { get; set; }
    }

    public class GetAdminTeacherWalletQueryHandler : IRequestHandler<GetAdminTeacherWalletQuery, Result<TeacherWalletSummaryDto>>
    {
        private readonly ILmsDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetAdminTeacherWalletQueryHandler(
            ILmsDbContext context,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        public async Task<Result<TeacherWalletSummaryDto>> Handle(
            GetAdminTeacherWalletQuery request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.InstructorId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var wallet = await _context.TeacherWallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.InstructorId == instructor.Id, cancellationToken);

            var dto = new TeacherWalletSummaryDto
            {
                WalletId = wallet?.Id ?? Guid.Empty,
                InstructorId = instructor.Id,
                AvailableBalance = wallet?.AvailableBalance ?? 0m,
                PendingBalance = wallet?.PendingBalance ?? 0m,
                TotalEarned = wallet?.TotalEarned ?? 0m,
                CommissionRate = instructor.CommissionRate
            };

            return Result<TeacherWalletSummaryDto>.Success(
                dto,
                _localizer[LocalizationKeys.Wallets.SummaryRetrieved].Value);
        }
    }
}

