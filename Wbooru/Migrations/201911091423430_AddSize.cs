namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSize : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShadowGalleryItems", "PreviewImageWidth", c => c.Int(nullable: false));
            AddColumn("dbo.ShadowGalleryItems", "PreviewImageHeight", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShadowGalleryItems", "PreviewImageHeight");
            DropColumn("dbo.ShadowGalleryItems", "PreviewImageWidth");
        }
    }
}
