using Bota.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Bota.Context
{
    public class ApplicationContext : DbContext
    {
        public DbSet<BotConfig> BotConfigs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageType> MessageTypes { get; set; }


        public ApplicationContext(DbContextOptions<ApplicationContext> options ) : base(options)
        {            
        }
        
        //var serverVersion = ServerVersion.AutoDetect(connectionString);
        //options => options.UseMySql(connectionString, serverVersion)

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BotConfig>().HasData(
                new BotConfig
                {
                    Id = 889924246820749312,
                    Prefix = "/",
                    SteamApiKey = "00AF2C79D6E3395D77541A5E0C5377E4"
                }
            );
            base.OnModelCreating(modelBuilder);
        }

        internal Task FirstOrDefaultAsync()
        {
            throw new NotImplementedException();
        }
    }
}
