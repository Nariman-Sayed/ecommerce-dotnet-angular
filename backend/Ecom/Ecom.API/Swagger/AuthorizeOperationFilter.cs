using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ecom.API.Swagger;

/// <summary>
/// Applies the OAuth2 security requirement to operations that require authorization.
/// </summary>
public sealed class AuthorizeOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies the OAuth2 security requirement to authorized operations.
    /// </summary>
    /// <param name="operation">The operation being generated.</param>
    /// <param name="context">The context for the current operation.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor action)
            return;

        if (HasAttribute<AllowAnonymousAttribute>(action.MethodInfo) ||
            HasAttribute<AllowAnonymousAttribute>(action.ControllerTypeInfo))
            return;

        if (!HasAttribute<AuthorizeAttribute>(action.MethodInfo) &&
            !HasAttribute<AuthorizeAttribute>(action.ControllerTypeInfo))
            return;

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "oauth2"
            }
        };

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [scheme] = Array.Empty<string>()
        });
    }

    private static bool HasAttribute<TAttribute>(MemberInfo member)
        where TAttribute : Attribute =>
        member.GetCustomAttributes<TAttribute>(inherit: true).Any();
}
