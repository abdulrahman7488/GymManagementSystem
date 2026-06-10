using AutoMapper;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.PlanViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.GetRepository<Plan>().GetALLAsync(ct: ct);
            return _mapper.Map<IEnumerable<PlanViewModel>>(plans);
        }
        public async Task<PlanViewModel?> GetPlanByIdAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            return plan is null ? null : _mapper.Map<PlanViewModel>(plan);
        }

        public async Task<UpdatePlanViewModel?> GetPlanToUpdateAsync(int planId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.GetRepository<Plan>().GetByIdAsync(planId, ct);
            if (plan is null || !plan.IsActive) return null;
            
            return _mapper.Map<UpdatePlanViewModel>(plan);
        }

        public async Task<bool> ToggleActivationAsync(int planId, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<Plan>();
            var plan = await repo.GetByIdAsync(planId, ct);
            if (plan is null) return false;

            if (plan.IsActive && await HasActiveMembershipsAsync(planId, ct))
                return false;

            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.Now;
            repo.UpdateAsync(plan);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }
        public async Task<bool> UpdatePlanAsync(int id, UpdatePlanViewModel model, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<Plan>();
            var plan = await repo.GetByIdAsync(id, ct);
            if (plan is null) return false;
            if (await HasActiveMembershipsAsync(id, ct))
                return false;

            _mapper.Map(model, plan);
            plan.UpdatedAt = DateTime.Now;
            repo.UpdateAsync(plan);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        #region Helper Methods

        private async Task<bool> HasActiveMembershipsAsync(int planId, CancellationToken ct)
        {
            return await _unitOfWork.GetRepository<MemberShip>().AnyAsync(m => m.PlanId == planId && m.EndDate > DateTime.Now, ct);
        }

        #endregion
    }

}
