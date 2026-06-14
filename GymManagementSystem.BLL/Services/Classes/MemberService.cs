using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels;
using GymManagementSystem.BLL.ViewModels.MemberViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IGenericRepository<Member> _memberRepository;
        private readonly IGenericRepository<MemberShip> _membershipRepository;
        private readonly IGenericRepository<Plan> _planRepository;
        private readonly IGenericRepository<HealthRecord> _healthrecordRepository;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IAttachmentService _attachmentService;

        public MemberService(
            IGenericRepository<Member> memberRepository,
            IGenericRepository<MemberShip> membershipRepository,
            IGenericRepository<Plan> planRepository,
            IGenericRepository<HealthRecord> healthrecordRepository,
            IGenericRepository<Booking> bookingRepository,
            IAttachmentService attachmentService)
        {
            _memberRepository = memberRepository;
            _membershipRepository = membershipRepository;
            _planRepository = planRepository;
            _healthrecordRepository = healthrecordRepository;
            _bookingRepository = bookingRepository;
            _attachmentService = attachmentService;
        }

        // ── CREATE ───────────────────────────────────────────────
        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            var emailExists = await _memberRepository.AnyAsync(x => x.Email == model.Email, ct);
            var phoneExists = await _memberRepository.AnyAsync(x => x.Phone == model.Phone, ct);
            if (emailExists || phoneExists) return false;

            var member = new Member()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                DateOFBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = new Address()
                {
                    BuildingNumber = model.BuildingNumber,
                    City = model.City,
                    Street = model.Street
                },
                HealthRecord = new HealthRecord()
                {
                    Height = model.HealthRecordViewModel.Height,
                    Weight = model.HealthRecordViewModel.Weight,
                    BloodType = model.HealthRecordViewModel.BloodType,
                    Note = model.HealthRecordViewModel.Note
                }
            };

            // Upload photo if provided
            if (model.Photo is not null)
                member.Photo = await _attachmentService.UploadAsync(model.Photo, "Members", ct);

            var result = await _memberRepository.AddAsync(member);
            return result > 0;
        }

        // ── GET ALL ──────────────────────────────────────────────
        public async Task<IEnumerable<MemberViewModel>> GetALLMembersAsync(CancellationToken ct = default)
        {
            var members = await _memberRepository.GetALLAsync(ct: ct);
            if (!members.Any()) return [];

            return members.Select(m => new MemberViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Photo = m.Photo,
                Gender = m.Gender.ToString(),
            });
        }

        // ── GET DETAILS ──────────────────────────────────────────
        public async Task<MemberViewModel?> GetMemberDetailsAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _memberRepository.GetByIdAsync(memberId, ct);
            if (member is null) return null;

            var viewmodel = new MemberViewModel()
            {
                Name = member.Name,
                Email = member.Email,
                Phone = member.Phone,
                Photo = member.Photo,
                DateOfBirth = member.DateOFBirth.ToShortDateString(),
                Gender = member.Gender.ToString(),
                Address = $"{member.Address.BuildingNumber}-{member.Address.Street}-{member.Address.City}",
            };

            var activeMembership = await _membershipRepository.FirstOrDeafultAsync(
                mp => mp.MemberId == memberId && mp.EndDate > DateTime.Now, ct: ct);

            if (activeMembership is not null)
            {
                var activePlan = await _planRepository.GetByIdAsync(activeMembership.PlanId, ct);
                viewmodel.PlanName = activePlan?.Name;
                viewmodel.MembershipStartDate = activeMembership.CreatedAt.ToShortDateString();
                viewmodel.MembershipEndDate = activeMembership.EndDate.ToShortDateString();
            }

            return viewmodel;
        }

        // ── GET HEALTH RECORD ────────────────────────────────────
        public async Task<HealthRecordViewModel?> GetMemberHealthRecordAsync(int memberId, CancellationToken ct = default)
        {
            var record = await _healthrecordRepository.FirstOrDeafultAsync(x => x.MemberId == memberId, ct: ct);
            if (record is null) return null;

            return new HealthRecordViewModel()
            {
                Height = record.Height,
                Weight = record.Weight,
                BloodType = record.BloodType,
                Note = record.Note
            };
        }

        // ── GET FOR UPDATE ───────────────────────────────────────
        public async Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _memberRepository.GetByIdAsync(memberId, ct);
            if (member is null) return null;

            return new MemberToUpdateViewModel()
            {
                Name = member.Name,
                Email = member.Email,
                Phone = member.Phone,
                Street = member.Address.Street,
                City = member.Address.City,
                BuildingNumber = member.Address.BuildingNumber,
                Photo = member.Photo
            };
        }

        // ── UPDATE ───────────────────────────────────────────────
        public async Task<bool> UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var member = await _memberRepository.GetByIdAsync(id, ct);
            if (member is null) return false;

            if (await _memberRepository.AnyAsync(m => m.Email == model.Email && m.Id != id, ct))
                return false;
            if (await _memberRepository.AnyAsync(m => m.Phone == model.Phone && m.Id != id, ct))
                return false;

            member.Email = model.Email;
            member.Phone = model.Phone;
            member.Address.City = model.City;
            member.Address.Street = model.Street;
            member.Address.BuildingNumber = model.BuildingNumber;
            member.UpdatedAt = DateTime.Now;

            // Update photo if a new one is provided
            if (model.PhotoFile is not null)
            {
                // Delete old photo first
                await _attachmentService.DeleteAsync(member.Photo, ct);
                // Upload new photo
                member.Photo = await _attachmentService.UploadAsync(model.PhotoFile, "Members", ct);
            }

            var result = await _memberRepository.UpdateAsync(member);
            return result > 0;
        }

        // ── REMOVE ───────────────────────────────────────────────
        public async Task<bool> RemoveMemberAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _memberRepository.GetByIdAsync(memberId, ct);
            if (member is null) return false;

            var hasFutureSessions = await _bookingRepository.AnyAsync(
                b => b.MemberId == memberId && b.Session.StartDate > DateTime.Now);
            if (hasFutureSessions) return false;

            // Delete photo from disk before removing member
            if (!string.IsNullOrEmpty(member.Photo))
                await _attachmentService.DeleteAsync(member.Photo, ct);

            var result = await _memberRepository.DeleteAsync(member);
            return result > 0;
        }
    }
}
