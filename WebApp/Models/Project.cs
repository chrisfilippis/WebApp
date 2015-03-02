using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Project
    {
        public ProjectDetails Details { get; set; }

        public CommentDetail[] Comments { get; set; }

        public FileLookupDetail[] Files { get; set; }
    }

    public class CommentDetail
    {
        public Comments Comment { get; set; }

        public Users Author { get; set; }
    }

    public class FileLookupDetail
    {
        public string Path { get; set; }

        public string User { get; set; }

        public DateTime Datetime { get; set; }
    }
}