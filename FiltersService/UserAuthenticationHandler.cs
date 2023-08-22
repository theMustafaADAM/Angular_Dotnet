using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Net.Http.Headers;
using System.Security.Claims;
using ModelService;
using System.Text;
using AuthService;
using DataService;
using System.Net;
using Serilog;


namespace FiltersService
{
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public class UserAuthenticationHandler : AuthenticationHandler<UserAuthenticationOptions>
	{
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _provider;
        private readonly AppSettings _appSettings;
        private readonly IAuthSvc _authSvc;
        private string[] UserRoles = new[] { "Administrator", "Customer" };
        private const string RefreshToken = "refreshToken";
        private const string AccessToken = "access_token";
        private const string Username = "username";
        private const string UserRole = "userRole";
        private const string User_Id = "user_id";
        private string decryptedTokenLayerTwo;
        private ClaimsPrincipal validateToken;
        private TokenValidationParameters validationParameters;
        private JwtSecurityTokenHandler handler;

        public UserAuthenticationHandler(
            IOptionsMonitor<UserAuthenticationOptions> options,
            IOptions<DataProtectionKeys> dataProtectionKeys,
            UserManager<ApplicationUser> userManager,
            IOptions<AppSettings> appSettings,
            IServiceProvider provider,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IAuthSvc authSvc
        ) : base(options, logger, encoder, clock)
        {
            _authSvc = authSvc;
            _provider = provider;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _dataProtectionKeys = dataProtectionKeys.Value;

        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Cookies.ContainsKey(AccessToken) || !Request.Cookies.ContainsKey(User_Id))
            {
                Log.Error("No Access Token or User Id found.");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[AccessToken]}",
                out AuthenticationHeaderValue headerValue))
            {
                Log.Error("Could not Parse Token from Authentication Header.");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }
            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[User_Id]}",
                out AuthenticationHeaderValue headerValueUid))
            {
                Log.Error("Could not Parse User Id from Authentication Header.");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            try
            {
                /* STEP 1. Get the Validation Parameters for our applications JWT Token */
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

                /* STEP 2. Create an instance of Jwt token handler */
                handler = new JwtSecurityTokenHandler();

                /* STEP 3. Create an instance of Jwt token  validation parameters */
                validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _appSettings.Site,
                    ValidAudience = _appSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                /* STEP 4. Get the Data protection service instance */
                var protectorProvider = _provider.GetService<IDataProtectionProvider>();

                /* STEP 5. create a protector instance */
                var protector = protectorProvider.CreateProtector(_dataProtectionKeys.ApplicationUserKey);

                /* STEP 6. Layer One Unprotect the user id */
                var decryptedUid = protector.Unprotect(headerValueUid.Parameter);

                /* STEP 7. Layer One Unprotect the user token */
                var decryptedToken = protector.Unprotect(headerValue.Parameter);

                /* STEP 8. Create an instance of the user tokenModel */
                TokenModel tokenModel;

                /* STEP 9 Get the existing token for the user from Database */
                /* Step 10. Create a scoped request */
                using (var scope = _provider.CreateScope())
                {
                    var dbContextService = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var userToken = dbContextService.Tokens.Include(x => x.User).
                        FirstOrDefault(ut => ut.UserID == decryptedUid
                                             && ut.User.UserName == Request.Cookies[Username]
                                             && ut.User.Id == decryptedUid);
                    tokenModel = userToken;
                }

                if (tokenModel == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));
                }
                /* STEP 11. Apply second layer of decryption using the key store in the token model */
                /* STEP 11.1 Create Protector instance for layer two using token model key */
                /* IMPORTANT - If np key exists or key is invalid - exception will be thrown */
                IDataProtector layerTwoProtector = protectorProvider.CreateProtector(tokenModel?.EncryptionKeyJwt);
                decryptedTokenLayerTwo = layerTwoProtector.Unprotect(decryptedToken);

                /* STEP 12. Validate the token we received - using validation parameters set in step 3 */
                /* IMPORTANT - If the validation fails - the method ValidateToken will throw exception */
                validateToken = handler.ValidateToken(decryptedTokenLayerTwo, validationParameters, out var securityToken);

                /* Checking Token Signature */
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Your are not authorized"));
                }

                /* STEP 13. Extract the email from the validated token */
                var username = validateToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                /* STEP 14. Get User by their email */

                if (Request.Cookies[Username] != username)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));
                }

                var user = await _userManager.FindByNameAsync(username);

                /* STEP 15. If user does not exist return authentication failed result */
                if (user == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));
                }

                /* STEP 16. We need to check if the user belongs to the group of user-roles */

                if (!UserRoles.Contains(user.UserRole))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to View this Page"));
                }

                /* STEP 17. Now we will create an authentication ticket, as the token is valid */
                var identity = new ClaimsIdentity(validateToken.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return await Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(SecurityTokenExpiredException))
                {
                    if (_appSettings.AllowSiteWideTokenRefresh)
                    {
                        var refreshToken = Request.Cookies[RefreshToken];
                        var accessToken = Request.Cookies[AccessToken];
                        var userId = Request.Cookies[User_Id];
                        var username = Request.Cookies[Username];
                        var role = Request.Cookies[UserRole];


                        if (accessToken != null && userId != null)
                        {
                            // Call the refresh token method if it is valid
                            TokenRequestModel model = new TokenRequestModel
                            {
                                RefreshToken = refreshToken,
                                GrantType = "refresh_token",
                                UserName = username
                            };
                            var result = await _authSvc.Auth(model);

                            if (result.ResponseInfo.StatusCode == HttpStatusCode.OK)
                            {
                                var identity = new ClaimsIdentity(result.Principal.Claims, Scheme.Name);
                                var principal = new ClaimsPrincipal(identity);
                                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                                return await Task.FromResult(AuthenticateResult.Success(ticket));
                            }


                        }
                    }

                }

                Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return await Task.FromResult(AuthenticateResult.Fail("Your are not authorized"));
            }
        }
    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}