namespace Wbooru.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitMigration : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.VisitRecords", new[] { "VisitRecordID" });
            DropPrimaryKey("dbo.VisitRecords");
            AlterColumn("dbo.VisitRecords", "VisitRecordID", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.VisitRecords", "VisitRecordID");
            CreateIndex("dbo.VisitRecords", "VisitRecordID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.VisitRecords", new[] { "VisitRecordID" });
            DropPrimaryKey("dbo.VisitRecords");
            AlterColumn("dbo.VisitRecords", "VisitRecordID", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.VisitRecords", "VisitRecordID");
            CreateIndex("dbo.VisitRecords", "VisitRecordID");
        }
    }
}
