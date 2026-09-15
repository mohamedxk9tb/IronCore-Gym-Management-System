using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymMvc.Migrations
{
    /// <inheritdoc />
    public partial class FixEntityRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_GymClasses_GymClassId1",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_GymClasses_Trainers_TrainerId1",
                table: "GymClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Subscriptions_SubscriptionId1",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Plans_PlanId1",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainerReviews_Trainers_TrainerId1",
                table: "TrainerReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlans_Trainers_TrainerId1",
                table: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_TrainerId1",
                table: "WorkoutPlans");

            migrationBuilder.DropIndex(
                name: "IX_TrainerReviews_TrainerId1",
                table: "TrainerReviews");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PlanId1",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SubscriptionId1",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_GymClasses_TrainerId1",
                table: "GymClasses");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_GymClassId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TrainerId1",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "TrainerId1",
                table: "TrainerReviews");

            migrationBuilder.DropColumn(
                name: "PlanId1",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriptionId1",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TrainerId1",
                table: "GymClasses");

            migrationBuilder.DropColumn(
                name: "GymClassId1",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainerId1",
                table: "WorkoutPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrainerId1",
                table: "TrainerReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanId1",
                table: "Subscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionId1",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrainerId1",
                table: "GymClasses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GymClassId1",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_TrainerId1",
                table: "WorkoutPlans",
                column: "TrainerId1");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerReviews_TrainerId1",
                table: "TrainerReviews",
                column: "TrainerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId1",
                table: "Subscriptions",
                column: "PlanId1");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId1",
                table: "Payments",
                column: "SubscriptionId1");

            migrationBuilder.CreateIndex(
                name: "IX_GymClasses_TrainerId1",
                table: "GymClasses",
                column: "TrainerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GymClassId1",
                table: "Bookings",
                column: "GymClassId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_GymClasses_GymClassId1",
                table: "Bookings",
                column: "GymClassId1",
                principalTable: "GymClasses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GymClasses_Trainers_TrainerId1",
                table: "GymClasses",
                column: "TrainerId1",
                principalTable: "Trainers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Subscriptions_SubscriptionId1",
                table: "Payments",
                column: "SubscriptionId1",
                principalTable: "Subscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Plans_PlanId1",
                table: "Subscriptions",
                column: "PlanId1",
                principalTable: "Plans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainerReviews_Trainers_TrainerId1",
                table: "TrainerReviews",
                column: "TrainerId1",
                principalTable: "Trainers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlans_Trainers_TrainerId1",
                table: "WorkoutPlans",
                column: "TrainerId1",
                principalTable: "Trainers",
                principalColumn: "Id");
        }
    }
}
