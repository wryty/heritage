using HeritageApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography.Xml;

namespace HeritageApi.Data;
public class DataContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Database.Migrate();
    }
    public DbSet<Exhibit> Exhibits { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {   
        base.OnModelCreating(modelBuilder);

        /*modelBuilder.Entity<IdentityRole<long>>().HasData(
            new IdentityRole<long> { Id = 1, Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
            new IdentityRole<long> { Id = 2, Name = "Member", NormalizedName = "MEMBER" }
        );*/

    }
}