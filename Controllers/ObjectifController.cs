using Microsoft.AspNetCore.Mvc;
using SuiviEntrainementSportif.Models;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Controllers
{
    public class ObjectifController : Controller
    {
        public static List<ObjectifSportif> Data = new List<ObjectifSportif>();

        public IActionResult Index()
        {
            return View(Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ObjectifSportif o)
        {
            o.Id = Data.Count + 1;
            Data.Add(o);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var obj = Data.Find(x => x.Id == id);
            return View(obj);
        }

        [HttpPost]
        public IActionResult Edit(ObjectifSportif o)
        {
            var old = Data.Find(x => x.Id == o.Id);
            if (old != null)
            {
                Data.Remove(old);
            }

            Data.Add(o);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var obj = Data.Find(x => x.Id == id);
            if (obj != null)
            {
                Data.Remove(obj);
            }

            return RedirectToAction("Index");
        }
    }
}
