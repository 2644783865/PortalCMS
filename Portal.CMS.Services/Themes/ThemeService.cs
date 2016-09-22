﻿using Portal.CMS.Entities;
using Portal.CMS.Entities.Entities.Themes;
using Portal.CMS.Services.PageBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Portal.CMS.Services.Themes
{
    public interface IThemeService
    {
        Theme Get(int themeId);

        IEnumerable<Theme> Get();

        int Upsert(int themeId, string themeName, int titleFontId, int textFontId, int largeTitleFontSize, int mediumTitleFontSize, int smallTitleFontSize, int tinyTitleFontSize, int textStandardFontSize);

        void Delete(int themeId);

        void Default(int themeId);

        Theme GetDefault();
    }

    public class ThemeService : IThemeService
    {
        #region Dependencies

        private readonly PortalEntityModel _context;
        private readonly IPageService _pageService;

        public ThemeService(PortalEntityModel context, IPageService pageService)
        {
            _context = context;
            _pageService = pageService;
        }

        #endregion Dependencies

        public Theme GetDefault()
        {
            var defaultTheme = _context.Themes.FirstOrDefault(x => x.IsDefault == true);

            return defaultTheme;
        }

        public Theme Get(int themeId)
        {
            var theme = _context.Themes.FirstOrDefault(x => x.ThemeId == themeId);

            return theme;
        }

        public IEnumerable<Theme> Get()
        {
            var results = _context.Themes.ToList();

            return results.OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.DateUpdated);
        }

        public int Upsert(int themeId, string themeName, int titleFontId, int textFontId, int largeTitleFontSize, int mediumTitleFontSize, int smallTitleFontSize, int tinyTitleFontSize, int textStandardFontSize)
        {
            var existingTheme = Get(themeId);

            if (existingTheme == null)
            {
                var newTheme = new Theme
                {
                    ThemeName = themeName,
                    TitleFontId = titleFontId,
                    TextFontId = textFontId,
                    DateAdded = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    TitleLargeFontSize = largeTitleFontSize,
                    TitleMediumFontSize = mediumTitleFontSize,
                    TitleSmallFontSize = smallTitleFontSize,
                    TitleTinyFontSize = tinyTitleFontSize,
                    TextStandardFontSize = textStandardFontSize,
                    IsDefault = false,
                };

                _context.Themes.Add(newTheme);

                _context.SaveChanges();

                return newTheme.ThemeId;
            }
            else
            {
                existingTheme.ThemeName = themeName;
                existingTheme.TitleFontId = titleFontId;
                existingTheme.TextFontId = textFontId;
                existingTheme.DateUpdated = DateTime.Now;
                existingTheme.TitleLargeFontSize = largeTitleFontSize;
                existingTheme.TitleMediumFontSize = mediumTitleFontSize;
                existingTheme.TitleSmallFontSize = smallTitleFontSize;
                existingTheme.TitleTinyFontSize = tinyTitleFontSize;
                existingTheme.TextStandardFontSize = textStandardFontSize;

                _context.SaveChanges();

                return existingTheme.ThemeId;
            }
        }

        public void Delete(int themeId)
        {
            var existingTheme = _context.Themes.FirstOrDefault(x => x.ThemeId == themeId);

            if (existingTheme == null)
                return;

            _context.Themes.Remove(existingTheme);

            _context.SaveChanges();
        }

        public void Default(int themeId)
        {
            var themes = Get();

            foreach (var theme in themes)
            {
                if (theme.ThemeId == themeId)
                    theme.IsDefault = true;
                else
                    theme.IsDefault = false;
            }

            _context.SaveChanges();
        }
    }
}