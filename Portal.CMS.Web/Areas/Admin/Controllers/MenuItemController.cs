﻿using Portal.CMS.Services.Authentication;
using Portal.CMS.Services.Menu;
using Portal.CMS.Services.PageBuilder;
using Portal.CMS.Services.Posts;
using Portal.CMS.Web.Architecture.ActionFilters;
using Portal.CMS.Web.Areas.Admin.ViewModels.MenuItem;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.Admin.Controllers
{
    [AdminFilter(ActionFilterResponseType.Modal)]
    public class MenuItemController : Controller
    {
        #region Dependencies

        private readonly IMenuService _menuService;
        private readonly IMenuItemService _menuItemService;
        private readonly IPageService _pageService;
        private readonly IPostService _postService;
        private readonly IRoleService _roleService;

        public MenuItemController(IMenuService menuService, IMenuItemService menuItemService, IPageService pageService, IPostService postService, IRoleService roleService)
        {
            _menuService = menuService;
            _menuItemService = menuItemService;
            _pageService = pageService;
            _postService = postService;
            _roleService = roleService;
        }

        #endregion Dependencies

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var model = new CreateViewModel
            {
                MenuList = _menuService.Get(),
                PageList = _pageService.Get(),
                PostList = _postService.Get(string.Empty, true),
                RoleList = await _roleService.GetAsync()
            };

            return View("_Create", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.MenuList = _menuService.Get();
                model.PageList = _pageService.Get();
                model.PostList = _postService.Get(string.Empty, true);
                model.RoleList = await _roleService.GetAsync();

                return View("_Create", model);
            }

            var menuItemId = _menuItemService.Create(model.MenuId, model.LinkText, model.LinkURL, model.LinkIcon);

            _menuItemService.Roles(menuItemId, model.SelectedRoleList);

            return Content("Refresh");
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int menuItemId)
        {
            var menuItem = _menuItemService.Get(menuItemId);

            var model = new EditViewModel
            {
                MenuItemId = menuItem.MenuItemId,
                LinkText = menuItem.LinkText,
                LinkIcon = menuItem.LinkIcon,
                LinkURL = menuItem.LinkURL,
                RoleList = await _roleService.GetAsync(),
                SelectedRoleList = menuItem.MenuItemRoles.Select(x => x.Role.RoleName).ToList()
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.RoleList = await _roleService.GetAsync();

                return View(model);
            }

            _menuItemService.Edit(model.MenuItemId, model.LinkText, model.LinkURL, model.LinkIcon);

            _menuItemService.Roles(model.MenuItemId, model.SelectedRoleList);

            return Content("Refresh");
        }

        [HttpGet]
        public ActionResult Delete(int menuItemId)
        {
            _menuItemService.Delete(menuItemId);

            return Redirect(HttpContext.Request.UrlReferrer.ToString());
        }
    }
}