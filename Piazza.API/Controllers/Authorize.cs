using System.Net.Http.Headers;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Oauth2.v2;
using Google.Apis.Script.v1;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piazza.API.Services;
using Piazza.Database;
using Piazza.Database.Models;

namespace Piazza.API.Controllers;

[AllowAnonymous]
[Route("api/auth")]
public class Authorize(IConfiguration configuration, PiazzaContext piazzaContext, JwtService jwtService) : ControllerBase
{
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery]string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("No authorization code returned.");

        var tokenResponse = await new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer()
        {
            ClientSecrets = new ClientSecrets()
            {
                ClientId = configuration["GoogleOAuthV2:ClientId"],
                ClientSecret = configuration["GoogleOAuthV2:ClientSecret"],
            }
        }).ExchangeCodeForTokenAsync("user",
            code, "https://content-jackal-fun.ngrok-free.app/api/auth/callback", CancellationToken.None);

        var oauthService = new Oauth2Service(new BaseClientService.Initializer()
        {
            HttpClientInitializer = new UserCredential(new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer()
                {
                    ClientSecrets = new ClientSecrets()
                    {
                        ClientId = configuration["GoogleOAuthV2:ClientId"],
                        ClientSecret = configuration["GoogleOAuthV2:ClientSecret"],
                    }
                }), "user", tokenResponse),
        });

        var info = await oauthService.Userinfo.V2.Me.Get().ExecuteAsync();

        var email = info.Email.Split('@')[0];

        var user = await piazzaContext.Users.FirstOrDefaultAsync(x => x.Username == email);

        if (user == null)
        {
            user = new User()
            {
                Id = Guid.NewGuid(),
                Username = email,
            };
            await piazzaContext.Users.AddAsync(user);
            await piazzaContext.SaveChangesAsync();
        }

        var jwtToken = jwtService.GenerateJWTToken(user);

        return Ok(jwtToken);
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var googleAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        var redirectUri = "https://content-jackal-fun.ngrok-free.app/api/auth/callback";
        var responseType = "code";
        var scope = "https://www.googleapis.com/auth/userinfo.email";

        var url = $"{googleAuthUrl}?client_id={configuration["GoogleOAuthV2:ClientID"]}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}";
        return Redirect(url);
    }
}