using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.CrossCutting;
using Bk.Termdat.CrossCutting.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Bk.Termdat.Common.Api;

public static class ServiceCollectionExtension
{
    public static void AddSwaggerGenWithKeyCloak(this IServiceCollection services, AuthenticationOptions authenticationOptions)
    {
        const string securityScheme = "oauth2";
        services.AddSwaggerGen(config =>
        {
            config.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}_{e.RelativePath}_{e.HttpMethod}");
            config.AddSecurityDefinition(securityScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authenticationOptions.Url + "/protocol/openid-connect/auth"),
                        TokenUrl = new Uri(authenticationOptions.Url + "/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string> { { "openid", "OpenId Scope" } }
                    }
                }
            });

            config.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(securityScheme, document)] = []
            });
        });
    }

    public static void AddAuthenticationWithJwtBearer(this IServiceCollection services, AuthenticationOptions authenticationOptions)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = authenticationOptions.Url;
            options.Audience = authenticationOptions.ClientId;
            options.TokenValidationParameters.ValidAudience = authenticationOptions.ClientId;
            options.TokenValidationParameters.ValidIssuer = authenticationOptions.Url;

            options.RequireHttpsMetadata = authenticationOptions.IsHttps;
            options.SaveToken = true;
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context => await AddCollectionAndGlossaryClaim(context),
            };
        });
    }

    private static async Task AddCollectionAndGlossaryClaim(TokenValidatedContext context)
    {
        var serviceProvider = context.HttpContext.RequestServices;

        var email = context.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var repository = serviceProvider.GetRequiredService<IUserAccountRepository>();
        var userAccount = await repository.GetDetailByEmail(email);
        if (userAccount == null)
        {
            return;
        }

        var identity = new ClaimsIdentity(new List<Claim>
        {
            new(CustomClaimTypes.CollectionIds,
              userAccount.AuthorizedCollections != null ? string.Join(",", userAccount.AuthorizedCollections.Select(y => y.Id)) : string.Empty),
            new(CustomClaimTypes.GlossaryIds,
             userAccount.AuthorizedGlossaries != null ? string.Join(",", userAccount.AuthorizedGlossaries.Select(y => y.Id)) :  string.Empty),
        });

        context.Principal.AddIdentity(identity);
    }

    public static void AddTermdatAuthorizationPolicies(this IServiceCollection services, AuthorizationOptions authorizationOptions)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Policy.RequireAdminPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.Admin)
            );
            options.AddPolicy(
                Policy.RequireWriterPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.Writer)
            );
            options.AddPolicy(
                Policy.RequireSuperUserPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.SuperUser)
            );
            options.AddPolicy(
                Policy.RequireAdminOrWriterPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.Admin, authorizationOptions.Writer));
            options.AddPolicy(
                Policy.RequireAdminOrWriterOrSuperUserPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.Admin, authorizationOptions.Writer, authorizationOptions.SuperUser));
            options.AddPolicy(
                Policy.RequireAdminOrWriterOrSuperUserOrSuperReaderPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.Admin, authorizationOptions.Writer, authorizationOptions.SuperUser, authorizationOptions.SuperReader));
            options.AddPolicy(
                Policy.RequireAdminOrSuperUserPolicy,
                p => p.RequireRole(authorizationOptions.Allow).RequireRole(authorizationOptions.Admin, authorizationOptions.SuperUser));
        });
    }
}
