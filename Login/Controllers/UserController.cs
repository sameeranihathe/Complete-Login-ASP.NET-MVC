﻿using Login.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
                #endregion

                #region generate activation code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region password hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailVerified = false;

                #region save to database
                using (UserDBEntities db = new UserDBEntities())
                {
                    db.Users.Add(user);
                    db.SaveChanges();

                    //send details to user via email
                    sendverificationEmail(user.EmailID, user.ActivationCode.ToString());
                    message = "Registration successfully completed. Check your email to verify the account.";
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Invalied request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;


            return View(user);
        }


        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;

            using (UserDBEntities db = new UserDBEntities())
            {
                db.Configuration.ValidateOnSaveEnabled = false;

                var v = db.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v!= null)
                {
                    v.IsEmailVerified = true;
                    db.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalied Request";
                }

            }
            ViewBag.Status = true;
            return View();
        }

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

        [NonAction]
        public void sendverificationEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("sameeranihathe@gmail.com", "sameera sampath");
            var toEmail = new MailAddress(emailID);
            var fromemailPassword = "kanchana143";
            string subject = "Your account is successfully created";

            string body = "<br/> <br/> Your account is successfully created. Please click the below link to verify the account" +
                "<br/> <br/> <a href='" + link + "'>" + link + "</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromemailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }


    }
}