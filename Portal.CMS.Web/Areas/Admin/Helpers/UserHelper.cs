﻿using Portal.CMS.Entities.Entities.Authentication;
using Portal.CMS.Services.Authentication;
using System.Linq;

namespace Portal.CMS.Web.Areas.Admin.Helpers
{
    public static class UserHelper
    {
        public static bool IsLoggedIn
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["UserAccount"] == null)
                    return false;
                else
                    return true;
            }
        }

        public static bool IsAdmin
        {
            get
            {
                User userSession = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                if (userSession == null)
                    return false;

                var userService = new UserService(new Entities.PortalEntityModel());

                var userAccount = userService.GetUser(userSession.UserId);

                if (userAccount.Roles.Any(x => x.Role.RoleName == "Admin"))
                    return true;
                else
                    return false;
            }
        }

        public static int UserId
        {
            get
            {
                User userAccount = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                return userAccount.UserId;
            }
        }

        public static string AvatarImagePath
        {
            get
            {
                User userAccount = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                if (string.IsNullOrWhiteSpace(userAccount.AvatarImagePath))
                    return "/Areas/Admin/Content/Images/profile-image-male.png";
                else
                    return userAccount.AvatarImagePath;
            }
        }

        public static string FullName
        {
            get
            {
                User userAccount = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                return string.Format("{0} {1}", userAccount.GivenName, userAccount.FamilyName);
            }
        }

        public static string GivenName
        {
            get
            {
                User userAccount = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                return userAccount.GivenName;
            }
        }

        public static string FamilyName
        {
            get
            {
                User userAccount = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                return userAccount.FamilyName;
            }
        }

        public static string EmailAddress
        {
            get
            {
                User userAccount = (User)System.Web.HttpContext.Current.Session["UserAccount"];

                return userAccount.EmailAddress;
            }
        }
    }
}