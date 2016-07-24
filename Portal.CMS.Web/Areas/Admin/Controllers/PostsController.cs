﻿using Portal.CMS.Entities.Entities.Generic;
using Portal.CMS.Entities.Entities.Posts;
using Portal.CMS.Services.Authentication;
using Portal.CMS.Services.Generic;
using Portal.CMS.Services.Posts;
using Portal.CMS.Web.Areas.Admin.ActionFilters;
using Portal.CMS.Web.Areas.Admin.Helpers;
using Portal.CMS.Web.Areas.Admin.ViewModels.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.Admin.Controllers
{
    [LoggedInFilter, AdminFilter]
    public class PostsController : Controller
    {
        #region Dependencies

        private readonly IPostService _postService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        private readonly IPostCategoryService _postCategoryService;
        private readonly IUserService _userService;

        public PostsController(IPostService postService, IPostImageService postImageService, IImageService imageService, IPostCategoryService postCategoryService, IUserService userService)
        {
            _postService = postService;
            _postImageService = postImageService;
            _imageService = imageService;
            _postCategoryService = postCategoryService;
            _userService = userService;
        }

        #endregion Dependencies

        [HttpGet]
        public ActionResult Index()
        {
            var model = new PostsViewModel()
            {
                Posts = _postService.Get(string.Empty, false),
                PostCategories = _postCategoryService.Get()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var postCategories = _postCategoryService.Get();

            var model = new CreatePostViewModel()
            {
                PostCategoryId = postCategories.First().PostCategoryId,
                PostAuthorUserId = UserHelper.UserId,
                PostCategoryList = postCategories,
                UserList = _userService.Get(new List<string>() { "Admin" }),
                PublicationState = PublicationState.Published,
                ImageList = _imageService.Get(),
            };

            return View("_Create", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ImageList = _imageService.Get();
                model.PostCategoryList = _postCategoryService.Get();
                model.UserList = _userService.Get(new List<string>() { "Admin" });

                return View("_Create", model);
            }

            var postId = _postService.Create(model.PostTitle, model.PostCategoryId, model.PostAuthorUserId, model.PostDescription, model.PostBody);

            if (model.PublicationState == PublicationState.Published)
                _postService.Publish(postId);

            UpdateBanner(postId, model.BannerImageId);
            UpdateGallery(postId, model.GalleryImageList);

            return this.Content("Refresh");
        }

        [HttpGet]
        public ActionResult Edit(int postId)
        {
            var post = _postService.Get(postId);

            var model = new EditPostviewModel()
            {
                PostId = post.PostId,
                PostTitle = post.PostTitle,
                PostDescription = post.PostDescription,
                PostBody = post.PostBody,
                PostCategoryId = post.PostCategoryId,
                PostAuthorUserId = post.PostAuthorUserId,
                ExistingGalleryImageList = post.PostImages.Where(x => x.PostImageType == PostImageType.Gallery).Select(x => x.ImageId).ToList(),
                PostCategoryList = _postCategoryService.Get(),
                UserList = _userService.Get(new List<string>() { "Admin" }),
                ImageList = _imageService.Get(),
            };

            if (post.IsPublished)
                model.PublicationState = Entities.Entities.Generic.PublicationState.Published;

            if (post.PostImages.Any(x => x.PostImageType == PostImageType.Banner))
                model.BannerImageId = post.PostImages.First(x => x.PostImageType == PostImageType.Banner).ImageId;

            return View("_Edit", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditPostviewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ImageList = _imageService.Get();
                model.PostCategoryList = _postCategoryService.Get();
                model.UserList = _userService.Get(new List<string>() { "Admin" });

                return View("_Edit", model);
            }

            _postService.Edit(model.PostId, model.PostTitle, model.PostCategoryId, model.PostAuthorUserId, model.PostDescription, model.PostBody);

            if (model.PublicationState == Entities.Entities.Generic.PublicationState.Published)
                _postService.Publish(model.PostId);
            else
                _postService.Draft(model.PostId);

            UpdateBanner(model.PostId, model.BannerImageId);
            UpdateGallery(model.PostId, model.GalleryImageList);

            return this.Content("Refresh");
        }

        [HttpGet]
        public ActionResult Delete(int postId)
        {
            _postService.Delete(postId);

            return RedirectToAction("Index", "Posts");
        }

        [HttpGet]
        public ActionResult Publish(int postId)
        {
            _postService.Publish(postId);

            return RedirectToAction("Index", "Posts");
        }

        [HttpGet]
        public ActionResult Draft(int postId)
        {
            _postService.Draft(postId);

            return RedirectToAction("Index", "Posts");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Inline(int postId, string markup)
        {
            _postService.Inline(postId, markup);

            return Content("Refresh");
        }

        #region Private Methods

        private void UpdateBanner(int postId, int bannerImageId)
        {
            if (bannerImageId != 0)
            {
                _postImageService.Wipe(postId, PostImageType.Banner);

                _postImageService.Add(postId, bannerImageId, PostImageType.Banner);
            }
        }

        private void UpdateGallery(int postId, string galleryImageIds)
        {
            if (!string.IsNullOrWhiteSpace(galleryImageIds))
            {
                _postImageService.Wipe(postId, PostImageType.Gallery);

                var galleryImageList = galleryImageIds.Split(',');

                foreach (var image in galleryImageList)
                    _postImageService.Add(postId, Convert.ToInt32(image), PostImageType.Gallery);
            }
        }

        #endregion Private Methods
    }
}