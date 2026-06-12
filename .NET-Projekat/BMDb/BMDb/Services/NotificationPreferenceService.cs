using System.Security.Claims;
using BMDb.Data;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface INotificationPreferenceService
    {
        Task<bool> CanCurrentUserViewNewsAsync(ClaimsPrincipal user);
    }

    public class NotificationPreferenceService : INotificationPreferenceService
    {
        private readonly ApplicationDbContext _context;

        public NotificationPreferenceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CanCurrentUserViewNewsAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.NotifikacijeUkljucene)
                .FirstOrDefaultAsync();
        }
    }
}
