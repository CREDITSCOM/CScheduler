using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CScheduler.Classes.Database
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext() : base("MySqlContext", throwIfV1Schema: false)
        {

        }

        public static DatabaseContext Create()
        {
            return new DatabaseContext();
        }

        public DbSet<SmartJob> SmartJobs { get; set; }
        public DbSet<CreditsNet> CreditsNets { get; set; }
        public DbSet<JobEvent> JobEvents { get; set; }
    }
}