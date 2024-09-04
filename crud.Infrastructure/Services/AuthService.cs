
using crud.Core.Enums;
using crud.Core.Helpers;
using crud.Core.Interfaces;
using crud.Core.Models;
using crud.Infrastructure.Context;
using crud.Infrastructure.Dtos.AuthDtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;



namespace crud.Infrastructure.Services
{
    public class AuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly JWT _jwt;
        private readonly IRefreshTokenRepository _refreshTokenRepository;


        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext,
            IOptions<JWT> jwt,
            IRefreshTokenRepository refreshTokenRepository
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _jwt = jwt.Value;
            _refreshTokenRepository = refreshTokenRepository;
            


        }



        #region public methods

        public async Task<RegistrationResult> Register(RegisterDto model)
        {
            var email = model.Email;
            var password = model.Password;
            var username = model.Username;


            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return new RegistrationResult { Status = RegistrationStatus.UserAlreadyExists, Message = "User already exists." };
            }


            var newUser = new IdentityUser
            {
                Email = email,
                UserName = email
            };

            try
            {

                var result = await _userManager.CreateAsync(newUser, password);
                if (!result.Succeeded)
                {

                    // Rollback transaction and return false
                    return new RegistrationResult { Status = RegistrationStatus.OtherError, Message = "Failed to create user." };
                }

                var roleExists = await _roleManager.RoleExistsAsync(Role.Empoloyee);
                if (!roleExists)
                {
                    // Role does not exist, create it
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(Role.Empoloyee));
                    if (!roleResult.Succeeded)
                    {

                        return new RegistrationResult { Status = RegistrationStatus.OtherError, Message = "Failed to create client role." };
                    }
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, Role.Empoloyee);
                if (!addToRoleResult.Succeeded)
                {

                    return new RegistrationResult { Status = RegistrationStatus.OtherError, Message = "Failed to add user to client role." };
                }
                return new RegistrationResult { Status = RegistrationStatus.Success, Message = "User registered successfully." };
            }
            catch (Exception ex)
            {

                return new RegistrationResult { Status = RegistrationStatus.OtherError, Message = "An error occurred during registration." };

            }

        }

        public async Task<LoginResultDto?> Login(LoginDto model, HttpContext httpContext)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return null;
                }

                var tokens = await GenerateTokens(user, httpContext);
                return new LoginResultDto
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> RefreshToken(HttpContext httpContext, string token)
        {
            var isValid = await ValidateRefreshToken(token);
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (!isValid)
            {
                await _refreshTokenRepository.RemoveAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();
                throw new SecurityTokenInvalidAudienceException("Refresh token is expired.");
            }

            var currentIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var currentUserAgent = httpContext.Request.Headers["User-Agent"].ToString();

            if (refreshToken.IpAddress != currentIpAddress || refreshToken.UserAgent != currentUserAgent)
            {
                throw new SecurityTokenInvalidAudienceException("Token usage from an unrecognized device or location.");
            }

            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            var Tokens = await GenerateTokens(user, httpContext, token);

            return Tokens.AccessToken;
        }
        
        #endregion

        #region private methods

        private async Task<(string AccessToken, string RefreshToken)> GenerateTokens(IdentityUser user, HttpContext httpContext, string? currentRefreshToken = null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim(ClaimTypes.Role, role));

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        }
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            if (currentRefreshToken == null)
            {
                var refreshToken = new ApplicationRefreshToken
                {
                    Token = GenerateSecureToken(),
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                    IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext.Request.Headers["User-Agent"].ToString()
                };

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                return (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken.Token);
            }
            return (new JwtSecurityTokenHandler().WriteToken(accessToken), currentRefreshToken);

        }
        private async Task<bool> ValidateRefreshToken(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
            return refreshToken != null && refreshToken.ExpiryDate >= DateTime.UtcNow;
        }
        private string GenerateSecureToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var tokenBytes = new byte[32];
                rng.GetBytes(tokenBytes);
                return Convert.ToBase64String(tokenBytes);
            }
        }

        #endregion
    }
}


