using crud.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crud.Core.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<ApplicationRefreshToken> GetByTokenAsync(string token);
        Task<ApplicationRefreshToken> GetByUserIdAsync(string id);
        Task AddAsync(ApplicationRefreshToken refreshToken);
        Task RemoveAsync(ApplicationRefreshToken refreshToken);
        Task SaveChangesAsync();
    }
}
