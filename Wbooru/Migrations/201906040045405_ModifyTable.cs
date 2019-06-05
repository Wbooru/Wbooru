namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyTable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TagRecords", "Tag_Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TagRecords", "Tag_Type", c => c.String());
        }
    }
}
