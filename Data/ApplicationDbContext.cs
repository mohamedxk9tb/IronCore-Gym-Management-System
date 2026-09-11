using GymMvc.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GymMvc.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Member> Members { get; set; }

    public DbSet<WeightLog> WeightLogs { get; set; }

    public DbSet<CheckIn> CheckIns { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // =========================
        // Member ↔ ApplicationUser
        // =========================

        builder.Entity<Member>()
            .HasOne(member => member.ApplicationUser)
            .WithOne(user => user.Member)
            .HasForeignKey<Member>(
                member => member.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================
        // Member → WeightLogs
        // =========================

        builder.Entity<Member>()
            .HasMany(member => member.WeightLogs)
            .WithOne(weightLog => weightLog.Member)
            .HasForeignKey(weightLog => weightLog.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================
        // Member → CheckIns
        // =========================

        builder.Entity<Member>()
            .HasMany(member => member.CheckIns)
            .WithOne(checkIn => checkIn.Member)
            .HasForeignKey(checkIn => checkIn.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========================
        // Decimal Precision
        // =========================

        builder.Entity<Member>()
            .Property(member => member.Height)
            .HasPrecision(5, 2);

        builder.Entity<Member>()
            .Property(member => member.Weight)
            .HasPrecision(5, 2);

        builder.Entity<WeightLog>()
            .Property(weightLog => weightLog.Weight)
            .HasPrecision(5, 2);
    }
}