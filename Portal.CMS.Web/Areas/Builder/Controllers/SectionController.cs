﻿using Portal.CMS.Services.Authentication;
using Portal.CMS.Services.Generic;
using Portal.CMS.Services.PageBuilder;
using Portal.CMS.Web.Architecture.ActionFilters;
using Portal.CMS.Web.Areas.Builder.ViewModels.Section;
using Portal.CMS.Web.ViewModels.Shared;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Portal.CMS.Web.Areas.Builder.Controllers
{
    [AdminFilter]
    public class SectionController : Controller
    {
        #region Dependencies

        private readonly IPageSectionService _pageSectionService;
        private readonly IPageSectionTypeService _pageSectionTypeService;
        private readonly IPagePartialService _partialService;
        private readonly IPageAssociationService _associationService;
        private readonly IImageService _imageService;
        private readonly IRoleService _roleService;

        public SectionController(IPageSectionService pageSectionService, IPageSectionTypeService pageSectionTypeService, IImageService imageService, IRoleService roleService, IPagePartialService partialService, IPageAssociationService associationService)
        {
            _pageSectionService = pageSectionService;
            _pageSectionTypeService = pageSectionTypeService;
            _imageService = imageService;
            _roleService = roleService;
            _partialService = partialService;
            _associationService = associationService;
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
        public JsonResult AddSection(int pageId, int pageSectionTypeId, string componentStamp)
        {
            try
            {
                var pageAssociation = _pageSectionService.Add(pageId, pageSectionTypeId, componentStamp);

                return Json(new { State = true, PageSectionId = pageAssociation.PageSection.PageSectionId, PageAssociationId = pageAssociation.PageAssociationId });
            }
            catch (Exception)
            {
                return Json(new { State = false });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddPartial(int pageId, string areaName, string controllerName, string actionName)
        {
            try
            {
                Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

                Type type = types.Where(t => t.Name == $"{controllerName}Controller").SingleOrDefault();

                if (type != null && type.GetMethod(actionName) != null)
                {
                    _partialService.Add(pageId, areaName, controllerName, actionName);

                    return Json(new { State = true });
                }

                return Json(new { State = false, Reason = "Invalid" });
            }
            catch (Exception)
            {
                return Json(new { State = false, Reason = "Exception" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int pageAssociationId)
        {
            try
            {
                _pageSectionService.Delete(pageAssociationId);

                return Json(new { State = true });
            }
            catch (Exception ex)
            {
                return Json(new { State = false, ex.InnerException.Message });
            }
        }

        #region Section Edit Methods

        [HttpGet]
        public ActionResult EditSection(int pageAssociationId)
        {
            var pageAssociation = _associationService.Get(pageAssociationId);

            var pageSection = _pageSectionService.Get(pageAssociation.PageSection.PageSectionId);

            var model = new EditSectionViewModel
            {
                PageAssociationId = pageAssociationId,
                SectionId = pageSection.PageSectionId,
                MediaLibrary = new PaginationViewModel
                {
                    ImageList = _imageService.Get(),
                    TargetInputField = "BackgroundImageId",
                    PaginationType = "section"
                },
                PageSectionHeight = _pageSectionService.DetermineSectionHeight(pageSection.PageSectionId),
                PageSectionBackgroundStyle = _pageSectionService.DetermineBackgroundStyle(pageSection.PageSectionId),
                BackgroundType = _pageSectionService.DetermineBackgroundType(pageSection.PageSectionId),
                BackgroundColour = _pageSectionService.DetermineBackgroundColour(pageSection.PageSectionId),
                RoleList = _roleService.Get(),
                SelectedRoleList = pageSection.PageSectionRoles.Select(x => x.Role.RoleName).ToList()
            };

            return View("_EditSection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditSection(EditSectionViewModel model)
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
        public ActionResult EditPartial(int pageAssociationId)
        {
            var pageAssociation = _associationService.Get(pageAssociationId);

            var model = new EditPartialViewModel
            {
                PageAssociationId = pageAssociationId,
                PagePartialId = pageAssociation.PagePartial.PagePartialId
            };

            return View("_EditPartial", model);
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

        #endregion Section Edit Methods

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