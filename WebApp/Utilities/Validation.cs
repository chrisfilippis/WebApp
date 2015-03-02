using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApp.Utilities
{
    public static class Validation
    {
        /// <summary>
        /// RegEx For Email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidMail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
            {
                return false;
            }

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            return match.Success;
        }

    }
}
