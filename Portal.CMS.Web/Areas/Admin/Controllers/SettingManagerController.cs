﻿using System.Web.Mvc;
using Portal.CMS.Services.Settings;
using Portal.CMS.Web.Architecture.ActionFilters;
using Portal.CMS.Web.Architecture.Helpers;
using Portal.CMS.Web.Areas.Admin.ViewModels.SettingManager;

namespace Portal.CMS.Web.Areas.Admin.Controllers
{
    [AdminFilter(ActionFilterResponseType.Page)]
    public class SettingManagerController : Controller
    {
        #region Dependencies

        private readonly ISettingService _settingService;

        public SettingManagerController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        #endregion Dependencies

        [HttpGet]
        public ActionResult Setup()
        {
            var model = new SetupViewModel
            {
                WebsiteName = SettingHelper.Get("Website Name"),
                WebsiteDescription = SettingHelper.Get("Description Meta Tag"),
                GoogleAnalyticsId = SettingHelper.Get("Google Analytics Tracking ID"),
                EmailFromAddress = SettingHelper.Get("Email From Address"),
                SendGridUserName = SettingHelper.Get("SendGrid UserName"),
                SendGridPassword = SettingHelper.Get("SendGrid Password"),
                CDNAddress = SettingHelper.Get("CDN Address")
            };

            if (string.IsNullOrWhiteSpace(model.EmailFromAddress))
                model.EmailFromAddress = UserHelper.EmailAddress;

            return View("_Setup", model);
        }

        [HttpPost]
        public ActionResult Setup(SetupViewModel model)
        {
            if (!ModelState.IsValid)
                return View("_Setup", model);

            _settingService.Edit("Website Name", model.WebsiteName);
            Session.Remove("Setting-Website Name");

            _settingService.Edit("Description Meta Tag", model.WebsiteDescription);
            Session.Remove("Setting-Description Meta Tag");

            _settingService.Edit("Google Analytics Tracking ID", model.GoogleAnalyticsId);
            Session.Remove("Setting-Google Analytics Tracking ID");

            _settingService.Edit("Email From Address", model.EmailFromAddress);
            Session.Remove("Setting-Email From Address");

            _settingService.Edit("SendGrid UserName", model.SendGridUserName);
            Session.Remove("Setting-SendGrid UserName");

            _settingService.Edit("SendGrid Password", model.SendGridPassword);
            Session.Remove("Setting-SendGrid Password");

            _settingService.Edit("CDN Address", model.CDNAddress);
            Session.Remove("Setting-CDN Address");

            return Content("Refresh");
        }
    }
}