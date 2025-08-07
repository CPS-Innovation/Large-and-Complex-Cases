using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace CPS.ComplexCases.Common.OpenApi.Filters
{
    /// <summary>
    /// Attribute to mark a function as not requiring security authentication
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OpenApiNoSecurityAttribute : Attribute
    {
    }

    /// <summary>
    /// Base class for security document filters with shared functionality
    /// </summary>
    public abstract class BaseSecurityDocumentFilter : IDocumentFilter
    {
        protected abstract OpenApiSecurityRequirement CreateSecurityRequirement();

        public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
        {
            var globalSecurityRequirement = CreateSecurityRequirement();
            var methodsWithNoSecurity = GetMethodsWithNoSecurityAttribute();
            var excludedOperationIds = methodsWithNoSecurity
                .Select(GetOperationIdFromMethod)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();

            foreach (var pathItem in document.Paths.Values)
            {
                foreach (var operation in pathItem.Operations.Values)
                {
                    if (!excludedOperationIds.Contains(operation.OperationId))
                    {
                        operation.Security ??= new List<OpenApiSecurityRequirement>();

                        if (!operation.Security.Any(sr => sr.ContainsKey(globalSecurityRequirement.First().Key)))
                        {
                            operation.Security.Add(globalSecurityRequirement);
                        }
                    }
                    else
                    {
                        operation.Security?.Clear();
                    }
                }
            }
        }

        protected IEnumerable<MethodInfo> GetMethodsWithNoSecurityAttribute()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                    foreach (var method in methods)
                    {
                        if (method.GetCustomAttribute<OpenApiNoSecurityAttribute>() != null)
                        {
                            yield return method;
                        }
                    }
                }
            }
        }

        protected string? GetOperationIdFromMethod(MethodInfo method)
        {
            var openApiAttr = method.GetCustomAttribute<OpenApiOperationAttribute>();
            if (openApiAttr != null && !string.IsNullOrEmpty(openApiAttr.OperationId))
            {
                return openApiAttr.OperationId;
            }

            var functionNameAttr = method.GetCustomAttribute<FunctionNameAttribute>();
            return functionNameAttr?.Name ?? method.Name;
        }
    }

    /// <summary>
    /// Adds Function Key security requirements to OpenAPI document except for functions marked with [OpenApiNoSecurity]
    /// </summary>
    public class FunctionKeySecurityDocumentFilter : BaseSecurityDocumentFilter
    {
        protected override OpenApiSecurityRequirement CreateSecurityRequirement()
        {
            return new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "FunctionKey",
                        }
                    },
                    new string[] {}
                }
            };
        }
    }

    /// <summary>
    /// Adds OAuth2 security requirements to OpenAPI document except for functions marked with [OpenApiNoSecurity]
    /// </summary>
    public class OAuth2SecurityDocumentFilter : BaseSecurityDocumentFilter
    {
        private readonly string[] _requiredScopes;

        public OAuth2SecurityDocumentFilter(params string[] requiredScopes)
        {
            _requiredScopes = requiredScopes ?? new[] { "read", "write" };
        }

        protected override OpenApiSecurityRequirement CreateSecurityRequirement()
        {
            return new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "OAuth2",
                        },
                    },
                    _requiredScopes
                }
            };
        }
    }
}