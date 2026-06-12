using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BMDb.Services
{
    public interface IUserKeyService
    {
        int GetCurrentUserKey(ClaimsPrincipal user);
    }

    public class UserKeyService : IUserKeyService
    {
        public int GetCurrentUserKey(ClaimsPrincipal user)
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
            {
                return 0;
            }

            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(id));
            return Math.Abs(BitConverter.ToInt32(bytes, 0));
        }
    }
}
