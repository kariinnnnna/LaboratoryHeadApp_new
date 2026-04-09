using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MolServiceContracts.BindingModels;
using MOLServiceWebClient;

namespace LaboratoryHeadApp.Controllers
{
    public class SoftwareController : Controller
    {
        private readonly IMolApiClient _client;

        public SoftwareController(IMolApiClient client)
        {
            _client = client;
        }

        // GET: Software
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _client.GetSoftwaresAsync();
            return View(result ?? new List<MolServiceContracts.ViewModels.SoftwareViewModel>());
        }

        // GET: Software/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SoftwareBindingModel());
        }

        // POST: Software/Create
        [HttpPost]
        public async Task<IActionResult> Create(SoftwareBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.CreateSoftwareAsync(model);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось создать программное обеспечение");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Software/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var element = await _client.GetSoftwareAsync(id);
            if (element == null)
            {
                return NotFound();
            }

            var model = new SoftwareBindingModel
            {
                Id = element.Id,
                SoftwareName = element.SoftwareName,
                SoftwareDescription = element.SoftwareDescription,
                SoftwareKey = element.SoftwareKey,
                SoftwareK = element.SoftwareK
            };

            return View(model);
        }

        // POST: Software/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(SoftwareBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _client.UpdateSoftwareAsync(model);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Не удалось обновить программное обеспечение");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Software/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _client.DeleteSoftwareAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Не удалось удалить программное обеспечение";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
