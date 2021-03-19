using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using RoboSphere.Discord.Data.Models;

namespace RoboSphere.Discord.Data
{
    [PublicAPI]
    public class SqlContext : DbContext
    {
        public SqlContext(Settings settings) => RoboSettings = settings;

        private Settings RoboSettings { get; }
        public DbSet<Setting> Settings { get; set; } = null!;
        public DbSet<Stopwatch> Stopwatch { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.UseSqlite(RoboSettings.Sqlite);
        }
    }
}
