using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ProjectList
    {
        public Users User { get; set; }

        public ProjectDetails[] Projects { get; set; }

    }

    public class ProjectDetails
    {
        public Users[] Supervisors { get; set; }

        public Projects Project { get; set; }

        public Users[] Students { get; set; }
    }
}