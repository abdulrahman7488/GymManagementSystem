using GymManagementSystem.BLL.Common;
using GymManagementSystem.BLL.ViewModels.MemberSessionViewModels;

namespace GymManagementSystem.BLL.Services.Interfaces
{
    public interface IMemberSessionService
    {
        Task<SessionsIndexViewModel>     GetSessionsIndexAsync(CancellationToken ct = default);
        Task<CreateBookingViewModel?>    GetBookingFormAsync(int sessionId, CancellationToken ct = default);
        Task<Result>                     CreateBookingAsync(int memberId, int sessionId, CancellationToken ct = default);
        Task<SessionMembersViewModel?>   GetMembersForUpcomingSessionAsync(int sessionId, CancellationToken ct = default);
        Task<SessionMembersViewModel?>   GetMembersForOngoingSessionAsync(int sessionId, CancellationToken ct = default);
        Task<Result>                     CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default);
        Task<Result>                     MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default);
    }
}
