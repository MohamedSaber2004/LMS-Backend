using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Queries.GetInstructorsForLearner
{
    public class GetInstructorsForLearnerQuery : IRequest<Result<PagginatedResult<InstructorCardDto>>>
    {
        public GetInstructorsForLearnerQuery() { }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Specialization { get; set; }
        public Guid? CountryId { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<InstructorCardDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<InstructorCardDto>.DefaultPageSize;
    }
}
