using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Pocos
{
    [Serializable]
    public class AjaxResponse
    {
        public bool success { get; set; }

        public string message { get; set; }

        public List<Error> errors { get; set; }

        public bool reload { get; set; }

    }

    [Serializable]
    public class Error
    {
        public string field { get; set; }

        public string error { get; set; }
    }

}