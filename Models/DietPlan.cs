namespace GymMvc.Models
{
    public class DietPlan
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public int TrainerId { get; set; }

        public string PlanDetails { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}