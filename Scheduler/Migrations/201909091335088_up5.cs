namespace CScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SmartJobs", "MaxFee", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SmartJobs", "MaxFee", c => c.String(nullable: false, unicode: false));
        }
    }
}
