using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Common;
using Core.Settings;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Newtonsoft.Json.Linq;

namespace BCNExplorer.Web.Controllers
{
    public class SecurityController : Controller
    {
        private readonly BaseSettings _baseSettings;

        public SecurityController(BaseSettings baseSettings)
        {
            _baseSettings = baseSettings;
        }

        [Route("auth")]
        public async Task<ActionResult> Auth()
        {

            ViewBag.Code = Request.QueryString["code"] ?? "none";

            var state = Request.QueryString["state"];
            var tempState = await GetTempStateAsync();

            if (state.Equals(tempState.Item1, StringComparison.Ordinal))
            {
                ViewBag.State = state + " (valid)";
            }
            else
            {
                ViewBag.State = state + " (invalid)";
            }

            ViewBag.Error = Request.QueryString["error"] ?? "none";

            return View();
        }
        
        public async Task<ActionResult> GetToken(string code)
        {
            var client = new TokenClient(
                _baseSettings.Authentication.TokenEndpoint,
                _baseSettings.Authentication.ClientId,
                _baseSettings.Authentication.ClientSecret);
            
            var tempState = await GetTempStateAsync();
            Request.GetOwinContext().Authentication.SignOut("TempState");

            var response = await client.RequestAuthorizationCodeAsync(
                code,
                _baseSettings.Authentication.RedirectUri);

            await ValidateResponseAndSignInAsync(response, tempState.Item2);

            if (!string.IsNullOrEmpty(response.IdentityToken))
            {
                ViewBag.IdentityTokenParsed = ParseJwt(response.IdentityToken);
            }
            if (!string.IsNullOrEmpty(response.AccessToken))
            {
                ViewBag.AccessTokenParsed = ParseJwt(response.AccessToken);
            }

            return RedirectToAction("Index", "Home");
        }

        //[Authorize]
        public ActionResult SignIn()
        {
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");
            SetTempState(state, nonce);

            var request = new AuthorizeRequest(_baseSettings.Authentication.AuthorizeEndpoint);

            var url = request.CreateAuthorizeUrl(
                clientId: _baseSettings.Authentication.ClientId,
                responseType: "code",
                scope: "email profile",
                redirectUri: _baseSettings.Authentication.RedirectUri,
                state: state,
                nonce: nonce);
            return Redirect(url);
        }

        private void SetTempState(string state, string nonce)
        {
            var tempId = new ClaimsIdentity("TempState");
            tempId.AddClaim(new Claim("state", state));
            tempId.AddClaim(new Claim("nonce", nonce));
            //Request.GetOwinContext().Authentication.SignIn(new AuthenticationProperties() { IsPersistent = true}, tempId);
            Request.GetOwinContext().Authentication.SignIn(tempId);
        }

        private async Task<Tuple<string, string>> GetTempStateAsync()
        {
            var data = await Request.GetOwinContext().Authentication.AuthenticateAsync("TempState");

            var state = data.Identity.FindFirst("state").Value;
            var nonce = data.Identity.FindFirst("nonce").Value;

            return Tuple.Create(state, nonce);
        }

        private async Task ValidateResponseAndSignInAsync(TokenResponse response, string nonce)
        {
            var claims = new List<Claim>();

            if (!string.IsNullOrWhiteSpace(response.AccessToken))
            {
                claims.AddRange(await GetUserInfoClaimsAsync(response.AccessToken));

                claims.Add(new Claim("access_token", response.AccessToken));
                claims.Add(new Claim("expires_at", (DateTime.UtcNow.ToEpochTime() + response.ExpiresIn).ToDateTimeFromEpoch().ToString()));
            }

            if (!string.IsNullOrWhiteSpace(response.RefreshToken))
            {
                claims.Add(new Claim("refresh_token", response.RefreshToken));
            }

            var id = new ClaimsIdentity(claims, "Cookies");
            
            Request.GetOwinContext().Authentication.SignIn(new AuthenticationProperties() { IsPersistent = true }, id);

        }

        private async Task<IEnumerable<Claim>> GetUserInfoClaimsAsync(string accessToken)
        {
            var userInfoClient = new UserInfoClient(new Uri(_baseSettings.Authentication.UserInfoEndpoint), accessToken);

            var userInfo = await userInfoClient.GetAsync();

            var claims = new List<Claim>();
            userInfo.Claims.ToList().ForEach(ui => claims.Add(new Claim(ui.Item1, ui.Item2)));

            return claims;
        }

