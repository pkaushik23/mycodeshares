using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace onlyFbLogin.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string ReturnUrl)
        {
            ViewData["ReturnUrl"] = ReturnUrl;
            return View();
        }

        public async Task FbLogin(string ReturnUrl)
        {
            //this causes correlation error. call back and redirect should be different.
            //await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme);

            //therfor challenge config should specify where users should be redirected once auth is complete.
            await HttpContext.ChallengeAsync(FacebookDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = new PathString(ReturnUrl)
            });
        }
    }
}
