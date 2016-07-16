﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Portal.CMS.Entities.Entities.Analytics
{
    public class AnalyticPostView
    {
        [Key]
        public int AnalyticPageViewId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public string ReferredUrl { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        public int UserId { get; set; }
    }
}