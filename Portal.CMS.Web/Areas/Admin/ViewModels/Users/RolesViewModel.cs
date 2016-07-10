﻿using Portal.CMS.Entities.Entities.Authentication;
using System.Collections.Generic;
using System.ComponentModel;

namespace Portal.CMS.Web.Areas.Admin.ViewModels.Users
{
    public class RolesViewModel
    {
        public int UserId { get; set; }

        [DisplayName("Roles")]
        public List<string> SelectedRoleList { get; set; } = new List<string>();

        public List<Role> RoleList { get; set; }
    }
}