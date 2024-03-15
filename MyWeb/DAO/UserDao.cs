
using MyWeb.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyWeb.DAO
{
    
    public class UserDao
    {
        MYWEBEntities1 db = null;
        public UserDao()
        {
            db = new MYWEBEntities1();
        }

        public long InsertUserFb (User entity)
        {
            var user = db.Users.SingleOrDefault( x=> x.username == entity.username );
            if(user == null)
            {
                db.Users.Add(entity);
                db.SaveChanges();
                return entity.ID;
            }
            else
            {
                return user.ID;
            }
        }
    }
}