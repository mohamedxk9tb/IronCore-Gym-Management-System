namespace GymMvc.Models;

public sealed record TrainerProfile(
    int Id,
    string Name,
    string Specialty,
    string Rating,
    string ImageFile,
    string Biography,
    string Experience,
    IReadOnlyList<string> Certifications,
    IReadOnlyList<string> Classes);

public static class TrainerProfiles
{
    public static readonly IReadOnlyList<TrainerProfile> All =
    [
        new(1, "Omar Hassan", "Strength & Olympic Lifting", "4.9", "trainer-01.png",
            "Omar builds strong foundations and sharp technical habits. His sessions are direct, progressive, and designed to make the next rep feel earned.",
            "12 years coaching", ["UKSCA", "BWL L2"], ["Strength Foundations", "Olympic Barbell"]),
        new(2, "Sarah Nabil", "Conditioning & HYROX", "4.8", "trainer-02.png",
            "Sarah develops durable engines for athletes who want to move with purpose. Her conditioning sessions balance pace, power, and recovery under pressure.",
            "9 years coaching", ["HYROX Foundation", "CIMSPA L3"], ["HYROX Engine", "Conditioning Circuit"]),
        new(3, "Adam Kareem", "Mobility & Recovery", "5.0", "trainer-03.png",
            "Adam helps members build freedom into every session. He combines deliberate mobility work with practical recovery habits that support long-term progress.",
            "10 years coaching", ["FRC Mobility", "NASM-CES"], ["Mobility Reset", "Recovery Flow"]),
        new(4, "Daniel Reed", "Performance Coaching", "4.9", "trainer-04.png",
            "Daniel coaches complete performance: stronger movement, clearer intent, and training plans that hold up when the work gets demanding.",
            "11 years coaching", ["NSCA-CSCS", "Precision Nutrition L1"], ["Performance Lab", "Athletic Strength"])
    ];
}
