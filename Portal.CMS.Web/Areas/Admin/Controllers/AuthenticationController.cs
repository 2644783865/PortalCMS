﻿using Portal.CMS.Services.Authentication;
using Portal.CMS.Web.Areas.Admin.ViewModels.Authentication;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.Admin.Controllers
{
    public class AuthenticationController : Controller
    {
        #region Dependencies

        private readonly ILoginService _loginService;
        private readonly IRegistrationService _registrationService;
        private readonly IUserService _userservice;
        private readonly IRoleService _roleService;

        public AuthenticationController(LoginService loginService, RegistrationService registrationService, UserService userService, RoleService roleService)
        {
            _loginService = loginService;
            _registrationService = registrationService;
            _userservice = userService;
            _roleService = roleService;
        }

        #endregion Dependencies

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View("_Login", new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("_Login", model);

            var userId = _loginService.Login(model.EmailAddress, model.Password);

            if (!userId.HasValue)
            {
                ModelState.AddModelError("InvalidCredentials", "Invalid Account Credentials");

                return View("_Login", model);
            }

            var userAccount = _userservice.GetUser(userId.Value);

            Session.Add("UserAccount", userAccount);

            return this.Content("Refresh");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View("_Register", new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("_Register", model);

            var userId = _registrationService.Register(model.EmailAddress, model.Password, model.GivenName, model.FamilyName);

            switch (userId.Value)
            {
                case -1:
                    ModelState.AddModelError("EmailAddressUsed", "The Email Address you entered is already registered");
                    return View("_Register", model);

                default:
                    if (_userservice.GetUserCount() == 1)
                        _roleService.Update(userId.Value, new List<string>() { "Admin", "Authenticated" });
                    else
                        _roleService.Update(userId.Value, new List<string>() { "Authenticated" });

                    var userAccount = _userservice.GetUser(userId.Value);

                    Session.Add("UserAccount", userAccount);

                    return this.Content("Refresh");
            }
        }
    }
}