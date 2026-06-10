
using GymManagementSystem.BLL.ViewModels.PlanViewModels;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanViewModel>> GetAllPlansAsync(CancellationToken ct = default);
        Task<PlanViewModel?> GetPlanByIdAsync(int planId, CancellationToken ct = default);
        Task<UpdatePlanViewModel?> GetPlanToUpdateAsync(int planId, CancellationToken ct = default);

        Task<bool> ToggleActivationAsync(int planId, CancellationToken ct = default);
        Task<bool> UpdatePlanAsync(int id, UpdatePlanViewModel model, CancellationToken ct = default);
    }
}
