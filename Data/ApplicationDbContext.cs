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

    // Mohamed
    public DbSet<Member> Members { get; set; }
    public DbSet<WeightLog> WeightLogs { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }

    // Marwan
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<GymClass> GymClasses { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    // Ziad
    public DbSet<Trainer> Trainers { get; set; }
    public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }
    public DbSet<DietMeal> DietMeals { get; set; }
    public DbSet<TrainerReview> TrainerReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Member - ApplicationUser
        builder.Entity<Member>()
            .HasOne(member => member.ApplicationUser)
            .WithOne(user => user.Member)
            .HasForeignKey<Member>(member => member.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member - WeightLog
        builder.Entity<Member>()
            .HasMany(member => member.WeightLogs)
            .WithOne(weightLog => weightLog.Member)
            .HasForeignKey(weightLog => weightLog.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member - CheckIn
        builder.Entity<Member>()
            .HasMany(member => member.CheckIns)
            .WithOne(checkIn => checkIn.Member)
            .HasForeignKey(checkIn => checkIn.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member - Subscription
        builder.Entity<Subscription>()
            .HasOne(subscription => subscription.Member)
            .WithMany()
            .HasForeignKey(subscription => subscription.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plan - Subscription
        builder.Entity<Subscription>()
            .HasOne(subscription => subscription.Plan)
            .WithMany()
            .HasForeignKey(subscription => subscription.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Subscription - Payment
        builder.Entity<Payment>()
            .HasOne(payment => payment.Subscription)
            .WithMany()
            .HasForeignKey(payment => payment.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Trainer - GymClass
        builder.Entity<GymClass>()
            .HasOne(gymClass => gymClass.Trainer)
            .WithMany()
            .HasForeignKey(gymClass => gymClass.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Member - Booking
        builder.Entity<Booking>()
            .HasOne(booking => booking.Member)
            .WithMany()
            .HasForeignKey(booking => booking.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // GymClass - Booking
        builder.Entity<Booking>()
            .HasOne(booking => booking.GymClass)
            .WithMany()
            .HasForeignKey(booking => booking.GymClassId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member - WorkoutPlan
        builder.Entity<WorkoutPlan>()
            .HasOne(workoutPlan => workoutPlan.Member)
            .WithMany()
            .HasForeignKey(workoutPlan => workoutPlan.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Trainer - WorkoutPlan
        builder.Entity<WorkoutPlan>()
            .HasOne(workoutPlan => workoutPlan.Trainer)
            .WithMany()
            .HasForeignKey(workoutPlan => workoutPlan.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkoutPlan - WorkoutExercise
        builder.Entity<WorkoutExercise>()
            .HasOne(exercise => exercise.WorkoutPlan)
            .WithMany(workoutPlan => workoutPlan.Exercises)
            .HasForeignKey(exercise => exercise.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkoutPlan - DietMeal
        builder.Entity<DietMeal>()
            .HasOne(meal => meal.WorkoutPlan)
            .WithMany(workoutPlan => workoutPlan.DietMeals)
            .HasForeignKey(meal => meal.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Trainer - TrainerReview
        builder.Entity<TrainerReview>()
            .HasOne(review => review.Trainer)
            .WithMany()
            .HasForeignKey(review => review.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Member - TrainerReview
        builder.Entity<TrainerReview>()
            .HasOne(review => review.Member)
            .WithMany()
            .HasForeignKey(review => review.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevent duplicate reviews by the same member for the same trainer.
        builder.Entity<TrainerReview>()
            .HasIndex(review => new
            {
                review.TrainerId,
                review.MemberId
            })
            .IsUnique();

        // Decimal precision
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