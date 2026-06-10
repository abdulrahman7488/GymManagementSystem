using GymManagementSystem.BLL.Common;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.MemberSessionViewModels;
using GymManagementSystem.DAL.Models;
using GymManagementSystem.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymManagementSystem.BLL.Services.Classes
{
    public class MemberSessionService : IMemberSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MemberSessionService(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        // ── INDEX ────────────────────────────────────────────────
        public async Task<SessionsIndexViewModel> GetSessionsIndexAsync(CancellationToken ct = default)
        {
            var now      = DateTime.Now;
            var sessions = await _unitOfWork._sessionRepository.GetAllSessionsWithTrainerAndCategoryAsync(ct: ct);
            var cards    = new List<SessionCardViewModel>();

            foreach (var s in sessions)
            {
                var booked = await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(s.Id, ct);
                cards.Add(new SessionCardViewModel
                {
                    SessionId    = s.Id,
                    Description  = s.Description,
                    TrainerName  = s.Trainer?.Name           ?? "–",
                    CategoryName = s.Category?.CategoryName  ?? "–",
                    StartDate    = s.StartDate,
                    EndDate      = s.EndDate,
                    Capacity     = s.Capacity,
                    BookedCount  = booked
                });
            }

            return new SessionsIndexViewModel
            {
                OngoingSessions  = cards.Where(c => c.StartDate <= now && c.EndDate >= now).ToList(),
                UpcomingSessions = cards.Where(c => c.StartDate >  now).ToList()
            };
        }

        // ── GET BOOKING FORM ─────────────────────────────────────
        public async Task<CreateBookingViewModel?> GetBookingFormAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.GetRepository<Session>().GetByIdAsync(sessionId, ct);
            if (session is null || session.StartDate <= DateTime.Now) return null;

            var booked  = await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(sessionId, ct);
            var members = await _unitOfWork.GetRepository<Member>().GetALLAsync(ct: ct);

            return new CreateBookingViewModel
            {
                SessionId          = sessionId,
                SessionDescription = session.Description,
                SessionStart       = session.StartDate,
                SessionEnd         = session.EndDate,
                AvailableSeats     = session.Capacity - booked,
                Members            = members.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text  = m.Name
                })
            };
        }

        // ── CREATE BOOKING ───────────────────────────────────────
        public async Task<Result> CreateBookingAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var now = DateTime.Now;

            var session = await _unitOfWork.GetRepository<Session>().GetByIdAsync(sessionId, ct);
            if (session is null)          return Result.NotFound("Session not found.");
            if (session.StartDate <= now) return Result.Validation("Only upcoming sessions can be booked.");

            var booked = await _unitOfWork._sessionRepository.GetCountOfBookedSlotsAsync(sessionId, ct);
            if (booked >= session.Capacity) return Result.Fail("Session is fully booked.");

            var member = await _unitOfWork.GetRepository<Member>().GetByIdAsync(memberId, ct);
            if (member is null) return Result.NotFound("Member not found.");

            var hasActiveMembership = await _unitOfWork.GetRepository<MemberShip>()
                .AnyAsync(m => m.MemberId == memberId && m.EndDate > now, ct);
            if (!hasActiveMembership)
                return Result.Validation("Member does not have an active membership.");

            var alreadyBooked = await _unitOfWork.GetRepository<Booking>()
                .AnyAsync(b => b.MemberId == memberId && b.SessionId == sessionId, ct);
            if (alreadyBooked)
                return Result.Fail("Member has already booked this session.");

            await _unitOfWork.GetRepository<Booking>().AddAsync(new Booking
            {
                MemberId  = memberId,
                SessionId = sessionId,
            });
            return Result.Ok();
        }

        // ── GET MEMBERS FOR UPCOMING SESSION ─────────────────────
        public async Task<SessionMembersViewModel?> GetMembersForUpcomingSessionAsync(int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.GetRepository<Session>().GetByIdAsync(sessionId, ct);
            if (session is null || session.StartDate <= DateTime.Now) return null;
            return await BuildSessionMembersAsync(session, "Upcoming", ct);
        }

        // ── GET MEMBERS FOR ONGOING SESSION ──────────────────────
        public async Task<SessionMembersViewModel?> GetMembersForOngoingSessionAsync(int sessionId, CancellationToken ct = default)
        {
            var now     = DateTime.Now;
            var session = await _unitOfWork.GetRepository<Session>().GetByIdAsync(sessionId, ct);
            if (session is null || session.StartDate > now || session.EndDate < now) return null;
            return await BuildSessionMembersAsync(session, "Ongoing", ct);
        }

        // ── CANCEL BOOKING ───────────────────────────────────────
        public async Task<Result> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var session = await _unitOfWork.GetRepository<Session>().GetByIdAsync(sessionId, ct);
            if (session is null)          return Result.NotFound("Session not found.");
            if (session.StartDate <= DateTime.Now)
                return Result.Validation("Cannot cancel a booking for a session that has already started.");

            var booking = await _unitOfWork.GetRepository<Booking>()
                .FirstOrDeafultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, Tracking: true, ct: ct);
            if (booking is null) return Result.NotFound("Booking not found.");

            await _unitOfWork.GetRepository<Booking>().DeleteAsync(booking);
            return Result.Ok();
        }

        // ── MARK ATTENDANCE ──────────────────────────────────────
        public async Task<Result> MarkAttendanceAsync(int memberId, int sessionId, CancellationToken ct = default)
        {
            var now     = DateTime.Now;
            var session = await _unitOfWork.GetRepository<Session>().GetByIdAsync(sessionId, ct);
            if (session is null)                                    return Result.NotFound("Session not found.");
            if (session.StartDate > now || session.EndDate < now)
                return Result.Validation("Attendance can only be marked for ongoing sessions.");

            var booking = await _unitOfWork.GetRepository<Booking>()
                .FirstOrDeafultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, Tracking: true, ct: ct);
            if (booking is null)     return Result.NotFound("Booking not found.");
            if (booking.IsAttended)  return Result.Validation("Attendance already marked.");

            booking.IsAttended = true;
            _unitOfWork.GetRepository<Booking>().UpdateAsync(booking);
            var rows = await _unitOfWork.SaveChangesAsync(ct);
            return rows > 0 ? Result.Ok() : Result.Fail("Failed to mark attendance.");
        }

        // ── PRIVATE HELPER ───────────────────────────────────────
        private async Task<SessionMembersViewModel> BuildSessionMembersAsync(
            Session session, string sessionType, CancellationToken ct)
        {
            var allBookings = await _unitOfWork.GetRepository<Booking>().GetALLAsync(ct: ct);
            var allMembers  = await _unitOfWork.GetRepository<Member>().GetALLAsync(ct: ct);

            var bookedMembers = allBookings
                .Where(b => b.SessionId == session.Id)
                .Select(b =>
                {
                    var m = allMembers.FirstOrDefault(x => x.Id == b.MemberId);
                    return new BookedMemberViewModel
                    {
                        MemberId    = b.MemberId,
                        SessionId   = b.SessionId,
                        MemberName  = m?.Name  ?? "Unknown",
                        MemberPhone = m?.Phone ?? "–",
                        BookingDate = b.CreatedAt.ToShortDateString(),
                        IsAttended  = b.IsAttended
                    };
                }).ToList();

            return new SessionMembersViewModel
            {
                SessionId          = session.Id,
                SessionDescription = session.Description,
                DateDisplay        = session.StartDate.ToString("dd MMM yyyy"),
                TimeRange          = $"{session.StartDate:hh:mm tt} – {session.EndDate:hh:mm tt}",
                SessionType        = sessionType,
                Members            = bookedMembers
            };
        }
    }
}
