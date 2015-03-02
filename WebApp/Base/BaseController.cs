using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Utilities;

namespace WebApp.Base
{
    public class BaseController : Controller
    {
        protected int LanguageId
        {
            get { return LanguageResolver.GetLanguage((string)ControllerContext.RouteData.Values["language"]); }
        }
    }
}