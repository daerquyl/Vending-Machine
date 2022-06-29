using Microsoft.EntityFrameworkCore;
using Vending.Machine.Domain.UserAccountManagement;
using Vending.Machine.Domain.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vending.Machine.Data
{
    public class VendingMachineContext: DbContext
    {
        public DbSet<VendingMachine> VendingMachines { get; set; }
        public DbSet<User> Users { get; set; }

        public VendingMachineContext(DbContextOptions<VendingMachineContext> options):base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VendingMachine");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(p => p.Deposit)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<VendingMachineAccount>()
                .Property(a => a.Deposit)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Cost)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<VendingMachine>()
                .OwnsOne(v => v.Money)
                .Property(m => m.Value)
                .HasField("_value");

            var converter = new EnumToStringConverter<UserRole>();
            modelBuilder
                .Entity<User>()
                .Property(e => e.Role)
                .HasConversion(converter);
        }
    }
}