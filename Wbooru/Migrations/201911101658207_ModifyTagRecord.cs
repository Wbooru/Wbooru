namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyTagRecord : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TagRecords", "RecordType", c => c.Int(nullable: false));
            DropColumn("dbo.TagRecords", "IsFilter");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TagRecords", "IsFilter", c => c.Boolean(nullable: false));
            DropColumn("dbo.TagRecords", "RecordType");
        }
    }
}
