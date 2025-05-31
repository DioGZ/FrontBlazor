using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;


namespace FrontBlazor.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private string _token = "";

        public void SetToken(string token)
        {
            _token = token;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }        
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                return Task.FromResult(new AuthenticationState(_anonymous));
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(_token);

                var claims = jwt.Claims.ToList();
                
                // Asegurarse de que tenemos un claim de nombre
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
                    if (!string.IsNullOrEmpty(email))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, email));
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                
                return Task.FromResult(new AuthenticationState(user));
            }
            catch
            {
                return Task.FromResult(new AuthenticationState(_anonymous));
            }
        }
    }
}