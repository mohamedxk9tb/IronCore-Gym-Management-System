using System.Collections.Generic;

namespace GymMvc.ViewModels
{
    public class ReportsViewModel
    {
        public decimal MonthlyIncome { get; set; }
        public int ActiveMembers { get; set; }

        // بيانات الشارت (آخر N شهور)
        public List<string> Months { get; set; } = new();
        public List<decimal> IncomeData { get; set; } = new();
        public List<int> MembersData { get; set; } = new();

        // تقارير إضافية مطلوبة
        public List<ExpiringMembershipViewModel> ExpiringMembershipsThisWeek { get; set; } = new();
        public List<PopularClassViewModel> MostPopularClasses { get; set; } = new();
    }

    public class ExpiringMembershipViewModel
    {
        public string MemberName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public System.DateTime EndDate { get; set; }
    }

    public class PopularClassViewModel
    {
        public string ClassName { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
    }
}