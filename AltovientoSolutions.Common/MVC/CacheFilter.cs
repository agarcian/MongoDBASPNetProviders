using System;
using System.Web;
using System.Web.Mvc;




namespace AltovientoSolutions.Common.MVC
{
    /// <summary>
    /// Implements a Cache Filter for the MVC framework.
    /// </summary>
    /// <remarks>
    /// Code obtained from http://weblogs.asp.net/rashid/archive/2008/03/28/asp-net-mvc-action-filter-caching-and-compression.aspx
    /// </remarks>
    public class CacheFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Gets or sets the cache duration in seconds. The default is 10 seconds.
        /// </summary>
        /// <value>The cache duration in seconds.</value>
        public int Duration
        {
            get;
            set;
        }

        public CacheFilterAttribute()
        {
            Duration = 10;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Duration <= 0) return;

            HttpCachePolicyBase cache = filterContext.HttpContext.Response.Cache;
            TimeSpan cacheDuration = TimeSpan.FromSeconds(Duration);

            cache.SetCacheability(HttpCacheability.Public);
            cache.SetExpires(DateTime.Now.Add(cacheDuration));
            cache.SetMaxAge(cacheDuration);
            cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
        }
    }
}