using GymMvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymMvc.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context =
            serviceProvider.GetRequiredService<ApplicationDbContext>();

        var userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // =========================================================
        // Prevent duplicate seed data
        // =========================================================

        if (await context.Plans.AnyAsync())
        {
            return;
        }

        // =========================================================
        // Ensure roles exist
        // =========================================================

        string[] roles =
        [
            "Admin",
            "Trainer",
            "Member"
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result =
                    await roleManager.CreateAsync(
                        new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{role}'.");
                }
            }
        }

        // =========================================================
        // 1. TRAINER USERS
        // =========================================================

        var trainerNames = new[]
        {
            "Ahmed Hassan",
            "Omar Khaled",
            "Youssef Adel",
            "Mahmoud Samir",
            "Karim Mostafa",
            "Amr Tarek",
            "Hassan Ali",
            "Mohamed Nabil",
            "Ibrahim Fathy",
            "Seif El Din Ahmed",
            "Adam Sherif",
            "Mina George",
            "Mostafa Hany",
            "Abdelrahman Essam",
            "Khaled Ashraf",
            "Tamer Mohamed",
            "Ziad Hossam",
            "Marwan Adel",
            "Islam Mahmoud",
            "Yassin Hamdy"
        };

        var trainerSpecialties = new[]
        {
            "Strength Training",
            "Bodybuilding",
            "CrossFit",
            "Functional Training",
            "Weight Loss",
            "Powerlifting",
            "HIIT",
            "Fitness Conditioning",
            "Mobility",
            "Athletic Performance"
        };

        var trainerCertificates = new[]
        {
            "NASM Certified Personal Trainer",
            "ACE Fitness Certification",
            "ISSA Personal Trainer",
            "CrossFit Level 2",
            "Precision Nutrition Level 1",
            "NSCA Certified Strength Coach",
            "ISSA Strength & Conditioning",
            "NASM Performance Enhancement",
            "ACE Health Coach",
            "IFBB Fitness Certification"
        };

        var trainers = new List<Trainer>();

        for (int i = 0; i < trainerNames.Length; i++)
        {
            var email =
                $"trainer{i + 1}@gmail.com";

            var user =
                await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = trainerNames[i],
                    EmailConfirmed = true
                };

                var createResult =
                    await userManager.CreateAsync(
                        user,
                        "Trainer@123");

                if (!createResult.Succeeded)
                {
                    var errors =
                        string.Join(
                            ", ",
                            createResult.Errors.Select(
                                e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create trainer user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, "Trainer"))
            {
                await userManager.AddToRoleAsync(
                    user,
                    "Trainer");
            }

            var trainer =
                await context.Trainers
                    .FirstOrDefaultAsync(
                        t => t.ApplicationUserId == user.Id);

            if (trainer is null)
            {
                trainer = new Trainer
                {
                    FullName = trainerNames[i],
                    Specialty =
                        trainerSpecialties[
                            i % trainerSpecialties.Length],
                    Certificates =
                        trainerCertificates[
                            i % trainerCertificates.Length],
                    ApplicationUserId = user.Id
                };

                context.Trainers.Add(trainer);

                trainers.Add(trainer);
            }
            else
            {
                trainers.Add(trainer);
            }
        }

        if (trainers.Any(t => t.Id == 0))
        {
            await context.SaveChangesAsync();
        }

        // =========================================================
        // 2. MEMBER USERS + MEMBERS
        // =========================================================

        var memberNames = new[]
        {
            "Ali Hassan",
            "Omar Samir",
            "Youssef Mohamed",
            "Mahmoud Adel",
            "Karim Hassan",
            "Amr Khaled",
            "Hassan Mahmoud",
            "Mohamed Ashraf",
            "Ibrahim Samy",
            "Seif Ahmed",
            "Adam Mostafa",
            "Mina Nabil",
            "Mostafa Tarek",
            "Abdelrahman Ali",
            "Khaled Samir",
            "Tamer Hassan",
            "Ziad Ahmed",
            "Marwan Khaled",
            "Islam Adel",
            "Yassin Mohamed",
            "Ahmed Nasser",
            "Fares Hany",
            "Sayed Mahmoud",
            "Mahmoud Fathy",
            "Kareem Ashraf"
        };

        var goals = new[]
        {
            "Lose weight",
            "Build muscle",
            "Stay fit",
            "Improve strength",
            "Improve endurance"
        };

        var fitnessLevels = new[]
        {
            "Beginner",
            "Intermediate",
            "Advanced"
        };

        var members = new List<Member>();

        for (int i = 0; i < memberNames.Length; i++)
        {
            var email =
                $"member{i + 1}@gmail.com";

            var user =
                await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = memberNames[i],
                    EmailConfirmed = true
                };

                var createResult =
                    await userManager.CreateAsync(
                        user,
                        "Member@123");

                if (!createResult.Succeeded)
                {
                    var errors =
                        string.Join(
                            ", ",
                            createResult.Errors.Select(
                                e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create member user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, "Member"))
            {
                await userManager.AddToRoleAsync(
                    user,
                    "Member");
            }

            var member =
                await context.Members
                    .FirstOrDefaultAsync(
                        m => m.ApplicationUserId == user.Id);

            if (member is null)
            {
                member = new Member
                {
                    FullName = memberNames[i],
                    Email = email,
                    Height = 165 + (i % 16),
                    Weight = 62 + (i * 2 % 35),
                    Goal = goals[i % goals.Length],
                    FitnessLevel =
                        fitnessLevels[
                            i % fitnessLevels.Length],
                    ApplicationUserId = user.Id
                };

                context.Members.Add(member);

                members.Add(member);
            }
            else
            {
                members.Add(member);
            }
        }

        if (members.Any(m => m.Id == 0))
        {
            await context.SaveChangesAsync();
        }

        // =========================================================
        // 3. MEMBERSHIP PLANS
        // =========================================================

        var planDefinitions = new[]
        {
            ("Iron Starter", 1, 650m),
            ("Iron Plus", 1, 850m),
            ("Strength Monthly", 1, 950m),
            ("Fitness Monthly", 1, 750m),
            ("Elite Monthly", 1, 1200m),

            ("Iron Quarter", 3, 1800m),
            ("Strength Quarter", 3, 2400m),
            ("Fitness Quarter", 3, 2100m),
            ("Elite Quarter", 3, 3000m),
            ("Performance Quarter", 3, 3300m),

            ("Iron Annual", 12, 6000m),
            ("Strength Annual", 12, 8500m),
            ("Fitness Annual", 12, 7200m),
            ("Elite Annual", 12, 11000m),
            ("Performance Annual", 12, 12500m),

            ("Student Fitness", 1, 500m),
            ("Student Strength", 3, 1350m),
            ("Weekend Warrior", 3, 1500m),
            ("Athlete Pro", 12, 14000m),
            ("Executive Fitness", 12, 10000m)
        };

        var plans = new List<Plan>();

        foreach (var definition in planDefinitions)
        {
            plans.Add(
                new Plan
                {
                    Name = definition.Item1,
                    DurationMonths = definition.Item2,
                    Price = definition.Item3
                });
        }

        context.Plans.AddRange(plans);
        await context.SaveChangesAsync();

        // =========================================================
        // 4. GYM CLASSES
        // =========================================================

        var classDefinitions = new[]
        {
            ("Morning Strength", "Monday", 6, 60),
            ("Powerlifting Basics", "Monday", 12, 75),
            ("HIIT Burn", "Monday", 15, 45),
            ("Functional Fitness", "Tuesday", 15, 60),
            ("Bodybuilding", "Tuesday", 12, 75),
            ("Mobility Flow", "Tuesday", 20, 45),
            ("CrossFit Fundamentals", "Wednesday", 14, 60),
            ("Upper Body Strength", "Wednesday", 12, 60),
            ("Core & Conditioning", "Wednesday", 18, 45),
            ("Weight Loss Circuit", "Thursday", 20, 50),
            ("Strength & Conditioning", "Thursday", 14, 70),
            ("Athletic Performance", "Thursday", 10, 75),
            ("Evening HIIT", "Friday", 18, 45),
            ("Full Body Workout", "Friday", 20, 60),
            ("Beginner Fitness", "Friday", 25, 50),
            ("Power Training", "Saturday", 12, 70),
            ("Functional Strength", "Saturday", 15, 60),
            ("Weekend Conditioning", "Saturday", 20, 45),
            ("Recovery & Mobility", "Sunday", 20, 45),
            ("Sunday Strength", "Sunday", 14, 60),
            ("Boxing Fitness", "Sunday", 18, 60),
            ("Core Blast", "Monday", 20, 40),
            ("Leg Day", "Tuesday", 12, 70),
            ("Push Pull Legs", "Wednesday", 15, 75),
            ("Fat Loss HIIT", "Thursday", 18, 45),
            ("Muscle Building", "Friday", 12, 75),
            ("Conditioning Pro", "Saturday", 14, 60),
            ("Beginner Strength", "Sunday", 20, 55),
            ("Cardio Boxing", "Tuesday", 18, 50),
            ("Athlete Conditioning", "Thursday", 10, 80)
        };

        var gymClasses = new List<GymClass>();

        for (int i = 0; i < classDefinitions.Length; i++)
        {
            gymClasses.Add(
                new GymClass
                {
                    TrainerId =
                        trainers[i % trainers.Count].Id,
                    Name = classDefinitions[i].Item1,
                    DayOfWeek = classDefinitions[i].Item2,
                    StartTime =
                        new TimeSpan(
                            6 + (i % 15),
                            (i % 2) * 30,
                            0),
                    DurationMinutes =
                        classDefinitions[i].Item3,
                    Capacity =
                        classDefinitions[i].Item4
                });
        }

        context.GymClasses.AddRange(gymClasses);
        await context.SaveChangesAsync();

        // =========================================================
        // 5. SUBSCRIPTIONS
        // =========================================================

        var now = DateTime.Now;
        var subscriptions = new List<Subscription>();

        for (int i = 0; i < members.Count; i++)
        {
            var startDate =
                now.AddMonths(-(i % 4));

            DateTime endDate;
            string status;

            if (i < 5)
            {
                // Expiring soon - useful for Admin Reports.
                endDate = now.AddDays(2 + i);
                status = "Active";
            }
            else if (i < 20)
            {
                endDate = now.AddMonths(2 + (i % 6));
                status = "Active";
            }
            else
            {
                endDate = now.AddDays(-(5 + i));
                status = "Expired";
            }

            subscriptions.Add(
                new Subscription
                {
                    MemberId = members[i].Id,
                    PlanId = plans[i % plans.Count].Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status
                });
        }

        context.Subscriptions.AddRange(subscriptions);
        await context.SaveChangesAsync();

        // =========================================================
        // 6. PAYMENTS
        // =========================================================

        var payments = new List<Payment>();

        foreach (var subscription in subscriptions)
        {
            // Do not create payment for expired subscriptions
            // so active subscriptions have a clean payment story.
            if (subscription.Status != "Active")
            {
                continue;
            }

            var plan =
                plans.First(
                    p => p.Id == subscription.PlanId);

            payments.Add(
                new Payment
                {
                    SubscriptionId = subscription.Id,
                    Amount = plan.Price,
                    PaymentDate =
                        subscription.StartDate.AddDays(1)
                });
        }

        context.Payments.AddRange(payments);
        await context.SaveChangesAsync();

        // =========================================================
        // 7. BOOKINGS
        // =========================================================

        var bookings = new List<Booking>();

        for (int i = 0; i < 40; i++)
        {
            var member =
                members[i % members.Count];

            var gymClass =
                gymClasses[(i * 3) % gymClasses.Count];

            var status =
                i % 7 == 0
                    ? "Cancelled"
                    : i % 5 == 0
                        ? "Attended"
                        : "Booked";

            bookings.Add(
                new Booking
                {
                    MemberId = member.Id,
                    GymClassId = gymClass.Id,
                    Status = status,
                    BookedAt =
                        now.AddDays(-(i % 20))
                });
        }

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();

        // =========================================================
        // 8. WEIGHT LOGS
        // =========================================================

        var weightLogs = new List<WeightLog>();

        for (int i = 0; i < 50; i++)
        {
            var member =
                members[i % members.Count];

            var baseWeight =
                member.Weight;

            var weight =
                baseWeight -
                ((i % 8) * 0.4m);

            weightLogs.Add(
                new WeightLog
                {
                    MemberId = member.Id,
                    Date =
                        now.AddDays(
                            -(60 - (i % 10) * 6)),
                    Weight = Math.Round(
                        weight,
                        1)
                });
        }

        context.WeightLogs.AddRange(weightLogs);
        await context.SaveChangesAsync();

        // =========================================================
        // 9. CHECK-INS
        // =========================================================

        var checkIns = new List<CheckIn>();

        for (int i = 0; i < 50; i++)
        {
            var member =
                members[i % members.Count];

            checkIns.Add(
                new CheckIn
                {
                    MemberId = member.Id,
                    CheckInTime =
                        now.AddDays(
                            -(i % 30))
                        .AddHours(
                            7 + (i % 10))
                });
        }

        context.CheckIns.AddRange(checkIns);
        await context.SaveChangesAsync();

        // =========================================================
        // 10. WORKOUT PLANS
        // =========================================================

        var workoutPlans = new List<WorkoutPlan>();

        for (int i = 0; i < 20; i++)
        {
            workoutPlans.Add(
                new WorkoutPlan
                {
                    MemberId =
                        members[i].Id,
                    TrainerId =
                        trainers[i % trainers.Count].Id,
                    CreatedDate =
                        now.AddDays(-(i * 3))
                });
        }

        context.WorkoutPlans.AddRange(workoutPlans);
        await context.SaveChangesAsync();

        // =========================================================
        // 11. WORKOUT EXERCISES
        // =========================================================

        var exerciseNames = new[]
        {
            "Barbell Squat",
            "Bench Press",
            "Deadlift",
            "Lat Pulldown",
            "Seated Row",
            "Shoulder Press",
            "Leg Press",
            "Romanian Deadlift",
            "Dumbbell Curl",
            "Triceps Pushdown",
            "Lateral Raise",
            "Cable Fly",
            "Walking Lunges",
            "Pull Ups",
            "Plank"
        };

        var exercises = new List<WorkoutExercise>();

        for (int i = 0; i < 40; i++)
        {
            exercises.Add(
                new WorkoutExercise
                {
                    WorkoutPlanId =
                        workoutPlans[i % workoutPlans.Count].Id,
                    ExerciseName =
                        exerciseNames[
                            i % exerciseNames.Length],
                    Sets = 3 + (i % 3),
                    Reps = 8 + (i % 7)
                });
        }

        context.WorkoutExercises.AddRange(exercises);
        await context.SaveChangesAsync();

        // =========================================================
        // 12. DIET MEALS
        // =========================================================

        var mealNames = new[]
        {
            "Breakfast",
            "Morning Snack",
            "Lunch",
            "Afternoon Snack",
            "Dinner"
        };

        var mealDescriptions = new[]
        {
            "Oats, Greek yogurt, banana and a handful of almonds.",
            "Apple with low-fat Greek yogurt.",
            "Grilled chicken, rice and mixed vegetables.",
            "Cottage cheese with fresh fruit.",
            "Grilled fish with salad and roasted potatoes."
        };

        var dietMeals = new List<DietMeal>();

        for (int i = 0; i < 60; i++)
        {
            dietMeals.Add(
                new DietMeal
                {
                    WorkoutPlanId =
                        workoutPlans[i % workoutPlans.Count].Id,
                    DayOfWeek =
                        ((DayOfWeek)(i % 7)).ToString(),
                    MealName =
                        mealNames[
                            i % mealNames.Length],
                    Description =
                        mealDescriptions[
                            i % mealDescriptions.Length]
                });
        }

        context.DietMeals.AddRange(dietMeals);
        await context.SaveChangesAsync();

        // =========================================================
        // 13. TRAINER REVIEWS
        // =========================================================

        var reviewComments = new[]
        {
            "Very professional and explains every exercise clearly.",
            "Great coach with a structured training approach.",
            "The sessions are challenging but well organized.",
            "Excellent communication and attention to technique.",
            "Helped me stay consistent with my training.",
            "Very knowledgeable about strength training.",
            "Good energy and a professional attitude.",
            "The workout plan was clear and easy to follow.",
            "Patient with beginners and explains the basics well.",
            "Great experience overall."
        };

        var reviews = new List<TrainerReview>();

        for (int i = 0; i < 20; i++)
        {
            reviews.Add(
                new TrainerReview
                {
                    TrainerId =
                        trainers[i % trainers.Count].Id,
                    MemberId =
                        members[i].Id,
                    Rating =
                        4 + (i % 2),
                    Comment =
                        reviewComments[
                            i % reviewComments.Length]
                });
        }

        context.TrainerReviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }
}