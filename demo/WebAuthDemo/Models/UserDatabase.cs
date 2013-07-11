using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAuthDemo.Models
{
    public class UserDatabase
    {
        static UserDatabase()
        {
            Users = new List<User>() { new User { Name = "demo" } };
        }
        public static IList<User> Users {get; private set;}
    }
}