        private string ParseJwt(string token)
        {
            if (!token.Contains("."))
            {
                return token;
            }

            var parts = token.Split('.');
            var part = Encoding.UTF8.GetString(Base64Url.Decode(parts[1]));

            var jwt = JObject.Parse(part);
            return jwt.ToString();
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var authManager = HttpContext.GetOwinContext().Authentication;

                authManager.SignOut("Cookies");
                authManager.SignOut("OpenIdConnect");

            }
            return RedirectToAction("Index", "Home");
        }
    }

    public class AuthorizeRequest
    {
        private readonly Uri _authorizeEndpoint;

        public AuthorizeRequest(Uri authorizeEndpoint)
        {
            this._authorizeEndpoint = authorizeEndpoint;
        }

        public AuthorizeRequest(string authorizeEndpoint)
        {
            this._authorizeEndpoint = new Uri(authorizeEndpoint);
        }

        public string Create(IDictionary<string, string> values)
        {
            return string.Format("{0}?{1}", (object)this._authorizeEndpoint.AbsoluteUri, (object)string.Join("&", Enumerable.ToArray<string>(Enumerable.Select<KeyValuePair<string, string>, string>((IEnumerable<KeyValuePair<string, string>>)values, (Func<KeyValuePair<string, string>, string>)(kvp => string.Format("{0}={1}", (object)WebUtility.UrlEncode(kvp.Key), (object)WebUtility.UrlEncode(kvp.Value)))))));
        }
    }

    public static class AuthorizeRequestExtensions
    {
        public static string Create(this AuthorizeRequest request, object values)
        {
            return request.Create((IDictionary<string, string>)AuthorizeRequestExtensions.ObjectToDictionary(values));
        }

        public static string CreateAuthorizeUrl(this AuthorizeRequest request, string clientId, string responseType, string scope = null, string redirectUri = null, string state = null, string nonce = null, string loginHint = null, string acrValues = null, string prompt = null, string responseMode = null, string codeChallenge = null, string codeChallengeMethod = null, object extra = null)
        {
            Dictionary<string, string> explicitValues = new Dictionary<string, string>()
      {
        {
          "client_id",
          clientId
        },
        {
          "response_type",
          responseType
        }
      };
            if (!string.IsNullOrWhiteSpace(scope))
                explicitValues.Add("scope", scope);
            if (!string.IsNullOrWhiteSpace(redirectUri))
                explicitValues.Add("redirect_uri", redirectUri);
            if (!string.IsNullOrWhiteSpace(state))
                explicitValues.Add("state", state);
            if (!string.IsNullOrWhiteSpace(nonce))
                explicitValues.Add("nonce", nonce);
            if (!string.IsNullOrWhiteSpace(loginHint))
                explicitValues.Add("login_hint", loginHint);
            if (!string.IsNullOrWhiteSpace(acrValues))
                explicitValues.Add("acr_values", acrValues);
            if (!string.IsNullOrWhiteSpace(prompt))
                explicitValues.Add("prompt", prompt);
            if (!string.IsNullOrWhiteSpace(responseMode))
                explicitValues.Add("response_mode", responseMode);
            if (!string.IsNullOrWhiteSpace(codeChallenge))
                explicitValues.Add("code_challenge", codeChallenge);
            if (!string.IsNullOrWhiteSpace(codeChallengeMethod))
                explicitValues.Add("code_challenge_method", codeChallengeMethod);
            return request.Create((IDictionary<string, string>)AuthorizeRequestExtensions.Merge(explicitValues, AuthorizeRequestExtensions.ObjectToDictionary(extra)));
        }

        private static Dictionary<string, string> ObjectToDictionary(object values)
        {
            if (values == null)
                return (Dictionary<string, string>)null;
            Dictionary<string, string> dictionary1 = values as Dictionary<string, string>;
            if (dictionary1 != null)
                return dictionary1;
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
            foreach (PropertyInfo propertyInfo in RuntimeReflectionExtensions.GetRuntimeProperties(values.GetType()))
            {
                string str = propertyInfo.GetValue(values) as string;
                if (!string.IsNullOrEmpty(str))
                    dictionary2.Add(propertyInfo.Name, str);
            }
            return dictionary2;
        }

        private static Dictionary<string, string> Merge(Dictionary<string, string> explicitValues, Dictionary<string, string> additionalValues = null)
        {
            Dictionary<string, string> dictionary = explicitValues;
            if (additionalValues != null)
                dictionary = Enumerable.ToDictionary<KeyValuePair<string, string>, string, string>(Enumerable.Concat<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>)explicitValues, Enumerable.Where<KeyValuePair<string, string>>((IEnumerable<KeyValuePair<string, string>>)additionalValues, (Func<KeyValuePair<string, string>, bool>)(add => !explicitValues.ContainsKey(add.Key)))), (Func<KeyValuePair<string, string>, string>)(final => final.Key), (Func<KeyValuePair<string, string>, string>)(final => final.Value));
            return dictionary;
        }
    }
}