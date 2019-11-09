namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyItems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Downloads", "GalleryItem_ID", c => c.Int());
            AddColumn("dbo.ShadowGalleryItems", "GalleryName", c => c.String());
            CreateIndex("dbo.Downloads", "GalleryItem_ID");
            AddForeignKey("dbo.Downloads", "GalleryItem_ID", "dbo.ShadowGalleryItems", "ID");
            DropColumn("dbo.Downloads", "GalleryPictureID");
            DropColumn("dbo.Downloads", "GalleryName");
            DropColumn("dbo.GalleryItemMarks", "GalleryName");
            DropColumn("dbo.GalleryItemMarks", "MarkGalleryID");
            DropColumn("dbo.VisitRecords", "GalleryName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.VisitRecords", "GalleryName", c => c.String());
            AddColumn("dbo.GalleryItemMarks", "MarkGalleryID", c => c.String());
            AddColumn("dbo.GalleryItemMarks", "GalleryName", c => c.String());
            AddColumn("dbo.Downloads", "GalleryName", c => c.String());
            AddColumn("dbo.Downloads", "GalleryPictureID", c => c.String());
            DropForeignKey("dbo.Downloads", "GalleryItem_ID", "dbo.ShadowGalleryItems");
            DropIndex("dbo.Downloads", new[] { "GalleryItem_ID" });
            DropColumn("dbo.ShadowGalleryItems", "GalleryName");
            DropColumn("dbo.Downloads", "GalleryItem_ID");
        }
    }
}
