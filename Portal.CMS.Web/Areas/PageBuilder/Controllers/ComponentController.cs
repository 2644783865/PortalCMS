﻿using LogBook.Services;
using LogBook.Services.Models;
using Portal.CMS.Entities.Enumerators;
using Portal.CMS.Services.Generic;
using Portal.CMS.Services.PageBuilder;
using Portal.CMS.Web.Architecture.ActionFilters;
using Portal.CMS.Web.Architecture.Extensions;
using Portal.CMS.Web.Architecture.Helpers;
using Portal.CMS.Web.Areas.PageBuilder.ViewModels.Component;
using Portal.CMS.Web.ViewModels.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.SessionState;

namespace Portal.CMS.Web.Areas.PageBuilder.Controllers
{
    [AdminFilter(ActionFilterResponseType.Modal)]
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ComponentController : Controller
    {
        #region Dependencies

        private readonly IPageSectionService _pageSectionService;
        private readonly IPageComponentService _pageComponentService;
        private readonly IImageService _imageService;
        private readonly LogHandler _logHandler;

        public ComponentController(IPageSectionService pageSectionService, IPageComponentService pageComponentService, IImageService imageService, LogHandler logHandler)
        {
            _pageSectionService = pageSectionService;
            _pageComponentService = pageComponentService;
            _imageService = imageService;
            _logHandler = logHandler;
        }

        #endregion Dependencies

        [HttpGet]
        [OutputCache(Duration = 60)]
        public ActionResult Add()
        {
            var model = new AddViewModel
            {
                PageComponentTypeList = _pageComponentService.GetComponentTypes()
            };

            return View("_Add", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Add(int pageSectionId, string containerElementId, string elementBody)
        {
            elementBody = elementBody.Replace("animated bounce", string.Empty);

            _pageComponentService.Add(pageSectionId, containerElementId, elementBody);

            return Json(new { State = true });
        }

        [HttpPost]
        public ActionResult Delete(int pageSectionId, string elementId)
        {
            try
            {
                _pageComponentService.Delete(pageSectionId, elementId);

                return Json(new { State = true });
            }
            catch (Exception ex)
            {
                return Json(new { State = false, Message = ex.InnerException });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int pageSectionId, string elementId, string elementHtml)
        {
            _pageComponentService.EditElement(pageSectionId, elementId, elementHtml);

            return Content("Refresh");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Link(int pageSectionId, string elementId, string elementHtml, string elementHref, string elementTarget)
        {
            _pageComponentService.EditAnchor(pageSectionId, elementId, elementHtml, elementHref, elementTarget);

            return Content("Refresh");
        }

        [HttpGet]
        public async Task<ActionResult> EditImage(int pageSectionId, string elementId, string elementType)
        {
            var pageSection = await _pageSectionService.GetAsync(pageSectionId);

            var imageList = _imageService.Get();

            var model = new ImageViewModel
            {
                SectionId = pageSectionId,
                ElementType = elementType,
                ElementId = elementId,
                GeneralImages = new PaginationViewModel
                {
                    PaginationType = "general",
                    TargetInputField = "SelectedImageId",
                    ImageList = imageList.Where(x => x.ImageCategory == ImageCategory.General)
                },
                IconImages = new PaginationViewModel
                {
                    PaginationType = "icon",
                    TargetInputField = "SelectedImageId",
                    ImageList = imageList.Where(x => x.ImageCategory == ImageCategory.Icon)
                },
                ScreenshotImages = new PaginationViewModel
                {
                    PaginationType = "screenshot",
                    TargetInputField = "SelectedImageId",
                    ImageList = imageList.Where(x => x.ImageCategory == ImageCategory.Screenshot)
                },
                TextureImages = new PaginationViewModel
                {
                    PaginationType = "texture",
                    TargetInputField = "SelectedImageId",
                    ImageList = imageList.Where(x => x.ImageCategory == ImageCategory.Texture)
                }
            };

            return View("_EditImage", model);
        }

        [HttpPost]
        public JsonResult EditImage(ImageViewModel model)
        {
            try
            {
                var selectedImage = _imageService.Get(model.SelectedImageId);

                _pageComponentService.EditImage(model.SectionId, model.ElementType, model.ElementId, selectedImage.CDNImagePath());

                return Json(new { State = true, Source = selectedImage.CDNImagePath() });
            }
            catch (Exception)
            {
                return Json(new { State = false });
            }
        }

        [HttpPost]
        public JsonResult Clone(int pageSectionId, string elementId, string componentStamp)
        {
            try
            {
                _pageComponentService.CloneElement(pageSectionId, elementId, componentStamp);

                return Json(new { State = true });
            }
            catch (Exception ex)
            {
                _logHandler.WriteLog(LogType.Error, "Portal CMS", ex, ex.Message, $"{UserHelper.FullName} {UserHelper.UserId}");

                return Json(new { State = false });
            }
        }

        [HttpGet]
        public ActionResult EditVideo(int pageSectionId, string widgetWrapperElementId, string videoPlayerElementId)
        {
            var model = new VideoViewModel
            {
                SectionId = pageSectionId,
                WidgetWrapperElementId = widgetWrapperElementId,
                VideoPlayerElementId = videoPlayerElementId,
                VideoUrl = string.Empty
            };

            return View("_EditVideo", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditVideo(VideoViewModel model)
        {
            try
            {
                _pageComponentService.EditSource(model.SectionId, model.VideoPlayerElementId, model.VideoUrl);

                return Json(new { State = true });
            }
            catch (Exception)
            {
                return Json(new { State = false });
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditFreestyle(int pageSectionId, string elementId, string elementHtml)
        {
            // REPLACE: MCE Tokens
            elementHtml = elementHtml.Replace("ui-draggable ui-draggable-handle mce-content-body", string.Empty);
            elementHtml = elementHtml.Replace("contenteditable=\"true\" spellcheck=\"false\"", string.Empty);

            _pageComponentService.EditElement(pageSectionId, elementId, elementHtml);

            return Content("Refresh");
        }

        [HttpGet]
        public ActionResult EditContainer(int pageSectionId, string elementId)
        {
            var model = new ContainerViewModel
            {
                SectionId = pageSectionId,
                ElementId = elementId
            };

            return View("_EditContainer", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditContainer(ContainerViewModel model)
        {
            _pageSectionService.EditAnimationAsync(model.SectionId, model.ElementId, model.Animation.ToString());

            return Content("Refresh");
        }
    }
}