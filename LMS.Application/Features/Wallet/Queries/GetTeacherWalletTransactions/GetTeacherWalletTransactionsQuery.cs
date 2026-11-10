using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Wallet.Queries.GetTeacherWalletTransactions
{
    public class GetTeacherWalletTransactionsQuery : IRequest<Result<PagginatedResult<TeacherWalletTransactionDto>>>
    {
        public int PageNumber { get; set; } = PagginatedResult<TeacherWalletTransactionDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<TeacherWalletTransactionDto>.DefaultPageSize;
    }

    public class GetTeacherWalletTransactionsQueryHandler : IRequestHandler<GetTeacherWalletTransactionsQuery, Result<PagginatedResult<TeacherWalletTransactionDto>>>
    {
        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<Messages> _localizer;

        public GetTeacherWalletTransactionsQueryHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        public async Task<Result<PagginatedResult<TeacherWalletTransactionDto>>> Handle(
            GetTeacherWalletTransactionsQuery request,
            CancellationToken cancellationToken)
        {
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == _currentUserService.UserId, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Availability.InstructorNotFound);

            var wallet = await _context.TeacherWallets
                .FirstOrDefaultAsync(w => w.InstructorId == instructor.Id, cancellationToken);

            if (wallet == null)
            {
                return Result<PagginatedResult<TeacherWalletTransactionDto>>.Success(
                    PagginatedResult<TeacherWalletTransactionDto>.Create(Array.Empty<TeacherWalletTransactionDto>(), 0, request.PageNumber, request.PageSize),
                    _localizer[LocalizationKeys.Wallets.NoTransactions].Value);
            }

            var query = _context.TeacherWalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TeacherWalletTransactionDto
                {
                    Id = t.Id,
                    SessionId = t.SessionId,
                    Amount = t.Amount,
                    GrossAmount = t.GrossAmount,
                    CommissionAmount = t.CommissionAmount,
                    CommissionRate = t.CommissionRate,
                    Type = t.Type,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<PagginatedResult<TeacherWalletTransactionDto>>.Success(
                PagginatedResult<TeacherWalletTransactionDto>.Create(items, totalCount, pageNumber, pageSize),
                _localizer[LocalizationKeys.Wallets.TransactionsRetrieved].Value);
        }
    }
}

