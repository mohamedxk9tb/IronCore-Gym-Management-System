using Microsoft.AspNetCore.Identity;

namespace GymMvc.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public Member? Member { get; set; }
}