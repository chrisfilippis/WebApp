using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using WebApp.Utilities;

namespace WebApp.Helpers
{
    public static class Helpers
    {
        public static MvcHtmlString GetTextConstant(this HtmlHelper helper, string key, int languageId, string defaultValue = "")
        {
            return new MvcHtmlString(Strings.GetTextConstant(key, languageId, defaultValue));
        }

        public static string GetTextConstant(string key, int languageId, string defaultValue = "")
        {
            return Strings.GetTextConstant(key, languageId, defaultValue);
        }
    }
}