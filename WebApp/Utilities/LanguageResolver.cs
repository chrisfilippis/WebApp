using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Utilities
{
    public static class LanguageResolver
    {
        private static Dictionary<int, string> Languages = new Dictionary<int, string>() { { 1, "gr" }, { 2, "en" } };

        public static int GetLanguage(object objectCode)
        {
            return GetLanguage(objectCode as string);
        }

        public static string GetLanguageCode(int id)
        {
            return Languages.Where(x => x.Key == id).Select(x => x.Value).FirstOrDefault();
        }

        public static int GetLanguage(string code)
        {
            return Languages.Where(x => x.Value == code).Select(x => x.Key).FirstOrDefault();
        }
    }
}