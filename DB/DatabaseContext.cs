using FullStackRebelsTestCSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace FullStackRebelsTestCSharp.DB
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options)
      : base(options)
        { }

        public DbSet<Show> Shows { get; set; }
        public DbSet<Cast> Cast { get; set; }
    }
}
