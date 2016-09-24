﻿using Portal.CMS.Entities;
using Portal.CMS.Entities.Entities.Menu;
using System.Linq;

namespace Portal.CMS.Services.Menu
{
    public interface IMenuItemService
    {
        MenuItem Get(int menuItemId);

        int Create(int menuId, string linkText, string linkURL, string linkIcon);

        void Edit(int menuItemId, string linkText, string linkURL, string linkIcon);

        void Delete(int menuItemId);
    }

    public class MenuItemService : IMenuItemService
    {
        #region Dependencies

        private readonly PortalEntityModel _context;

        public MenuItemService(PortalEntityModel context)
        {
            _context = context;
        }

        #endregion Dependencies

        public MenuItem Get(int menuItemId)
        {
            var menuItem = _context.MenuItems.FirstOrDefault(x => x.MenuItemId == menuItemId);

            return menuItem;
        }

        public int Create(int menuId, string linkText, string linkURL, string linkIcon)
        {
            var newMenuItem = new Entities.Entities.Menu.MenuItem
            {
                MenuId = menuId,
                LinkText = linkText,
                LinkURL = linkURL,
                LinkIcon = linkIcon
            };

            _context.MenuItems.Add(newMenuItem);

            _context.SaveChanges();

            return newMenuItem.MenuItemId;
        }

        public void Edit(int menuItemId, string linkText, string linkURL, string linkIcon)
        {
            var menuItem = _context.MenuItems.SingleOrDefault(x => x.MenuItemId == menuItemId);

            if (menuItem == null)
                return;

            menuItem.LinkText = linkText;
            menuItem.LinkURL = linkURL;
            menuItem.LinkIcon = linkIcon;

            _context.SaveChanges();
        }

        public void Delete(int menuItemId)
        {
            var menuItem = _context.MenuItems.SingleOrDefault(x => x.MenuItemId == menuItemId);

            if (menuItem == null)
                return;

            _context.MenuItems.Remove(menuItem);

            _context.SaveChanges();
        }
    }
}