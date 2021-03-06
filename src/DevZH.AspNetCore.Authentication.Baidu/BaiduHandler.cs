﻿using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using DevZH.AspNetCore.Authentication.Internal;
using DevZH.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json.Linq;

namespace DevZH.AspNetCore.Authentication.Baidu
{
    /// <summary>
    /// 对一系列认证过程的调控
    /// </summary>
    public class BaiduHandler : OAuthHandler<BaiduOptions>
    {
        public BaiduHandler(HttpClient backchannel) : base(backchannel)
        {
        }

        /// <summary>
        /// 主要生成的是 Authorization 的链接。其中，display 等是百度对 OAuth 2.0 的非标准扩展标识，用来控制外观及行为显示
        /// </summary>
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var queryBuilder = new QueryBuilder {
                { "client_id", Options.ClientId },
                { "scope", FormatScope() },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "state", Options.StateDataFormat.Protect(properties) },
                { "display", Options.Display.GetDescription()}
            };
            if (Options.IsForce) queryBuilder.Add("force_login", "1");
            if (Options.IsConfirm) queryBuilder.Add("confirm_login", "1");
            if (Options.UseSms) queryBuilder.Add("login_type", "sms");
            return Options.AuthorizationEndpoint + queryBuilder;
        }

        /// <summary>
        /// 根据获取到的 token，来得到登录用户的基本信息，并配对。
        /// </summary>
        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            var endpoint = Options.UserInformationEndpoint + "?access_token=" + UrlEncoder.Encode(tokens.AccessToken);
            var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve Baidu user information ({response.StatusCode}) Please check if the authentication information is correct and the corresponding Baidu API is enabled.");
            }

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Options.AuthenticationScheme);
            var context = new OAuthCreatingTicketContext(ticket, Context, Options, Backchannel, tokens, payload);

            var identifier = BaiduHelper.GetId(payload);
            if (!string.IsNullOrEmpty(identifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
                identity.AddClaim(new Claim("urn:baidu:id", identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var name = BaiduHelper.GetName(payload);
            if (!string.IsNullOrEmpty(name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, name, ClaimValueTypes.String, Options.ClaimsIssuer));
                identity.AddClaim(new Claim("urn:baidu:name", name, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var portrait = BaiduHelper.GetPortrait(payload);
            if (!string.IsNullOrEmpty(portrait))
            {
                identity.AddClaim(new Claim("urn:baidu:portrait", portrait, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            await Options.Events.CreatingTicket(context);

            return context.Ticket;
        }
    }
}