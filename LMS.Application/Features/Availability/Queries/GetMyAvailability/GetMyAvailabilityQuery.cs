using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Queries.GetMyAvailability
{
    public class GetMyAvailabilityQuery : IRequest<Result<MyAvailabilityDto>>
    {
        public GetMyAvailabilityQuery() { }
    }
}
