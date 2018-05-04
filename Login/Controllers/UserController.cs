using Login.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Login.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified, ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";

            //model validation
            if (ModelState.IsValid)
            {
                #region //email already exsists
                var isExist = IsEmailExsist(user.EmailID);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exists");
                    return View(user);
                }

                #region generate activation code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailVerified = false;
            }
            else
            {
                message = "Invalied request";
            }
            

            //generate activation code

            //password hashing

            //save data to database

            //send email to user


            return View(user);
        }


        //Verify Email


        //Verify Email Link


        //Login


        //Login POST


        //LOgout

        [NonAction]
        public bool IsEmailExsist(string emailID)
        {
            using (UserDBEntities db = new UserDBEntities())
            {
                var v = db.Users.Where(a => a.EmailID == emailID).FirstOrDefault();
                return v != null;
            }
        }

       
    }
}