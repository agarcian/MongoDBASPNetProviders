using System.Web.Mvc;

namespace WebUI4.Areas.IPC
{
    public class IPCAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "IPC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "IPC_default",
                "IPC/{controller}/{action}/{id}",
                new { controller="Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
