using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LaboratoryHeadApp.Controllers
{
    public class MolController : Controller
    {
        // GET: MolController
        public ActionResult Index()
        {
            return View();
        }

        // GET: MolController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MolController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MolController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MolController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MolController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MolController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MolController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }
}
