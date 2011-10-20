using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI4.Areas.Mobile.Controllers
{
    public class LyricsController : Controller
    {
        //
        // GET: /Mobile/Home/

        public ActionResult Index()
        {
            AltovientoSolutions.DAL.Mariacheros.MariachiMediator mediator = new AltovientoSolutions.DAL.Mariacheros.MariachiMediator("Lyrics");
            List<AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel> lyrics = new List<AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel>();

            lyrics.AddRange(mediator.GetAllLyrics());
            return View(lyrics);
        }

        public ActionResult Song(string id)
        {
            AltovientoSolutions.DAL.Mariacheros.MariachiMediator mediator = new AltovientoSolutions.DAL.Mariacheros.MariachiMediator("Lyrics");
            AltovientoSolutions.DAL.Mariacheros.Model.LyricsModel song = mediator.GetLyrics(id);
            return View(song);
        }

    }
}
