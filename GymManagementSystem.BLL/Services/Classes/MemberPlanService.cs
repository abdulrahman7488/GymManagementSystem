using GymManagementSystem.BLL.Common;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.MemberPlanViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class MemberPlanService : IMemberPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MemberPlanService(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<IEnumerable<MemberPlanViewModel>> GetAllMembershipsAsync(CancellationToken ct = default)
        {
            var memberships = await _unitOfWork.GetRepository<MemberShip>().GetALLAsync(ct: ct);
            var members = await _unitOfWork.GetRepository<Member>().GetALLAsync(ct: ct);
            var plans = await _unitOfWork.GetRepository<Plan>().GetALLAsync(ct: ct);

            return memberships.Select(m => new MemberPlanViewModel
            {
                Id = m.Id,
                MemberId = m.MemberId,
                MemberName = members.FirstOrDefault(x => x.Id == m.MemberId)?.Name ?? "Unknown",
                PlanId = m.PlanId,
                PlanName = plans.FirstOrDefault(x => x.Id == m.PlanId)?.Name ?? "Unknown",
                StartDate = m.CreatedAt.ToShortDateString(),
                EndDate = m.EndDate.ToShortDateString(),
                Status = m.Status,
                IsActive = m.IsActive,
            });
        }

        // ── GET CREATE FORM ──────────────────────────────────────
        public async Task<CreateMemberPlanViewModel> GetCreateFormAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.GetRepository<Member>().GetALLAsync(ct: ct);
            var plans = await _unitOfWork.GetRepository<Plan>().GetALLAsync(ct: ct);

            return new CreateMemberPlanViewModel
            {
                Members = members.Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name }),
                Plans = plans.Where(p => p.IsActive)
                               .Select(p => new SelectListItem
                               {
                                   Value = p.Id.ToString(),
                                   Text = $"{p.Name} ({p.DurationInDays} days – {p.Price:C})"
                               })
            };
        }

        // ── ASSIGN PLAN ──────────────────────────────────────────
        public async Task<Result> AssignPlanAsync(int memberId, int planId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);
            if (member is null) return Result.NotFound("Member not found.");

            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if (plan is null) return Result.NotFound("Plan not found.");
            if (!plan.IsActive) return Result.Validation("Cannot assign an inactive plan.");

            var hasActive = await _unitOfWork.GetRepository<MemberShip>()
                .AnyAsync(m => m.MemberId == memberId && m.EndDate > DateTime.Now, ct);
            if (hasActive) return Result.Fail("Member already has an active membership.");

            var membership = new MemberShip
            {
                MemberId = memberId,
                PlanId = planId,
                EndDate = DateTime.Now.AddDays(plan.DurationInDays)
            };

            await _unitOfWork.GetRepository<MemberShip>().AddAsync(membership);
            return Result.Ok();
        }

        // ── CANCEL MEMBERSHIP ────────────────────────────────────
        public async Task<Result> CancelMembershipAsync(int membershipId, CancellationToken ct = default)
        {
            var membership = await _unitOfWork.GetRepository<MemberShip>().GetByIdAsync(membershipId, ct);
            if (membership is null) return Result.NotFound("Membership not found.");
            if (!membership.IsActive) return Result.Validation("Only active memberships can be cancelled.");

            await _unitOfWork.GetRepository<MemberShip>().DeleteAsync(membership);
            return Result.Ok();
        }
    }
}
