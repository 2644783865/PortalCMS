﻿using Portal.CMS.Services.Authentication;
using Portal.CMS.Web.Architecture.ActionFilters;
using Portal.CMS.Web.Architecture.Helpers;
using Portal.CMS.Web.Areas.Admin.ViewModels.UserManager;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.Admin.Controllers
{
    [LoggedInFilter, AdminFilter]
    public class UserManagerController : Controller
    {
        #region Dependencies

        readonly IUserService _userService;
        readonly IRegistrationService _registrationService;
        readonly IRoleService _roleService;

        public UserManagerController(IUserService userService, IRegistrationService registrationService, IRoleService roleService)
        {
            _userService = userService;
            _registrationService = registrationService;
            _roleService = roleService;
        }

        #endregion Dependencies

        [HttpGet]
        public ActionResult Index()
        {
            var model = new UsersViewModel
            {
                Users = _userService.Get()
            };

            return View(model);
        }

        [HttpGet]
        [OutputCache(Duration = 86400)]
        public ActionResult Create()
        {
            return View("_Create", new CreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View("_Create", model);

            var userId = _registrationService.Register(model.EmailAddress, model.Password, model.GivenName, model.FamilyName);

            switch (userId.Value)
            {
                case -1:
                    ModelState.AddModelError("EmailAddressUsed", "The Email Address you entered is already registered");
                    return View("_Create", model);

                default:
                    if (_userService.GetUserCount() == 1)
                        _roleService.Update(userId.Value, new List<string> { nameof(Admin) });
                    else
                        _roleService.Update(userId.Value, new List<string> { "Authenticated" });

                    if (!UserHelper.IsLoggedIn)
                        Session.Add("UserAccount", _userService.GetUser(userId.Value));

                    return Content("Refresh");
            }
        }

        [HttpGet]
        public ActionResult Details(int userId)
        {
            var user = _userService.GetUser(userId);

            var model = new DetailsViewModel
            {
                UserId = userId,
                EmailAddress = user.EmailAddress,
                GivenName = user.GivenName,
                FamilyName = user.FamilyName,
                DateAdded = user.DateAdded,
                DateUpdated = user.DateUpdated
            };

            return View("_Details", model);
        }

        [HttpPost]
        public ActionResult Details(DetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return View("_Details", model);

            _userService.UpdateDetails(model.UserId, model.EmailAddress, model.GivenName, model.FamilyName);

            if (model.UserId == UserHelper.UserId)
            {
                Session.Remove("UserAccount");

                Session.Add("UserAccount", _userService.GetUser(model.UserId));
            }

            return Content("Refresh");
        }

        [HttpGet]
        public ActionResult Roles(int? userId)
        {
            var model = new RolesViewModel
            {
                UserId = userId.Value,
                RoleList = _roleService.Get()
            };

            var userRoles = _roleService.Get(userId);

            foreach (var role in userRoles)
                model.SelectedRoleList.Add(role.RoleName);

            return PartialView("_Roles", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Roles(RolesViewModel model)
        {
            if (!ModelState.IsValid)
                return View("_Roles", model);

            _roleService.Update(model.UserId.Value, model.SelectedRoleList);

            if (model.UserId == UserHelper.UserId)
            {
                Session.Remove("UserAccount");
                Session.Remove("UserRoles");

                Session.Add("UserAccount", _userService.GetUser(model.UserId.Value));
                Session.Add("UserRoles", _roleService.Get(model.UserId));
            }

            return Content("Refresh");
        }

        [HttpGet]
        public ActionResult Delete(int userId)
        {
            _userService.DeleteUser(userId);

            return RedirectToAction(nameof(Index));
        }
    }
}