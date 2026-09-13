using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GymMvc.Data;

namespace GymMvc.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int?> GetCurrentTrainerIdAsync()
        {
            var userId = GetIdentityUserId();
            if (userId == null) return null;

            var trainer = await _context.Trainers
                .Where(t => t.ApplicationUserId == userId)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync();

            return trainer;
        }

        public async Task<int?> GetCurrentMemberIdAsync()
        {
            var userId = GetIdentityUserId();
            if (userId == null) return null;

            // NOTE: requires Member.ApplicationUserId (Mohamed's model).
            // Until that field exists, this will not resolve correctly.
            var member = await _context.Members
                .Where(m => EF.Property<string>(m, "ApplicationUserId") == userId)
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync();

            return member;
        }

        private string? GetIdentityUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}