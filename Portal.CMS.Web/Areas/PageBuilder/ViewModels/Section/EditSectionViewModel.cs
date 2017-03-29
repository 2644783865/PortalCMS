﻿using Portal.CMS.Entities.Entities;
using Portal.CMS.Entities.Enumerators;
using Portal.CMS.Web.ViewModels.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;

namespace Portal.CMS.Web.Areas.PageBuilder.ViewModels.Section
{
    public class EditSectionViewModel
    {
        public int PageAssociationId { get; set; }

        public int SectionId { get; set; }

        public string BackgroundType { get; set; }

        public int BackgroundImageId { get; set; }

        public PaginationViewModel MediaLibrary { get; set; }

        [DisplayName("Background Colour")]
        public string BackgroundColour { get; set; }

        [DisplayName("Height")]
        public PageSectionHeight PageSectionHeight { get; set; }

        [DisplayName("Background Style")]
        public PageSectionBackgroundStyle PageSectionBackgroundStyle { get; set; }

        [DisplayName("Roles")]
        public List<string> SelectedRoleList { get; set; } = new List<string>();

        [DisplayName("Attach Image")]
        public HttpPostedFileBase AttachedImage { get; set; }

        [DisplayName("Category")]
        public ImageCategory ImageCategory { get; set; }

        #region Enumerable Properties

        public List<Role> RoleList { get; set; }

        #endregion Enumerable Properties
    }
}