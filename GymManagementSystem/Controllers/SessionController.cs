using GymManagementSystem.BLL.ViewModels.SessionViewModels;
using GymManagementSystem.BLL.Services.Interfaces;
using GymManagementSystem.BLL.ViewModels.SessionViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient.DataClassification;

namespace GymManagementSystem.Controllers
{
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        #region Index
        public async Task<IActionResult> Index(CancellationToken ct)
            => View(await _sessionService.GetAllSessionsAsync(ct));
        #endregion
        #region Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateDropdownsAsync(ct);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSessionViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(ct);
                return View(model);
            }

            var result = await _sessionService.CreateSessionAsync(model, ct);
            if (result.Sucess)
            {
                TempData["SuccessMessage"] = "Session created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Error;
            await PopulateDropdownsAsync(ct);
            return View(model);
        }

        #endregion
        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var session = await _sessionService.GetSessionByIdAsync(id, ct);
            if (session is null)
            {
                TempData["ErrorMessage"] = "Session not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(session);
        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken ct)
        {
            var session = await _sessionService.GetSessionToUpdateAsync(id, ct);
            if (session is null)
            {
                TempData["ErrorMessage"] = "Session cannot be edited (not found, already started, or has bookings).";
                return RedirectToAction(nameof(Index));
            }
            await PopulateDropdownsAsync(ct);
            return View(session);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateSessionViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(ct);
                return View(model);
            }

            var result = await _sessionService.UpdateSessionAsync(id, model, ct);
            if (result.Sucess)
            {
                TempData["SuccessMessage"] = "Session updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Error;
            await PopulateDropdownsAsync(ct);
            return View(model);
        }
        #endregion

        #region Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var session = await _sessionService.GetSessionByIdAsync(id, ct);
            if (session is null)
            {
                TempData["ErrorMessage"] = "Session not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(session);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _sessionService.RemoveSessionAsync(id, ct);
            TempData[result.Sucess ? "SuccessMessage" : "ErrorMessage"] =
                result.Sucess? "Session deleted successfully." : result.Error;
            return RedirectToAction(nameof(Index));
        }
        #endregion
        private async Task PopulateDropdownsAsync(CancellationToken ct)
        {
            ViewBag.Trainers = new SelectList(await _sessionService.GetTrainersForDropDownAsync(ct), "Id", "Name");
            ViewBag.Categories = new SelectList(await _sessionService.GetCategoriesForDropDownAsync(ct), "Id", "CategoryName");
        }

    }

}
