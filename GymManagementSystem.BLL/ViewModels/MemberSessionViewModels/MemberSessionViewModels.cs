using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.BLL.ViewModels.MemberSessionViewModels
{
    public class SessionCardViewModel
    {
        public int      SessionId    { get; set; }
        public string   Description  { get; set; } = default!;
        public string   TrainerName  { get; set; } = default!;
        public string   CategoryName { get; set; } = default!;
        public DateTime StartDate    { get; set; }
        public DateTime EndDate      { get; set; }
        public int      Capacity     { get; set; }
        public int      BookedCount  { get; set; }
        public int      AvailableSeats => Capacity - BookedCount;
        public string   TimeRangeDisplay => $"{StartDate:hh:mm tt} – {EndDate:hh:mm tt}";
        public string   DateDisplay      => StartDate.ToString("dd MMM yyyy");
    }

    public class SessionsIndexViewModel
    {
        public List<SessionCardViewModel> OngoingSessions  { get; set; } = [];
        public List<SessionCardViewModel> UpcomingSessions { get; set; } = [];
    }

    public class CreateBookingViewModel
    {
        [Required(ErrorMessage = "Member is required")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Required]
        public int SessionId { get; set; }

        public string   SessionDescription { get; set; } = default!;
        public DateTime SessionStart       { get; set; }
        public DateTime SessionEnd         { get; set; }
        public int      AvailableSeats     { get; set; }

        public IEnumerable<SelectListItem> Members { get; set; } = [];
    }

    public class BookedMemberViewModel
    {
        public int    MemberId    { get; set; }
        public int    SessionId   { get; set; }
        public string MemberName  { get; set; } = default!;
        public string MemberPhone { get; set; } = default!;
        public string BookingDate { get; set; } = default!;
        public bool   IsAttended  { get; set; }
    }

    public class SessionMembersViewModel
    {
        public int    SessionId          { get; set; }
        public string SessionDescription { get; set; } = default!;
        public string DateDisplay        { get; set; } = default!;
        public string TimeRange          { get; set; } = default!;
        public string SessionType        { get; set; } = default!;  // "Ongoing" / "Upcoming"
        public List<BookedMemberViewModel> Members { get; set; } = [];
    }
}
