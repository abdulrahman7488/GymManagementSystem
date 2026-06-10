

using GymManagementSystem.BLL.ViewModels.TrainerViewModels;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface ITrainerService
    {
        Task<IEnumerable<TrainerViewModel>> GetAllTrainersAsync(CancellationToken ct = default);
        Task<TrainerViewModel?> GetTrainerDetailsAsync(int trainerId, CancellationToken ct = default);
        Task<TrainerToUpdateViewModel?> GetTrainerToUpdateAsync(int trainerId, CancellationToken ct = default);
        Task<bool> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default);
        Task<bool> UpdateTrainerDetailsAsync(int trainerId, TrainerToUpdateViewModel model, CancellationToken ct = default);
        Task<bool> RemoveTrainerAsync(int trainerId, CancellationToken ct = default);
    }
}
