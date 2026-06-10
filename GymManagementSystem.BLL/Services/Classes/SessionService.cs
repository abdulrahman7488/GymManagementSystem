using AutoMapper;
using GymManagementSystem.BLL.ViewModels.SessionViewModels;
using GymManagementSystem.BLL.Common;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.SessionViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Models.Enums;
using GymManagementSystem.DAL.Repositories.Classes;
using GymManagementSystem.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SessionService(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }


        public  async Task<IEnumerable<SessionViewModel>?> GetAllSessionsAsync(CancellationToken ct = default)
        {
            var sessions = await _unitOfWork._sessionRepository.GetAllSessionsWithTrainerAndCategoryAsync(ct: ct);
            if (sessions?.Any() != true) return null;
            sessions = sessions.OrderByDescending(x => x.StartDate);
            var MappedSessions = _mapper.Map<IEnumerable<SessionViewModel>>(sessions);
            foreach (var session in MappedSessions)
            {
                session.AvailableSlots = session.Capacity - (await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(session.Id, ct));
            }
            return MappedSessions;



        }
        public async  Task<SessionViewModel?> GetSessionByIdAsync(int SessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork._sessionRepository.GetSessionWithTrainerAndCategoryAsync(SessionId, ct);
            if (session == null) return null;
            var MappedSession = _mapper.Map<Session, SessionViewModel>(session);
            MappedSession.AvailableSlots = MappedSession.Capacity - (await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(session.Id, ct));
            return MappedSession;

        }

        public async Task<UpdateSessionViewModel?> GetSessionToUpdateAsync(int SessionId, CancellationToken ct = default)
        {
           var session= await _unitOfWork.GetRepository<Session>().GetByIdAsync(SessionId, ct);
           if(session is null) return null;
           if(!await IsSessionValidForUpdatingAsync(session,ct)) return null;
           return _mapper.Map<Session, UpdateSessionViewModel>(session);
        }

        private async Task<bool> IsSessionValidForUpdatingAsync(Session session,CancellationToken ct=default)
        {
           if(session.StartDate <= DateTime.Now) return false;
           var booked=await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(session.Id, ct);
            return booked == 0;
        }

        public async Task<Result> CreateSessionAsync(CreateSessionViewModel model, CancellationToken ct = default)
        {
            if (model.EndDate <= model.StartDate)
                return Result.Validation("End Date Must Be After Start Date");
             if(model.StartDate <= DateTime.Now)
                return Result.Validation("Start Date Must Be In The Future");
             var TrainerRepo=_unitOfWork.GetRepository<Trainer>();
            var trainer= await TrainerRepo.GetByIdAsync(model.TrainerId, ct);
            if(trainer is null)
                return Result.NotFound("Trainer Not Found");
            var CategoryRepo= _unitOfWork.GetRepository<Category>();
            var category = await CategoryRepo.GetByIdAsync(model.CategoryId, ct);
            if(category is null)
                return Result.NotFound("Category Not Found");
            var IsValidSpeciality = Enum.TryParse<Specialities>(category.CategoryName, true, out var CategorySpeciality);
            if(!IsValidSpeciality||trainer.Specialities!=CategorySpeciality)
                return Result.Validation("Trainer Speciality Does Not Match Session Category");
             var session = _mapper.Map<CreateSessionViewModel, Session>(model); 
            var Sessionrepo = _unitOfWork.GetRepository<Session>();
            await Sessionrepo.AddAsync(session);
            var AffectedRows = await _unitOfWork.SaveChangesAsync(ct);
            return AffectedRows>0?Result.Ok() : Result.Fail("Failed To Create Session");



        }
        public async Task<Result> UpdateSessionAsync(int id, UpdateSessionViewModel model, CancellationToken ct = default)
        {
           var SessionRepo = _unitOfWork.GetRepository<Session>();
            var Session=await SessionRepo.GetByIdAsync(id, ct);
            if(Session is null) return Result.NotFound("Session Not Found");
            if(Session.StartDate <= DateTime.Now)
                return Result.Validation("Cannot Update Session That Has Already Started");
             var BookedCount= await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(id, ct);
            if (BookedCount > 0)
            {
                return Result.Validation("Cannot Update Session That Has Booked Slots");
            }
            if(model.EndDate <= model.StartDate)
                return Result.Validation("End Date Must Be After Start Date");
            if (model.StartDate <= DateTime.Now)
            {
                return Result.Validation("Start Date Must Be In The Future");
            }
            var TrainerRepo = _unitOfWork.GetRepository<Trainer>();
            var Trainer = await TrainerRepo.GetByIdAsync(model.TrainerId, ct);
            if (Trainer is null)
                return Result.NotFound("Trainer Not Found");
            var CategoryRepo = _unitOfWork.GetRepository<Category>();
            var category = await CategoryRepo.GetByIdAsync(Session.CategoryId, ct);
            if (category is null)
                return Result.NotFound("Category Not Found");
            var IsValidSpeciality = Enum.TryParse<Specialities>(category.CategoryName, true, out var CategorySpeciality);
            if (!IsValidSpeciality || Trainer.Specialities != CategorySpeciality)
                return Result.Validation("Trainer Speciality Does Not Match Session Category");
            _mapper.Map(model, Session);
            Session.UpdatedAt = DateTime.Now;
            await SessionRepo.UpdateAsync(Session);
            var AffectedRows = await _unitOfWork.SaveChangesAsync(ct);
            return AffectedRows > 0 ? Result.Ok() : Result.Fail("Failed To Update Session");
        }


        public  async Task<Result> RemoveSessionAsync(int SessionId, CancellationToken ct = default)
        {
            var SessionRepo = _unitOfWork.GetRepository<Session>();
            var Session = await SessionRepo.GetByIdAsync(SessionId, ct);
            if (Session is null) return Result.NotFound("Session Not Found");
            if (Session.EndDate >= DateTime.Now)
                return Result.Validation("Can Not Delete A Session That  Has Not Yet Ended");
            var BookedCount = await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(SessionId, ct);
            if (BookedCount > 0)
            {
                return Result.Validation(" Can Not Delete A Session That Has Bookings");
            }
            await SessionRepo.DeleteAsync(Session);
            var AffectedRows = await _unitOfWork.SaveChangesAsync(ct);
            return AffectedRows > 0 ? Result.Ok() : Result.Fail("Failed To Delete Session");
        }


        public async Task<IEnumerable<CategorySelectViewModel>> GetCategoriesForDropDownAsync(CancellationToken ct = default)
        {
            var Categories = await  _unitOfWork.GetRepository<Category>().GetALLAsync(ct: ct);
            return _mapper.Map<IEnumerable<CategorySelectViewModel>>(Categories);
        }
            



        public  async Task<IEnumerable<TrainerSelectViewModel>> GetTrainersForDropDownAsync(CancellationToken ct = default)
        {
            var Trainers = await _unitOfWork.GetRepository<Trainer>().GetALLAsync(ct: ct);
            return _mapper.Map<IEnumerable<TrainerSelectViewModel>>(Trainers);
        }
    }
}
