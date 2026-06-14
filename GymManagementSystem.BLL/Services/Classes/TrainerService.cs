using AutoMapper;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.TrainerViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class TrainerService : ITrainerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;

        public TrainerService(IUnitOfWork unitOfWork, IMapper mapper, IAttachmentService attachmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _attachmentService = attachmentService;
        }

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<IEnumerable<TrainerViewModel>> GetAllTrainersAsync(CancellationToken ct = default)
        {
            var trainers = await _unitOfWork.GetRepository<Trainer>().GetALLAsync(ct: ct);
            return _mapper.Map<IEnumerable<TrainerViewModel>>(trainers);
        }

        // ── GET DETAILS ──────────────────────────────────────────
        public async Task<TrainerViewModel?> GetTrainerDetailsAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);
            return trainer is null ? null : _mapper.Map<TrainerViewModel>(trainer);
        }

        // ── GET FOR UPDATE ───────────────────────────────────────
        public async Task<TrainerToUpdateViewModel?> GetTrainerToUpdateAsync(int trainerId, CancellationToken ct = default)
        {
            var trainer = await _unitOfWork.GetRepository<Trainer>().GetByIdAsync(trainerId, ct);
            if (trainer is null) return null;

            var vm = _mapper.Map<TrainerToUpdateViewModel>(trainer);
            vm.Photo = trainer.Photo; // pass existing photo URL to view
            return vm;
        }

        // ── CREATE ───────────────────────────────────────────────
        public async Task<bool> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<Trainer>();

            if (await repo.AnyAsync(t => t.Email == model.Email, ct)) return false;
            if (await repo.AnyAsync(t => t.Phone == model.Phone, ct)) return false;

            var entity = _mapper.Map<Trainer>(model);

            // Upload photo if provided
            if (model.Photo is not null)
                entity.Photo = await _attachmentService.UploadAsync(model.Photo, "Trainers", ct);

            await repo.AddAsync(entity);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<bool> UpdateTrainerDetailsAsync(int trainerId, TrainerToUpdateViewModel model, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<Trainer>();
            var trainer = await repo.GetByIdAsync(trainerId, ct);
            if (trainer is null) return false;

            if (await repo.AnyAsync(t => t.Email == model.Email && t.Id != trainerId, ct)) return false;
            if (await repo.AnyAsync(t => t.Phone == model.Phone && t.Id != trainerId, ct)) return false;

            _mapper.Map(model, trainer);
            trainer.UpdatedAt = DateTime.Now;

            // Update photo if a new one is provided
            if (model.PhotoFile is not null)
            {
                await _attachmentService.DeleteAsync(trainer.Photo, ct);
                trainer.Photo = await _attachmentService.UploadAsync(model.PhotoFile, "Trainers", ct);
            }

            await repo.UpdateAsync(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }

        // ── REMOVE ───────────────────────────────────────────────
        public async Task<bool> RemoveTrainerAsync(int trainerId, CancellationToken ct = default)
        {
            var repo = _unitOfWork.GetRepository<Trainer>();
            var trainer = await repo.GetByIdAsync(trainerId, ct);
            if (trainer is null) return false;

            var hasFutureSessions = await _unitOfWork.GetRepository<Session>()
                .AnyAsync(s => s.TrainerId == trainerId && s.StartDate > DateTime.Now, ct);
            if (hasFutureSessions) return false;

            // Delete photo from disk before removing trainer
            if (!string.IsNullOrEmpty(trainer.Photo))
                await _attachmentService.DeleteAsync(trainer.Photo, ct);

            await repo.DeleteAsync(trainer);
            var result = await _unitOfWork.SaveChangesAsync(ct);
            return result > 0;
        }
    }
}
