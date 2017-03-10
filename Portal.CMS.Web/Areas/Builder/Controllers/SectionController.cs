﻿using System;
using System.Linq;
using System.Web.Mvc;
using Portal.CMS.Services.Authentication;
using Portal.CMS.Services.Generic;
using Portal.CMS.Services.PageBuilder;
using Portal.CMS.Web.Architecture.ActionFilters;
using Portal.CMS.Web.Areas.Builder.ViewModels.Section;
using Portal.CMS.Web.ViewModels.Shared;

namespace Portal.CMS.Web.Areas.Builder.Controllers
{
    [AdminFilter]
    public class SectionController : Controller
    {
        #region Dependencies

        private readonly IPageSectionService _pageSectionService;
        private readonly IPageSectionTypeService _pageSectionTypeService;
        private readonly IImageService _imageService;
        private readonly IRoleService _roleService;

        public SectionController(IPageSectionService pageSectionService, IPageSectionTypeService pageSectionTypeService, IImageService imageService, IRoleService roleService)
        {
            _pageSectionService = pageSectionService;
            _pageSectionTypeService = pageSectionTypeService;
            _imageService = imageService;
            _roleService = roleService;
        }

        #endregion Dependencies

        [HttpGet]
        public ActionResult Add(int pageId)
        {
            var model = new AddViewModel
            {
                PageId = pageId,
                SectionTypeList = _pageSectionTypeService.Get()
            };

            return View("_Add", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(int pageId, int pageSectionTypeId, string componentStamp)
        {
            try
            {
                var pageSectionId = _pageSectionService.Add(pageId, pageSectionTypeId, componentStamp);

                return new JsonResult { Data = pageSectionId };
            }
            catch (Exception ex)
            {
                return Json(new { State = false, ex.InnerException.Message });
            }
        }

        [HttpGet]
        public ActionResult Edit(int sectionId)
        {
            var pageSection = _pageSectionService.Get(sectionId);

            var model = new EditViewModel
            {
                SectionId = sectionId,
                MediaLibrary = new PaginationViewModel
                {
                    ImageList = _imageService.Get(),
                    TargetInputField = "BackgroundImageId",
                    PaginationType = "section"
                },
                PageSectionHeight = _pageSectionService.DetermineSectionHeight(sectionId),
                PageSectionBackgroundStyle = _pageSectionService.DetermineBackgroundStyle(sectionId),
                BackgroundType = _pageSectionService.DetermineBackgroundType(sectionId),
                BackgroundColour = _pageSectionService.DetermineBackgroundColour(sectionId),
                RoleList = _roleService.Get(),
                SelectedRoleList = pageSection.PageSectionRoles.Select(x => x.Role.RoleName).ToList()
            };

            return View("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(EditViewModel model)
        {
            try
            {
                if ("colour".Equals(model.BackgroundType, StringComparison.OrdinalIgnoreCase))
                {
                    _pageSectionService.SetBackgroundType(model.SectionId, false);

                    if (!string.IsNullOrWhiteSpace(model.BackgroundColour))
                        _pageSectionService.Background(model.SectionId, model.BackgroundColour);
                }
                else
                {
                    _pageSectionService.SetBackgroundType(model.SectionId, true);

                    if (model.BackgroundImageId > 0)
                        _pageSectionService.Background(model.SectionId, model.BackgroundImageId);

                    _pageSectionService.SetBackgroundStyle(model.SectionId, model.PageSectionBackgroundStyle);
                }

                _pageSectionService.Height(model.SectionId, model.PageSectionHeight);
                _pageSectionService.Roles(model.SectionId, model.SelectedRoleList);

                return Json(new { State = true, SectionMarkup = _pageSectionService.Get(model.SectionId).PageSectionBody });
            }
            catch (Exception)
            {
                return Json(new { State = false });
            }
        }

        [HttpGet]
        public ActionResult Markup(int pageSectionId)
        {
            var pageSection = _pageSectionService.Get(pageSectionId);

            var model = new MarkupViewModel
            {
                PageSectionId = pageSectionId,
                PageSectionBody = pageSection.PageSectionBody
            };

            return View("_Markup", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public JsonResult Markup(MarkupViewModel model)
        {
            _pageSectionService.Markup(model.PageSectionId, model.PageSectionBody);

            return Json(new { State = true, Markup = model.PageSectionBody });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int pageSectionId)
        {
            try
            {
                _pageSectionService.Delete(pageSectionId);

                return Json(new { State = true });
            }
            catch (Exception ex)
            {
                return Json(new { State = false, ex.InnerException.Message });
            }
        }

        #region Section Backup Methods

        [HttpGet]
        public ActionResult Restore(int pageSectionId)
        {
            var pageSection = _pageSectionService.Get(pageSectionId);

            var model = new RestoreViewModel
            {
                PageSectionId = pageSectionId,
                PageSectionBackup = pageSection.PageSectionBackups.ToList()
            };

            return View("_Restore", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBackup(int pageSectionId)
        {
            _pageSectionService.Backup(pageSectionId);

            return Content("Refresh");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestoreBackup(int pageSectionId, int restorePointId)
        {
            var pageMarkup = _pageSectionService.RestoreBackup(pageSectionId, restorePointId);

            return Json(new { State = true, Markup = pageMarkup });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBackup(int restorePointId)
        {
            _pageSectionService.DeleteBackup(restorePointId);

            return Content("Refresh");
        }

        #endregion Section Backup Methods
    }
}