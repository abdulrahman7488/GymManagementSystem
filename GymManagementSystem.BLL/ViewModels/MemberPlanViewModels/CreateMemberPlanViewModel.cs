using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymManagementSystem.BLL.ViewModels.MemberPlanViewModels
{
    public class CreateMemberPlanViewModel
    {
        [Required(ErrorMessage = "Member is required")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Plan is required")]
        [Display(Name = "Plan")]
        public int PlanId { get; set; }

        // Dropdowns populated by service
        public IEnumerable<SelectListItem> Members { get; set; } = [];
        public IEnumerable<SelectListItem> Plans   { get; set; } = [];
    }
}
