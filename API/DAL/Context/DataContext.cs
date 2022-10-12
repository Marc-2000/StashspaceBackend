using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.DAL.Context
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasKey(pr => new { pr.UserID, pr.RoleID });
            modelBuilder.Entity<UserRole>()
                .HasOne(pr => pr.User)
                .WithMany(p => p.UserRoles)
                .HasForeignKey(pr => pr.UserID);
            modelBuilder.Entity<UserRole>()
                .HasOne(pr => pr.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(pr => pr.RoleID);
        }
    }
}
