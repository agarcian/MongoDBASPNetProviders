using System.Web.Mvc;

namespace WebUI4.Areas.Mariachi
{
    public class MariachiAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Mariachi";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Mariachi_default",
                "Mariachi/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }, new string[] { "WebUI4.Areas.Mariachi.Controllers" }
            );
        }
    }
}
