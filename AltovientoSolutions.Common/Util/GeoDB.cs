using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace AltovientoSolutions.Common.Util
{
    public class GeoDB
    {

        public static Dictionary<String, String> GetAllUSStates()
        {
            Dictionary<String, String> allStates = new Dictionary<string, string>();

            allStates.Add("AK", "ALASKA");
            allStates.Add("AL", "ALABAMA");
            allStates.Add("AR", "ARKANSAS");
            allStates.Add("AZ", "ARIZONA");
            allStates.Add("CA", "CALIFORNIA");
            allStates.Add("CO", "COLORADO");
            allStates.Add("CT", "CONNECTICUT");
            allStates.Add("DC", "DISTRICT OF COLUMBIA");
            allStates.Add("DE", "DELAWARE");
            allStates.Add("FL", "FLORIDA");
            allStates.Add("GA", "GEORGIA");
            allStates.Add("HI", "HAWAII");
            allStates.Add("IA", "IOWA");
            allStates.Add("ID", "IDAHO");
            allStates.Add("IL", "ILLINOIS");
            allStates.Add("IN", "INDIANA");
            allStates.Add("KS", "KANSAS");
            allStates.Add("KY", "KENTUCKY");
            allStates.Add("LA", "LOUISIANA");
            allStates.Add("MA", "MASSACHUSETTS");
            allStates.Add("MD", "MARYLAND");
            allStates.Add("ME", "MAINE");
            allStates.Add("MI", "MICHIGAN");
            allStates.Add("MN", "MINNESOTA");
            allStates.Add("MO", "MISSOURI");
            allStates.Add("MS", "MISSISSIPPI");
            allStates.Add("MT", "MONTANA");
            allStates.Add("NC", "NORTH CAROLINA");
            allStates.Add("ND", "NORTH DAKOTA");
            allStates.Add("NE", "NEBRASKA");
            allStates.Add("NH", "NEW HAMPSHIRE");
            allStates.Add("NJ", "NEW JERSEY");
            allStates.Add("NM", "NEW MEXICO");
            allStates.Add("NV", "NEVADA");
            allStates.Add("NY", "NEW YORK");
            allStates.Add("OH", "OHIO");
            allStates.Add("OK", "OKLAHOMA");
            allStates.Add("OR", "OREGON");
            allStates.Add("PA", "PENNSYLVANIA");
            allStates.Add("PR", "PUERTO RICO");
            allStates.Add("RI", "RHODE ISLAND");
            allStates.Add("SC", "SOUTH CAROLINA");
            allStates.Add("SD", "SOUTH DAKOTA");
            allStates.Add("TN", "TENNESSEE");
            allStates.Add("TX", "TEXAS");
            allStates.Add("UT", "UTAH");
            allStates.Add("VA", "VIRGINIA");
            allStates.Add("VI", "VIRGIN ISLANDS");
            allStates.Add("VT", "VERMONT");
            allStates.Add("WA", "WASHINGTON");
            allStates.Add("WI", "WISCONSIN");
            allStates.Add("WV", "WEST VIRGINIA");
            allStates.Add("WY", "WYOMING");

            return allStates;
        }

        public static List<System.Web.Mvc.SelectListItem> GetListUSStates()
        {
            return GetListUSStates(null);
            
        }

        public static List<System.Web.Mvc.SelectListItem> GetListUSStates(string SelectedState)
        {
            List<System.Web.Mvc.SelectListItem> returnList = new List<SelectListItem>();

            foreach (KeyValuePair<String, String> pair in GetAllUSStates())
            {
                SelectListItem li = new SelectListItem();
                li.Value = pair.Key;
                li.Text = pair.Value;
                li.Selected = !String.IsNullOrEmpty(SelectedState) && (String.Compare(SelectedState, pair.Key, true) == 0);
                returnList.Add(li);
            }

            return returnList;
        }
    }
}