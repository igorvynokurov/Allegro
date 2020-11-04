using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllegroSearchService.Api.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AllegroSearchService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranslateController : Controller
    {
        // GET: TranslateController
        // GET: api/<SearchController>
        [HttpGet]
        public ActionResult Index()
        {
            var model = new TranslateViewModel() { TextRu = "test", TextPl = "test" };
            return View("Index",model);
        }

        // GET: TranslateController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TranslateController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TranslateController/Create
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

        // GET: TranslateController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TranslateController/Edit/5
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

        // GET: TranslateController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TranslateController/Delete/5
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
