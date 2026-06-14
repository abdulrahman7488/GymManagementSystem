using GymManagementSystem.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    [Authorize]
    public class MemberSessionController : Controller
    {
        private readonly IMemberSessionService _memberSessionService;

        public MemberSessionController(IMemberSessionService memberSessionService)
            => _memberSessionService = memberSessionService;

        // GET /MemberSession
        public async Task<IActionResult> Index(CancellationToken ct)
            => View(await _memberSessionService.GetSessionsIndexAsync(ct));

        // GET /MemberSession/Create?sessionId=5
        [HttpGet]
        public async Task<IActionResult> Create(int sessionId, CancellationToken ct)
        {
            var vm = await _memberSessionService.GetBookingFormAsync(sessionId, ct);
            if (vm is null)
            {
                TempData["ErrorMessage"] = "Session not found or is not bookable.";
                return RedirectToAction(nameof(Index));
            }
            return View(vm);
        }

        // POST /MemberSession/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int memberId, int sessionId, CancellationToken ct)
        {
            var result = await _memberSessionService.CreateBookingAsync(memberId, sessionId, ct);
            if (result.Sucess)
            {
                TempData["SuccessMessage"] = "Session booked successfully.";
                return RedirectToAction(nameof(Index));
            }

            var form = await _memberSessionService.GetBookingFormAsync(sessionId, ct);
            if (form is null) return RedirectToAction(nameof(Index));
            form.MemberId = memberId;
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(form);
        }

        // GET /MemberSession/GetMembersForUpcomingSession/5
        [HttpGet]
        public async Task<IActionResult> GetMembersForUpcomingSession(int sessionId, CancellationToken ct)
        {
            var vm = await _memberSessionService.GetMembersForUpcomingSessionAsync(sessionId, ct);
            if (vm is null) { TempData["ErrorMessage"] = "Session not found or not upcoming."; return RedirectToAction(nameof(Index)); }
            return View("SessionMembers", vm);
        }

        // GET /MemberSession/GetMembersForOngoingSessions/5
        [HttpGet]
        public async Task<IActionResult> GetMembersForOngoingSessions(int sessionId, CancellationToken ct)
        {
            var vm = await _memberSessionService.GetMembersForOngoingSessionAsync(sessionId, ct);
            if (vm is null) { TempData["ErrorMessage"] = "Session not found or not ongoing."; return RedirectToAction(nameof(Index)); }
            return View("SessionMembers", vm);
        }

        // POST /MemberSession/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int memberId, int sessionId, CancellationToken ct)
        {
            var result = await _memberSessionService.CancelBookingAsync(memberId, sessionId, ct);
            TempData[result.Sucess ? "SuccessMessage" : "ErrorMessage"] =
                result.Sucess ? "Booking cancelled." : result.Error;
            return RedirectToAction(nameof(Index));
        }

        // POST /MemberSession/MarkAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(int memberId, int sessionId, CancellationToken ct)
        {
            var result = await _memberSessionService.MarkAttendanceAsync(memberId, sessionId, ct);
            TempData[result.Sucess ? "SuccessMessage" : "ErrorMessage"] =
                result.Sucess ? "Attendance marked." : result.Error;
            return RedirectToAction(nameof(GetMembersForOngoingSessions), new { sessionId });
        }
    }
}
