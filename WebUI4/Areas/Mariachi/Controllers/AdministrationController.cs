using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.DAL.Mariacheros;


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

        public ActionResult AllLyrics()
        {
            MariachiMediator mediator = new MariachiMediator("Lyrics");
            List<AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel> allLyrics = mediator.GetAllLyrics();
           
            return View(allLyrics);
        }

        //
        // GET: /Mariachi/Administration/Lyrics
        [HttpGet]
        public ActionResult Lyrics(string id)
        {
            MariachiMediator mediator = new MariachiMediator("Lyrics");
            AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel model = mediator.GetLyrics(id);

            Models.SongDetailsModel songDetailsModel = new Models.SongDetailsModel();
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
        public ActionResult Lyrics(Models.SongDetailsModel model)
        {
            if (ModelState.IsValid)
            {
                MariachiMediator mediator = new MariachiMediator("Lyrics");
                mediator.SaveSong(null, model.SongTitle, model.Author, model.Lyrics);
                TempData["Message"] = "Successfully updated the database";
                return RedirectToAction("AllLyrics");
            }
            else
            {
                return View(new Models.SongDetailsModel());
            }
        }


    }
}
