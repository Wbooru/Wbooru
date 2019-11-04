namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyDownloadProp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Downloads", "TotalBytes", c => c.Long(nullable: false));
            AlterColumn("dbo.Downloads", "GalleryPictureID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Downloads", "GalleryPictureID", c => c.Int(nullable: false));
            DropColumn("dbo.Downloads", "TotalBytes");
        }
    }
}
