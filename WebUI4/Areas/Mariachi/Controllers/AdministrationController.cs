using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.DAL.Mariacheros;
using AltovientoSolutions.DAL.Mariacheros.Model;


namespace WebUI4.Areas.Mariachi.Controllers
{
    public class AdministrationController : Controller
    {
        //
        // GET: /Mariachi/Administration/

        public ActionResult Index()
        {
            return RedirectToAction("AllLyrics");
        }


        //
        // GET: /Mariachi/Administration/AllLyrics
        [Authorize(Roles="Administrators")]
        public ActionResult AllLyrics()
        {
            MariachiMediator mediator = new MariachiMediator("Lyrics");
            List<AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel> allLyrics = mediator.GetAllLyrics();
           
            return View(allLyrics);
        }

        //
        // GET: /Mariachi/Administration/Lyrics
        [HttpGet]
        [Authorize(Roles = "Administrators")]
        public ActionResult Lyrics(string id)
        {
            MariachiMediator mediator = new MariachiMediator("Lyrics");
            AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel model = mediator.GetLyrics(id);

            LyricsModel songDetailsModel = new LyricsModel();
            if (model != null)
            {
                songDetailsModel.Id = model.Id;
                songDetailsModel.Lyrics = model.Lyrics;
                songDetailsModel.SongTitle = model.SongTitle;
                songDetailsModel.Author = model.Author;
            }

            return View(songDetailsModel);
        }

        //
        // GET: /Mariachi/Administration/Lyrics
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public ActionResult Lyrics(LyricsModel model)
        {
            if (ModelState.IsValid)
            {
                MariachiMediator mediator = new MariachiMediator("Lyrics");
                mediator.SaveSong(model.Id, model.SongTitle, model.Author, model.Lyrics);
                TempData["Message"] = "Successfully updated the database";
                return RedirectToAction("AllLyrics");
            }
            else
            {
                return View(new LyricsModel());
            }
        }

        //
        // DELETE: /Mariachi/Administration/Lyrics
        [HttpDelete]
        [Authorize(Roles = "Administrators")]
        public ActionResult Lyrics(string id, string dummy)
        {
                
            MariachiMediator mediator = new MariachiMediator("Lyrics");
            if (mediator.DeleteSong(id))
            {
               var result = new {Status = "OK"};
               return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Response.Status = "404 Not Found";
                var result = new { Status = "Fail", Message="Object not found" };
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        [Authorize(Roles = "Administrators")]
        public ActionResult ParseBands()
        {
           // AltovientoSolutions.DAL.SimpleDBImport.Importer.ParseXimpleDbBands();


            return Content("This function has been disabled.");
        }

    }
}
