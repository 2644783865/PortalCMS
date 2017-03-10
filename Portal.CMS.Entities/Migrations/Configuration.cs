namespace Portal.CMS.Entities.Migrations
{
    using System.Data.Entity.Migrations;
    using Seed;

    internal sealed class Configuration : DbMigrationsConfiguration<Portal.CMS.Entities.PortalEntityModel>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Portal.CMS.Entities.PortalEntityModel context)
        {
            ThemeSeed.Seed(context);
            RoleSeed.Seed(context);
            SettingSeed.Seed(context);
            MenuSeed.Seed(context);
            PostCategorySeed.Seed(context);
            PageSectionTypeSeed.Seed(context);
            PageComponentTypeSeed.Seed(context);
            MediaSeed.Seed(context);
            CopySeed.Seed(context);

            context.SaveChanges();

            PageSeed.Seed(context);
            PostSeed.Seed(context);
        }
    }
}