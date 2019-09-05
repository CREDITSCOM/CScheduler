namespace CScheduler.Migrations
{
    using CScheduler.Classes.Database;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<DatabaseContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations";
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
        }

        protected override void Seed(DatabaseContext Context)
        {
            //Context.CreditsNets.AddOrUpdate(x => x.ID, 
            //    new CreditsNet { ID = 1, Name = "Credits network", EndPoint = "http://wallet.credits.com/Main/api/UnsafeTransaction" }, 
            //    new CreditsNet { ID = 2, Name = "Test net", EndPoint = "http://wallet.credits.com/testnet-r4_2/api/UnsafeTransaction" },
            //    new CreditsNet { ID = 3, Name = "Devs & dapps", EndPoint = "http://wallet.credits.com/DevsDappsTestnet/api/UnsafeTransaction" });                        
            //Context.SaveChanges();
        }
    }
}
