//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace AltovientoSolutions.Common.Util
//{
//    public class CacheHelper
//    {
//        private System.Web.Caching.Cache cache;


//        public CacheHelper(System.Web.Caching.Cache Cache)
//        {
//            cache = Cache;
//        }


//        public T GetCachedItem<T>(string CacheItemKey, string ResetCacheArgument, Delegate Del)
//        {
//            string CacheKey = CacheItemKey; // To DO: Define the logic to generate the key based on your query.
         
//            T queryResult = default(T); // To Do: Declare the object with the right type.
            
//            try
//            {
//                // If the ResetCache parameter is passed in the query string, Or the Cache item is null, Or the cache item is of a different type as expected, then reset the cache.
//                if (!String.IsNullOrEmpty(ResetCacheArgument) || cache[CacheKey] == null || cache[CacheKey].GetType() == typeof(object)) // TO DO: replace typeof(object) with the expected type e.g. typeof(MyType)
//                {
//                    // TO DO: load the object with the new result by calling a delegate.
//                    queryResult = Del;

//                    // load the object in the cache.
//                    // Expiring the Cache in 20 minutes.  We can revisit this parameter. 
//                    cache.Add(CacheKey, queryResult, null, DateTime.MaxValue, new TimeSpan(0, 20, 0), System.Web.Caching.CacheItemPriority.Normal, null); // Equivalent to Cache["CacheItemKey"] = queryResult; but allows to set expiration.
//                }
//                else
//                {
//                    // the cache is available. Load the object from the cache.
//                    queryResult = (T)cache[CacheKey]; // TO DO: Replace (object) with the right casting
//                }
//            }
//            catch (Exception)
//            {
//                // If something failed, run the query again and clear the cache.
//                queryResult = default(T);
//                cache.Remove(CacheKey);
//            }

//            return queryResult;
//        }


//    }
//}
