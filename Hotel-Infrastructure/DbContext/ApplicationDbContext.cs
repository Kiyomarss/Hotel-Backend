using ContactsManager.Core.Domain.IdentityEntities;
using Entities;
using Hotel_Core.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RepositoryContracts;

namespace Hotel_Infrastructure.DbContext
{
 public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
 {
  public ApplicationDbContext(DbContextOptions options) : base(options) { }
  
  public DatabaseFacade Database => base.Database;
  
  public new EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => base.Entry(entity);
  
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
     .WithMany()
     .HasForeignKey(b => b.CabinId) ;
   });
   
   modelBuilder.Entity<Booking>(entity =>
   {
    entity.HasOne(b => b.Guest)
     .WithMany()
     .HasForeignKey(b => b.GuestId);
   });
  }
 }
}
