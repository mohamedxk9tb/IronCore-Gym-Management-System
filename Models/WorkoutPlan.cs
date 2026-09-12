namespace GymMvc.Models
{
    public class WorkoutPlan
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public int TrainerId { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}