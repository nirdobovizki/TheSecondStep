using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAuthDemo.Models
{
    public class User
    {
        public string Name { get; set; }
        public bool CheckPassword(string value)
        {
            return value == "demo";
        }
        public string TotpSecret { get; set; }
        public bool TwoFactorAuthEnabled { get; set; }
    }
}