namespace CScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SmartJobs", "CronExpression");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SmartJobs", "CronExpression", c => c.String(unicode: false));
        }
    }
}
