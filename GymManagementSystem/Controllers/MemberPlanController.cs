using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.MemberPlanViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    public class MemberPlanController : Controller
    {
        private readonly IMemberPlanService _memberPlanService;

        public MemberPlanController(IMemberPlanService memberPlanService)
            => _memberPlanService = memberPlanService;

        // GET /MemberPlan
        public async Task<IActionResult> Index(CancellationToken ct)
            => View(await _memberPlanService.GetAllMembershipsAsync(ct));

        // GET /MemberPlan/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
            => View(await _memberPlanService.GetCreateFormAsync(ct));

        // POST /MemberPlan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMemberPlanViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var form = await _memberPlanService.GetCreateFormAsync(ct);
                model.Members = form.Members;
                model.Plans   = form.Plans;
                return View(model);
            }

            var result = await _memberPlanService.AssignPlanAsync(model.MemberId, model.PlanId, ct);
            if (result.Sucess)
            {
                TempData["SuccessMessage"] = "Plan assigned successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, result.Error!);
            var freshForm = await _memberPlanService.GetCreateFormAsync(ct);
            model.Members = freshForm.Members;
            model.Plans   = freshForm.Plans;
            return View(model);
        }

        // POST /MemberPlan/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int membershipId, CancellationToken ct)
        {
            var result = await _memberPlanService.CancelMembershipAsync(membershipId, ct);
            TempData[result.Sucess ? "SuccessMessage" : "ErrorMessage"] =
                result.Sucess ? "Membership cancelled successfully." : result.Error;
            return RedirectToAction(nameof(Index));
        }
    }
}
