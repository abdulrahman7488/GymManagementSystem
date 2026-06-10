using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.PlanViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    public class PlansController : Controller
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
            => View(await _planService.GetAllPlansAsync(ct));

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var plan = await _planService.GetPlanByIdAsync(id, ct);
            if (plan is null)
            {
                TempData["ErrorMessage"] = "Plan not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var plan = await _planService.GetPlanToUpdateAsync(id, ct);
            if (plan is null)
            {
                TempData["ErrorMessage"] = "Plan cannot be edited (not found, inactive, or has active memberships).";
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdatePlanViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _planService.UpdatePlanAsync(id, model, ct);
            if (result)
            {
                TempData["SuccessMessage"] = "Plan updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Plan Failed To update";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _planService.ToggleActivationAsync(id, ct);
            if (result)
                TempData["SuccessMessage"] = "Plan status changed";
            TempData["ErrorMessage"] = "Failed to Toggle Plan Status";
            return RedirectToAction(nameof(Index));
        }

    }
}
