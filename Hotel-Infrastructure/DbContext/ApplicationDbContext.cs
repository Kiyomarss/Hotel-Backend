using ContactsManager.Core.Domain.IdentityEntities;
using Entities;
using Hotel_Core.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RepositoryContracts;

namespace Hotel_Infrastructure.DbContext
{
 public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
 {
  public ApplicationDbContext(DbContextOptions options) : base(options) { }
  
  public DatabaseFacade Database => base.Database;
  
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
   base.OnModelCreating(modelBuilder);

   modelBuilder.Entity<Booking>().ToTable("Bookings");
   modelBuilder.Entity<Cabin>().ToTable("Cabins");
   modelBuilder.Entity<Guest>().ToTable("Guests");
   modelBuilder.Entity<Setting>().ToTable("Setting");
   
   modelBuilder.Entity<Booking>(entity =>
   {
    entity.HasOne(b => b.Cabin)
     .WithMany(c => c.Bookings)
     .HasForeignKey(b => b.CabinId) ;
   });
   
   modelBuilder.Entity<Booking>(entity =>
   {
    entity.HasOne(b => b.Guest)
     .WithMany(c => c.Bookings)
     .HasForeignKey(b => b.GuestId);
   });
  }
 }
}
