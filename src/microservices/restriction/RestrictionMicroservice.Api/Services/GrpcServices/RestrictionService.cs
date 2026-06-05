using Grpc.Core;
using RestrictionGrpcService;
using RestrictionMicroservice.Api.Services.UnitOfWork;

namespace RestrictionMicroservice.Api.Services.GrpcServices
{
    public class RestrictionService(IUnitOfWork unitOfWork) : RestrictionInfo.RestrictionInfoBase
    {
        public override async Task<GetRestrictionInfoReply> GetRestrictionInfo(GetRestrictionInfoRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
                throw new ArgumentException("User id cannot be converted into Guid type");

            var activeRestriction = await unitOfWork.RestrictionRepository.GetActiveRestrictionByRestrictedUserIdAsync(userId);
            if (activeRestriction == null)
                return new GetRestrictionInfoReply { RestrictionType = RestrictionType.NoRestrictions };

            var getRestrictionInfoReply = new GetRestrictionInfoReply();
            switch (activeRestriction.RestrictionType)
            {
                case Enums.RestrictionType.All:
                    getRestrictionInfoReply.RestrictionType = RestrictionType.All;
                    break;
                case Enums.RestrictionType.ReviewPosting:
                    getRestrictionInfoReply.RestrictionType = RestrictionType.ReviewPosting;
                    break;
                case Enums.RestrictionType.Commenting:
                    getRestrictionInfoReply.RestrictionType = RestrictionType.Commenting;
                    break;
                case Enums.RestrictionType.Reporting:
                    getRestrictionInfoReply.RestrictionType = RestrictionType.Reporting;
                    break;
            }

            return getRestrictionInfoReply;
        }
    }
}
