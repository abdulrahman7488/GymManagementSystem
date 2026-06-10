using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.TrainerViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers

{
    public class TrainersController : Controller
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
            => View(await _trainerService.GetAllTrainersAsync(ct));

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreateTrainerViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _trainerService.CreateTrainerAsync(model, ct);
            if (result)
            {
                TempData["SuccessMessage"] = "Trainer created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Trainer Failed create";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerDetailsAsync(id, ct);
            if (trainer is null)
            {
                TempData["ErrorMessage"] = "Trainer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerToUpdateAsync(id, ct);
            if (trainer is null)
            {
                TempData["ErrorMessage"] = "Trainer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(trainer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TrainerToUpdateViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _trainerService.UpdateTrainerDetailsAsync(id, model, ct);
            if (result)
            {
                TempData["SuccessMessage"] = "Trainer updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Trainer Failed To update";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var trainer = await _trainerService.GetTrainerDetailsAsync(id, ct);
            if (trainer is null)
            {
                TempData["ErrorMessage"] = "Trainer not found.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _trainerService.RemoveTrainerAsync(id, ct);
            if (result)
                TempData["SuccessMessage"] = "Trainer deleted successfully.";
            TempData["ErrorMessage"] = "Failed To delete Trainer";
            return RedirectToAction(nameof(Index));
        }
    }

}
