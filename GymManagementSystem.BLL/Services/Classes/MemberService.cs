using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels;
using GymManagementSystem.BLL.ViewModels.MemberViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class MemberService : IMemberService
    {
        private readonly IGenericRepository<Member> _memberRepository;
        private readonly IGenericRepository<MemberShip> _membershipRepository;
        private readonly IGenericRepository<Plan> _planRepository;
        private readonly IGenericRepository<HealthRecord> _healthrecordRepository;
        private readonly IGenericRepository<Booking> _bookingRepository;

        public MemberService(IGenericRepository<Member> memberRepository,
           IGenericRepository<MemberShip> membershipRepository,
           IGenericRepository<Plan> planRepository,
           IGenericRepository<HealthRecord> healthrecordRepository,
           IGenericRepository<Booking> bookingRepository)
        {
            _memberRepository = memberRepository;
            _membershipRepository = membershipRepository;
            _planRepository = planRepository;
            _healthrecordRepository = healthrecordRepository;
            _bookingRepository = bookingRepository;
        }
        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            var emailExists = await _memberRepository.AnyAsync(x => x.Email == model.Email, ct);
            var phoneExists = await _memberRepository.AnyAsync(x => x.Phone == model.Phone, ct);
            if (emailExists || phoneExists) return false;
            //ManaulMapping
            var Member = new Member()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                DateOFBirth = model.DateOfBirth,
                Gender = model.Gender,
                Address = new Address()
                {
                    BuildingNumber= model.BuildingNumber,
                    City = model.City,
                    Street = model.Street

                },
                HealthRecord= new HealthRecord()
                {
                    Height = model.HealthRecordViewModel.Height,
                    Weight = model.HealthRecordViewModel.Weight,
                    BloodType = model.HealthRecordViewModel.BloodType,
                    Note = model.HealthRecordViewModel.Note
                }



            };
            var Result=await _memberRepository.AddAsync(Member);
            return Result > 0;

        }

        public async Task<IEnumerable<MemberViewModel>> GetALLMembersAsync(CancellationToken ct = default)
        {
            var Members = await _memberRepository.GetALLAsync(ct: ct);
            if (!Members.Any()) return [];
            var MembersViewModel = Members.Select(m => new MemberViewModel
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Photo = m.Photo,
                Gender = m.Gender.ToString(),

            });
            return MembersViewModel;
        }

        public  async Task<MemberViewModel?> GetMemberDetailsAsync(int memberId, CancellationToken ct = default)
        {
            var Member =  await _memberRepository.GetByIdAsync(memberId, ct);
            if (Member is null) return null;
            var viewmodel = new MemberViewModel()
            {
                Name= Member.Name,
                Email= Member.Email,
                Phone = Member.Phone,
                DateOfBirth = Member.DateOFBirth.ToShortDateString(),
                Gender= Member.Gender.ToString(),
                Address = $"{Member.Address.BuildingNumber}-{Member.Address.Street}-{Member.Address.City}",

            };
            var activeMembership = await _membershipRepository.FirstOrDeafultAsync(Mp => Mp.MemberId == memberId
                &&Mp.EndDate>DateTime.Now,ct:ct);

            if(activeMembership is not null)
            {
                var activeplan= await _planRepository.GetByIdAsync(activeMembership.PlanId, ct);
                viewmodel.PlanName = activeplan?.Name;
                viewmodel.MembershipStartDate = activeMembership.CreatedAt.ToShortDateString();
                viewmodel.MembershipEndDate = activeMembership.EndDate.ToShortDateString();

            }
            return viewmodel;
        }

        public async Task<HealthRecordViewModel?> GetMemberHealthRecordAsync(int memberId, CancellationToken ct = default)
        {
            var record=await _healthrecordRepository.FirstOrDeafultAsync(x=>x.MemberId== memberId, ct: ct);
            if (record is null) return null;
            else
            {
                return new HealthRecordViewModel()
                {
                    Height = record.Height,
                    Weight = record.Weight,
                    BloodType = record.BloodType,
                    Note = record.Note
                };
            }
        }

        public async Task<MemberToUpdateViewModel?> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default)
        {
            var member =  await _memberRepository.GetByIdAsync(memberId, ct);
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

        public async Task<bool> RemoveMemberAsync(int memberId, CancellationToken ct = default)
        {
            var Member = await _memberRepository.GetByIdAsync(memberId, ct);
            if (Member is null) return false;
            var HasFutureSessions = await _bookingRepository.AnyAsync(b => b.MemberId == memberId && b.Session.StartDate > DateTime.Now);
            if ((HasFutureSessions))
            {
                return false;
            }
           var Result= await _memberRepository.DeleteAsync(Member);
            return Result > 0 ? true : false;


        }

        public async Task<bool> UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default)
        {
            var Member=await _memberRepository.GetByIdAsync(id, ct);
            if (Member is null) return false;
             if(await _memberRepository.AnyAsync(m=>m.Email == model.Email && m.Id != id, ct))
                return false;
            if(await _memberRepository.AnyAsync(m => m.Phone == model.Phone && m.Id != id, ct))
                return false;

            Member.Email= model.Email;
            Member.Phone= model.Phone;
            Member.Address.City = model.City;
            Member.Address.Street = model.Street;
            Member.Address.BuildingNumber = model.BuildingNumber;
            Member.UpdatedAt=DateTime.Now;
            var Result = await _memberRepository.UpdateAsync(Member);
            return Result > 0 ? true : false;

        }
    }
}
