using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ProfileDetails
    {
        public Users User { get; set; }

        public Projects[] Projects { get; set; }
    }
}