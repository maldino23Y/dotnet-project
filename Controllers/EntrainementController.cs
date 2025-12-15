using Microsoft.AspNetCore.Mvc;
using SuiviEntrainementSportif.Models;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Controllers
{
    public class EntrainementController : Controller
    {
        // Simulation database
        public static List<Entrainement> Data = new List<Entrainement>();

        public IActionResult Index()
        {
            return View(Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Entrainement e)
        {
            e.Id = Data.Count + 1;
            Data.Add(e);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var entr = Data.Find(x => x.Id == id);
            return View(entr);
        }

        [HttpPost]
        public IActionResult Edit(Entrainement e)
        {
            var old = Data.Find(x => x.Id == e.Id);
            if (old != null)
            {
                Data.Remove(old);
            }

            Data.Add(e);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var entr = Data.Find(x => x.Id == id);
            if (entr != null)
            {
                Data.Remove(entr);
            }

            return RedirectToAction("Index");
        }
    }
}
