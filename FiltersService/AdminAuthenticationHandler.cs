using Serilog;
using System.Text;
using DataService;
using ModelService;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace FiltersService
{
    public class AdminAuthenticationHandler : AuthenticationHandler<AdminAuthenticationOptions>
    {
        private readonly IdentityDefaultOptions _identityDefaultOptions;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DataProtectionKeys _dataProtectionKeys;
        private readonly IServiceProvider _Provider;
        private readonly AppSettings _appSettings;

        private const string AccessToken = "access_token";
        private const string Username = "username";
        private const string User_ID = "user_id";
        private string[] UserRoles = new[] { "Administrator" };

        public AdminAuthenticationHandler(
            UrlEncoder encoder,
            ISystemClock clock,
            ILoggerFactory logger,
            IServiceProvider provider,
            IOptions<AppSettings> appSettings,
            UserManager<ApplicationUser> userManager,
            IOptions<DataProtectionKeys> dataProtectionKeys,
            IOptionsMonitor<AdminAuthenticationOptions> options,
            IOptions<IdentityDefaultOptions> identityDefaultOptions
        ) : base(options, logger, encoder, clock)
        {
            _identityDefaultOptions = identityDefaultOptions.Value;
            _dataProtectionKeys = dataProtectionKeys.Value;
            _appSettings = appSettings.Value;
            _userManager = userManager;
            _Provider = provider;
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Cookies.ContainsKey(AccessToken) || !Request.Cookies.ContainsKey(User_ID))
            {
                Log.Error("No Access Token Or User ID Found !");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[AccessToken]}",
                out var headerValue))
            {
                Log.Error("Could not Parse Token from Authentication Header !");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!AuthenticationHeaderValue.TryParse($"{"Bearer " + Request.Cookies[User_ID]}",
                out var headerValueUid))
            {
                Log.Error("Could not Parse User ID from Authentication Header !");
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            try
            {

                /* Step1: Get the validation Parameters for our applications JWT Token */
                byte[] key = Encoding.ASCII.GetBytes(s: _appSettings.Secret);

                /* Step2: Create an instance of Jwt token handler */
                var handler = new JwtSecurityTokenHandler();

                /* Step3: Create an instance of Jwt token validation parameter */
                TokenValidationParameters validationParameters = new TokenValidationParameters
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

                /* Step4: Get the data protection service instance */
                var protectorProvider = _Provider.GetService<IDataProtectionProvider>();

                /* Step5: Create a protector instance */

                IDataProtector protector = protectorProvider.CreateProtector(purpose: _dataProtectionKeys.ApplicationUserKey);

                /* Step6: Layer One Unprotect the user id */
                string decryptedUid = protector.Unprotect(protectedData: headerValueUid.Parameter);

                /* Step7: Layer One Unprotect the user token */
                string decryptedToken = protector.Unprotect(protectedData: headerValue.Parameter);

                /* Step8: Create an instance of the user tokenModel */
                TokenModel tokenModel = new TokenModel();

                /* Step9: Get the existing token for the user from Database using a scoped request */
                using (var scope = _Provider.CreateScope())
                {
                    var dbContextService = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    TokenModel? userToken = dbContextService.Tokens.Include(x => x.User)
                        .FirstOrDefault(ut => ut.UserID == decryptedUid &&
                                              ut.User.UserName == Request.Cookies[Username] &&
                                              ut.User.Id == decryptedUid &&
                                              ut.User.UserRole == "Administrator");
                    tokenModel = userToken;
                }

                /* Step10: Check if tokenModel is null */
                if (tokenModel == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to view this page !"));
                }

                /* Step11: Apply second layer of decryption using the key store in the token model */
                /* Step12: Create protector instance for layer two using token model key */
                /* IMPORTANT: if no key exists or key is invalid , exception will be thrown */
                IDataProtector layerTwoProtector = protectorProvider.CreateProtector(purpose: tokenModel?.EncryptionKeyJwt);
                string decryptedTokenLayerTwo = layerTwoProtector.Unprotect(decryptedToken);

                /* Step13:  Validate the token we received - using validation parameters set in step 3 */
                /* IMPORTANT: if the validation fails, the method ValidateToken will throw exception */
                var validateToke = handler.ValidateToken(decryptedTokenLayerTwo, validationParameters, out var securityToken);

                /* Step 14: Checking Token Signature */
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Yue are not authorized !"));
                }

                /* Step 15: Extract the username from the validated token */
                var username = validateToke.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;

                if (Request.Cookies[key: username] != username)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to view this page !"));
                }

                /* Step 16: Get email by the username */
                var user = await _userManager.FindByNameAsync(username);

                /* Step 17: if null return authentication failed result */
                if(user == null)
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to view this page !"));
                }

                /* Step 18: Check if user belonges to the group of user role */
                if (!UserRoles.Contains(user.UserRole))
                {
                    return await Task.FromResult(AuthenticateResult.Fail("You are not authorized to view this page !"));
                }

                /* Step 19: Now we will create an authentication ticket, as the token is valid */
                var identity = new ClaimsIdentity(validateToke.Claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return await Task.FromResult(AuthenticateResult.Success(ticket));

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                return await Task.FromResult(AuthenticateResult.Fail("Yue are not authorized !"));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            //return base.HandleChallengeAsync(properties);

            /*   Logic handling failed requests   */
            /*   Delete any invalid cookies       */

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("user_id");

            Response.Headers["WWW-Authenticate"] = $"Not Authorized";
            Response.Redirect(location: _identityDefaultOptions.AccessDeniedPath);


            return Task.CompletedTask;
        }

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }
}

