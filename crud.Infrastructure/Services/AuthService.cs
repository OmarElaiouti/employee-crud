
using crud.Core.Enums;
using crud.Core.Helpers;
using crud.Infrastructure.Context;
using crud.Infrastructure.Dtos.AuthDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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


        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext,
            IOptions<JWT> jwt
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _jwt = jwt.Value;


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

        public async Task<string> Login(LoginDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    return null;

                }


                var token = await GenerateJwtToken(user);
                return token;
            }
            catch (Exception ex)
            {
                // Log or handle the exception appropriately
                return null;
            }
        }

        #endregion

        #region private methods

        private async Task<string> GenerateJwtToken(IdentityUser user)
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
            var signingCredintials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredintials
                );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);


        }

        #endregion
    }
}


