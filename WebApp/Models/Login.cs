using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Login
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public int UserGroupId { get; set; }

        public bool IsValid(string _email, string _password)
        {
            using (var db = new DataContainer())
            {
                var user = db.Users.Where(x => x.userEmail == _email && !(x.userIsAnonymous ?? false) && x.userIsActive).FirstOrDefault();

                if (user != null)
                {
                    this.UserGroupId = user.userGroupId;
                }

                return user != null;
            }

        }
    }
}