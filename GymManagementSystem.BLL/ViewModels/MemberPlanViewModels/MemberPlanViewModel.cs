namespace GymManagementSystem.BLL.ViewModels.MemberPlanViewModels
{
    public class MemberPlanViewModel
    {
        public int    Id         { get; set; }
        public int    MemberId   { get; set; }
        public string MemberName { get; set; } = default!;
        public int    PlanId     { get; set; }
        public string PlanName   { get; set; } = default!;
        public string StartDate  { get; set; } = default!;
        public string EndDate    { get; set; } = default!;
        public string Status     { get; set; } = default!;  // "Active" / "Expired"
        public bool   IsActive   { get; set; }
    }
}
