namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDownloadProp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Downloads", "DisplayDownloadedLength", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Downloads", "DisplayDownloadedLength");
        }
    }
}
