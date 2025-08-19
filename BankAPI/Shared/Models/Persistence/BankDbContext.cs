using Microsoft.EntityFrameworkCore;
using BankAPI.Shared.Models;

namespace BankAPI.Shared;

public class BankDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId);

        modelBuilder.Entity<Account>()
        .Property(a => a.IsLocked)
        .HasDefaultValue(false); // Добавляем конфигурацию для IsLocked

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("numeric(18,2)");

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Currency)
            .HasConversion<int>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Type)
            .HasConversion<int>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Description)
            .IsRequired();

        modelBuilder.Entity<Transaction>()
            .HasOne<Account>()
            .WithMany()
            .HasForeignKey(t => t.CounterpartyAccountId)
            .IsRequired(false);

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.Property(m => m.Id).ValueGeneratedNever();
            entity.Property(m => m.EventType).IsRequired().HasMaxLength(100);
            entity.Property(m => m.EventData).IsRequired().HasColumnType("jsonb");
            entity.Property(m => m.CreatedAt).IsRequired();
            entity.Property(m => m.Status).IsRequired();
        });

    }
}