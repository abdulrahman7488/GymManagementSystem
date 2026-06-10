using GymManagementSystem.BLL.Common;
using GymManagementSystem.BLL.ViewModels.MemberPlanViewModels;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IMemberPlanService
    {
        Task<IEnumerable<MemberPlanViewModel>> GetAllMembershipsAsync(CancellationToken ct = default);
        Task<CreateMemberPlanViewModel>        GetCreateFormAsync(CancellationToken ct = default);
        Task<Result> AssignPlanAsync(int memberId, int planId, CancellationToken ct = default);
        Task<Result> CancelMembershipAsync(int membershipId, CancellationToken ct = default);
    }
}
