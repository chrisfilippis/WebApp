using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class TextConstant
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Datetime { get; set; }

        public TextConstantValues[] TextConstantValues { get; set; }
    }
}