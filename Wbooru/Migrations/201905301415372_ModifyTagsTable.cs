namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyTagsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TagRecords",
                c => new
                    {
                        TagID = c.Int(nullable: false, identity: true),
                        Tag_Name = c.String(),
                        Tag_Type = c.String(),
                        AddTime = c.DateTime(nullable: false),
                        FromGallery = c.String(),
                    })
                .PrimaryKey(t => t.TagID);
            
            DropTable("dbo.Tags");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        TagID = c.Int(nullable: false, identity: true),
                        TagName = c.String(),
                        AddTime = c.DateTime(nullable: false),
                        FromGallery = c.String(),
                    })
                .PrimaryKey(t => t.TagID);
            
            DropTable("dbo.TagRecords");
        }
    }
}
