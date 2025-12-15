using Microsoft.AspNetCore.Mvc;
using SuiviEntrainementSportif.Models;
using System.Collections.Generic;

namespace SuiviEntrainementSportif.Controllers
{
    public class UtilisateurController : Controller
    {
        public static List<Utilisateur> Data = new List<Utilisateur>();

        public IActionResult Index()
        {
            return View(Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Utilisateur u)
        {
            u.Id = Data.Count + 1;  
            Data.Add(u);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var user = Data.Find(x => x.Id == id);
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(Utilisateur u)
        {
            var old = Data.Find(x => x.Id == u.Id);
            if (old != null)
            {
                Data.Remove(old);
            }

            Data.Add(u);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var user = Data.Find(x => x.Id == id);
            if (user != null)
            {
                Data.Remove(user);
            }

            return RedirectToAction("Index");
        }
    }
}
