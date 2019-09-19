namespace CScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmartJobs", "DeleteTaskAfterExecution", c => c.Boolean(nullable: false));
            AddColumn("dbo.SmartJobs", "IsDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmartJobs", "IsDeleted");
            DropColumn("dbo.SmartJobs", "DeleteTaskAfterExecution");
        }
    }
}
