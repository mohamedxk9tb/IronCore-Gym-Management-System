namespace GymMvc.ViewModels
{
    public class WorkoutDietPlanViewModel
    {
        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string WorkoutPlan { get; set; } = string.Empty;

        public string DietPlan { get; set; } = string.Empty;

        public List<WorkoutPlanItem> WorkoutPlans { get; set; } = new();

        public List<DietPlanItem> DietPlans { get; set; } = new();
    }

    public class WorkoutPlanItem
    {
        public int Id { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string PlanDetails { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class DietPlanItem
    {
        public int Id { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string PlanDetails { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}