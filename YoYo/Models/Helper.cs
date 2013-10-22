using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace YoYo.Models
{
    public class Helper
    {

        public static Models.User GetUser()
        {   
            YoyoEntities db = new YoyoEntities();

            string username = Membership.GetUser().UserName;

            Models.User u = db.Users.Where(x => x.UserName == username).FirstOrDefault();

            return u;
            
        }
    }
}