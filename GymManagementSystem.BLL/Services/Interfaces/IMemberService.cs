using GymManagementSystem.BLL.ViewModels;
using GymManagementSystem.BLL.ViewModels.MemberViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberViewModel>> GetALLMembersAsync(CancellationToken ct = default);
        Task<bool>CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default);
       
        Task<MemberViewModel?> GetMemberDetailsAsync(int memberId, CancellationToken ct = default);
    Task<HealthRecordViewModel?>GetMemberHealthRecordAsync(int memberId, CancellationToken ct = default);
        Task<MemberToUpdateViewModel?>GetMemberToUpdateAsync(int memberId, CancellationToken ct = default);
        Task<bool>UpdateMemberDetailsAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default);
        Task<bool>RemoveMemberAsync(int memberId, CancellationToken ct = default);
            

    }
}
