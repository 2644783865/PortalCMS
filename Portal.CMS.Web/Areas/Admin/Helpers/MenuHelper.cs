﻿using Portal.CMS.Entities.Entities.Menu;
using Portal.CMS.Services.Menu;
using Portal.CMS.Web.DependencyResolution;
using StructureMap;
using System.Collections.Generic;

namespace Portal.CMS.Web.Areas.Admin.Helpers
{
    public static class MenuHelper
    {
        public static List<MenuItem> Get(string menuName)
        {
            IContainer container = IoC.Initialize();

            IMenuService menuService = container.GetInstance<MenuService>();

            var menuItems = menuService.View(UserHelper.UserId, menuName);

            return menuItems;
        }
    }
}