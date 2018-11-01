using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LRC_NET_Framework
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public const int MaxRecordsInPhoneHistory = 10;
        public const int MaxRecordsInAddressHistory = 5;
        public const int MaxRecordsInEmailHistory = 10;
        public const int MaxRecordsIn_MembershipFormsHistory = 5;
        public const int MaxRecordsIn_CopeFormsHistory = 5;
        public const string MembershipFormsFolder = "~/Content/MembershipForms/";
        public const string CopeFormsFolder = "~/Content/CopeForms/";
        public const string BuildingsFolder = "~/Content/BuildingForms/";
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
