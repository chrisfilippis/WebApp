using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Utilities
{
    public static class UserUtilities
    {
        /// <summary>
        /// Reset User's password
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="user"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static string ResetPassword(Users user, DataContainer db)
        {
            return ChangePassword(null, user, db);
        }

        /// <summary>
        /// Change User Password and returns new password
        /// </summary>
        /// <param name="pass">if null or string.Emty generate a random</param>
        /// <param name="user"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static string ChangePassword(string pass, Users user, DataContainer db)
        {
            var newPass = !string.IsNullOrWhiteSpace(pass) ? pass : Utilities.Helpers.GetRandomPassword();
            var hashArray = Utilities.Helpers.GetHashedPassword(newPass);

            var salt = hashArray[0];
            var password = hashArray[1];

            user.userSalt = salt;
            user.userPassword = password;
            db.SaveChanges();
            return newPass;
        }

        public static JsonEvent[] UserEvents(HttpContextBase context)
        {
            var userId = 0;
            if (Utilities.Helpers.IsLoggedIn(context, out userId))
            {
                using (var db = new DataContainer())
                {
                    var Events = db.Events.Where(x => x.Project.UserProjects.Select(g => g.upUserId).Contains(userId))
                        .ToArray()
                        .Select(x => new JsonEvent { Datetime = x.EventDate.ToString("dd/MM/yyyy"), Url = x.EventFriendlyUrl, Id = x.EventId, Descitpion = x.EventDescription, Title = x.EventTitle })
                        .ToArray();
                    db.Database.Connection.Close();
                    return Events;
                }
            }

            return new JsonEvent[] { };
        }

    }
}