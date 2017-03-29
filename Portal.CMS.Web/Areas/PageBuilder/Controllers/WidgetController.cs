﻿using Portal.CMS.Services.Posts;
using Portal.CMS.Web.Architecture.Helpers;
using Portal.CMS.Web.Areas.PageBuilder.ViewModels.Widget;
using System.Linq;
using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.PageBuilder.Controllers
{
    public class WidgetController : Controller
    {
        #region Dependencies

        readonly IPostService _postService;

        public WidgetController(IPostService postService)
        {
            _postService = postService;
        }

        #endregion Dependencies

        public ActionResult RecentPostList()
        {
            var model = new PostListViewModel
            {
                PostList = _postService.Read(UserHelper.UserId, string.Empty).Take(6).ToList()
            };

            return View("_RecentPostList", model);
        }
    }
}