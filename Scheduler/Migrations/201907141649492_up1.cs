namespace CScheduler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class up1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CreditsNets",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    Name = c.String(unicode: false),
                    EndPoint = c.String(unicode: false),
                })
                .PrimaryKey(t => t.ID);

            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    Name = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                })
                .PrimaryKey(t => t.Id);
            //.Index(t => t.Name, unique: true, name: "RoleNameIndex");
            Sql("CREATE index `IX_Name` on `AspNetRoles` (`Name`)");

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    RoleId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true);
            //.Index(t => t.UserId)
            //.Index(t => t.RoleId);
            Sql("CREATE index `IX_UserId` on `AspNetUserRoles` (`UserId`)");
            Sql("CREATE index `IX_RoleId` on `AspNetUserRoles` (`RoleId`)");

            CreateTable(
                "dbo.SmartJobs",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, unicode: false),
                    Method = c.String(nullable: false, unicode: false),
                    Address = c.String(nullable: false, unicode: false),
                    CronExpression = c.String(unicode: false),
                    CreatedAt = c.DateTime(nullable: false, precision: 0),
                    IsActive = c.Boolean(nullable: false),
                    ExecutionMode = c.Int(nullable: false),
                    Errors = c.Int(nullable: false),
                    Executes = c.Int(nullable: false),
                    CreatedBy_Id = c.String(maxLength: 128, storeType: "nvarchar"),
                    CreditsNet_ID = c.Int(),
                    Rule_ID = c.Int(),
                })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedBy_Id)
                .ForeignKey("dbo.CreditsNets", t => t.CreditsNet_ID)
                .ForeignKey("dbo.Rules", t => t.Rule_ID);
            //.Index(t => t.CreatedBy_Id)
            //.Index(t => t.CreditsNet_ID)
            //.Index(t => t.Rule_ID);
            Sql("CREATE index `IX_CreatedBy_Id` on `SmartJobs` (`CreatedBy_Id`)");
            Sql("CREATE index `IX_CreditsNet_ID` on `SmartJobs` (`CreditsNet_ID`)");
            Sql("CREATE index `IX_Rule_ID` on `SmartJobs` (`Rule_ID`)");

            CreateTable(
                "dbo.AspNetUsers",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    FirstName = c.String(nullable: false, unicode: false),
                    LastName = c.String(nullable: false, unicode: false),
                    IsActivated = c.Boolean(nullable: false),
                    FullName = c.String(unicode: false),
                    PublicKey = c.String(unicode: false),
                    PrivateKey = c.String(unicode: false),
                    Email = c.String(maxLength: 256, storeType: "nvarchar"),
                    EmailConfirmed = c.Boolean(nullable: false),
                    PasswordHash = c.String(unicode: false),
                    SecurityStamp = c.String(unicode: false),
                    PhoneNumber = c.String(unicode: false),
                    PhoneNumberConfirmed = c.Boolean(nullable: false),
                    TwoFactorEnabled = c.Boolean(nullable: false),
                    LockoutEndDateUtc = c.DateTime(precision: 0),
                    LockoutEnabled = c.Boolean(nullable: false),
                    AccessFailedCount = c.Int(nullable: false),
                    UserName = c.String(nullable: false, maxLength: 256, storeType: "nvarchar"),
                })
                .PrimaryKey(t => t.Id);
            //.Index(t => t.UserName, unique: true, name: "UserNameIndex");
            Sql("CREATE index `IX_UserName` on `AspNetUsers` (`UserName`)");

            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    ClaimType = c.String(unicode: false),
                    ClaimValue = c.String(unicode: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true);
            //.Index(t => t.UserId);
            Sql("CREATE index `IX_UserId` on `AspNetUserClaims` (`UserId`)");

            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                {
                    LoginProvider = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    ProviderKey = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                    UserId = c.String(nullable: false, maxLength: 128, storeType: "nvarchar"),
                })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true);
            //.Index(t => t.UserId);
            Sql("CREATE index `IX_UserId` on `AspNetUserLogins` (`UserId`)");

            CreateTable(
                "dbo.JobEvents",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    IsSuccessed = c.Boolean(nullable: false),
                    Text = c.String(unicode: false),
                    RequestDate = c.DateTime(nullable: false, precision: 0),
                    ResponseDate = c.DateTime(nullable: false, precision: 0),
                    SmartJob_ID = c.Int(),
                })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.SmartJobs", t => t.SmartJob_ID);
            //.Index(t => t.SmartJob_ID);
            Sql("CREATE index `IX_SmartJob_ID` on `JobEvents` (`SmartJob_ID`)");

            CreateTable(
                "dbo.Rules",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    RegularDateFrom = c.String(unicode: false),
                    RegularDateTo = c.String(unicode: false),
                    RegularValue = c.Int(nullable: false),
                    RegularPeriod = c.Int(nullable: false),
                    OnceDate = c.String(unicode: false),
                    CronExpression = c.String(unicode: false),
                    Presentation = c.String(unicode: false),
                })
                .PrimaryKey(t => t.ID);
        }

        public override void Down()
        {
            DropForeignKey("dbo.SmartJobs", "Rule_ID", "dbo.Rules");
            DropForeignKey("dbo.JobEvents", "SmartJob_ID", "dbo.SmartJobs");
            DropForeignKey("dbo.SmartJobs", "CreditsNet_ID", "dbo.CreditsNets");
            DropForeignKey("dbo.SmartJobs", "CreatedBy_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropIndex("dbo.JobEvents", new[] { "SmartJob_ID" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.SmartJobs", new[] { "Rule_ID" });
            DropIndex("dbo.SmartJobs", new[] { "CreditsNet_ID" });
            DropIndex("dbo.SmartJobs", new[] { "CreatedBy_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropTable("dbo.Rules");
            DropTable("dbo.JobEvents");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.SmartJobs");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.CreditsNets");
        }
    }
}
