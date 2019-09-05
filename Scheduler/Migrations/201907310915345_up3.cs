namespace CScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SmartJobs", "MaxFee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SmartJobs", "MaxFee");
        }
    }
}
