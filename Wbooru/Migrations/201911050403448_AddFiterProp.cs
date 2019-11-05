namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFiterProp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TagRecords", "IsFilter", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TagRecords", "IsFilter");
        }
    }
}
