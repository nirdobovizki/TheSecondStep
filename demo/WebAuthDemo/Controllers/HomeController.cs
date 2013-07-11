using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAuthDemo.Models;

namespace WebAuthDemo.Controllers
{
    public class HomeController : Controller
    {
        // because this is just a stupid demo app we don't have logged in user managment
        // we fake it with something that sort-of works when running with single user
        // access (like under Visual Studio)
        private static User _currentUser;

        public ActionResult Index()
        {
            if (_currentUser == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.User = _currentUser;
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult LoginAjax(string userName, string password)
        {
            var user = UserDatabase.Users.Where(o => string.Compare(o.Name, userName, StringComparison.InvariantCultureIgnoreCase) == 0).SingleOrDefault();
            if (user == null)
            {
                return Json(new { result = "fail" });
            }

            if (!user.CheckPassword(password))
            {
                return Json(new { result = "fail" });
            }

            _currentUser = user;

            if (user.TwoFactorAuthEnabled)
            {
                return Json(new { result = "twostep" });
            }

            return Json(new { result = "success" });
        }

        public ActionResult LoginStepTwoAjax(bool fallback, string code)
        {
            if (!fallback)
            {
                int intCode;
                if (int.TryParse(code, out intCode))
                {
                    if (TheSecondStep.MobileApp.Authenticate(
                        TheSecondStep.MobileApp.DefaultSystemSettings,
                        new TheSecondStep.MobileApp.MobileAppUserSettings { Secret = _currentUser.TotpSecret }, 
                        intCode))
                    {
                        return Json(new { result = "success" });
                    }
                }
                return Json(new { result = "fail" });
            }
            else
            {
                // TODO
                return Json(new { result = "fail" });
            }
        }

        public ActionResult Logout()
        {
            _currentUser = null;
            return RedirectToAction("Index");
        }

        public ActionResult EnableTwoStep()
        {
            TheSecondStep.MobileApp.MobileAppUserSettings secret = TheSecondStep.MobileApp.CreateNewSecret(TheSecondStep.MobileApp.DefaultSystemSettings);
            _currentUser.TotpSecret = secret.Secret;
            ViewBag.Secret = secret.Secret;
            ViewBag.QrCode = "http://chart.apis.google.com/chart?chs=400x400&chld=M&cht=qr&chl=" + Uri.EscapeDataString(
                TheSecondStep.MobileApp.GetSecretUrl(TheSecondStep.MobileApp.DefaultSystemSettings,secret,"The Second Step Demo"));
            return View();
        }

        public ActionResult EnableTwoStepAjax(string code)
        {
            int intCode;
            if (int.TryParse(code, out intCode))
            {
                if (TheSecondStep.MobileApp.Authenticate(
                        TheSecondStep.MobileApp.DefaultSystemSettings,
                        new TheSecondStep.MobileApp.MobileAppUserSettings { Secret = _currentUser.TotpSecret },
                        intCode))
                {
                    _currentUser.TwoFactorAuthEnabled = true;
                    return Json(new { result = "success" });
                }
            }
            return Json(new { result = "fail" });
        }
    }
}
