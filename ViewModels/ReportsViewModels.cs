namespace GymMvc.ViewModels
{
    public class ReportsViewModel
    {
        public decimal MonthlyIncome { get; set; }

        public int ActiveMembers { get; set; }

        public List<string> Months { get; set; } = new();

        public List<decimal> IncomeData { get; set; } = new();

        public List<int> MembersData { get; set; } = new();
    }
}