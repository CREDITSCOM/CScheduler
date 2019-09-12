namespace CScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ApiKey", c => c.String(unicode: false));
            DropColumn("dbo.SmartJobs", "MaxFee");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SmartJobs", "MaxFee", c => c.String(unicode: false));
            DropColumn("dbo.AspNetUsers", "ApiKey");
        }
    }
}
