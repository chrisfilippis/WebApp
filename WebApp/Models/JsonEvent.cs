using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class JsonEvent
    {
        public string Descitpion { get; set; }

        public int Id { get; set; }

        public string Datetime { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }
    }
}