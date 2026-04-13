using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChengYuan.Core.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ChengYuan.AspNetCore;

internal sealed class AutoApiControllerConvention(AutoApiControllerOptions options) : IApplicationModelConvention
{
    private static readonly Dictionary<string, string> HttpMethodPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Get"] = "GET",
        ["Find"] = "GET",
        ["Query"] = "GET",
        ["Create"] = "POST",
        ["Add"] = "POST",
        ["Insert"] = "POST",
        ["Update"] = "PUT",
        ["Put"] = "PUT",
        ["Delete"] = "DELETE",
        ["Remove"] = "DELETE",
        ["Patch"] = "PATCH",
    };

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            if (!typeof(IApplicationService).IsAssignableFrom(controller.ControllerType))
            {
                continue;
            }

            ConfigureController(controller);
        }
    }

    private void ConfigureController(ControllerModel controller)
    {
        ConfigureApiExplorer(controller);
        ConfigureControllerName(controller);
        ConfigureRoute(controller);

        foreach (var action in controller.Actions)
        {
            ConfigureAction(action);
        }
    }

    private static void ConfigureApiExplorer(ControllerModel controller)
    {
        controller.ApiExplorer.IsVisible ??= true;
    }

    private static void ConfigureControllerName(ControllerModel controller)
    {
        var name = controller.ControllerType.Name;

        if (name.EndsWith("AppService", StringComparison.Ordinal))
        {
            name = name[..^"AppService".Length];
        }
        else if (name.EndsWith("ApplicationService", StringComparison.Ordinal))
        {
            name = name[..^"ApplicationService".Length];
        }
        else if (name.EndsWith("Service", StringComparison.Ordinal))
        {
            name = name[..^"Service".Length];
        }

        controller.ControllerName = name;
    }

    private void ConfigureRoute(ControllerModel controller)
    {
        if (controller.Selectors.Any(s => s.AttributeRouteModel is not null))
        {
            return;
        }

        var routeTemplate = $"{options.RoutePrefix}/{ToKebabCase(controller.ControllerName)}";

        foreach (var selector in controller.Selectors)
        {
            selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(routeTemplate));
        }
    }

    private static void ConfigureAction(ActionModel action)
    {
        if (action.Selectors.Any(s => s.ActionConstraints.OfType<HttpMethodActionConstraint>().Any()))
        {
            return;
        }

        var httpMethod = ResolveHttpMethod(action.ActionMethod);

        foreach (var selector in action.Selectors)
        {
            selector.ActionConstraints.Add(new HttpMethodActionConstraint([httpMethod]));
        }

        ConfigureActionRoute(action, httpMethod);
    }

    private static void ConfigureActionRoute(ActionModel action, string httpMethod)
    {
        if (action.Selectors.Any(s => s.AttributeRouteModel is not null))
        {
            return;
        }

        var actionName = NormalizeActionName(action.ActionMethod.Name);

        if (string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase) &&
            action.Parameters.Any(p => string.Equals(p.ParameterName, "id", StringComparison.OrdinalIgnoreCase)))
        {
            actionName = "{id}";
        }
        else if (!string.IsNullOrEmpty(actionName))
        {
            actionName = ToKebabCase(actionName);
        }

        if (!string.IsNullOrEmpty(actionName))
        {
            foreach (var selector in action.Selectors)
            {
                selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(actionName));
            }
        }
    }

    private static string ResolveHttpMethod(MethodInfo method)
    {
        foreach (var (prefix, httpMethod) in HttpMethodPrefixes)
        {
            if (method.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return httpMethod;
            }
        }

        return "POST";
    }

    private static string NormalizeActionName(string methodName)
    {
        foreach (var prefix in HttpMethodPrefixes.Keys)
        {
            if (methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                methodName = methodName[prefix.Length..];
                break;
            }
        }

        if (methodName.EndsWith("Async", StringComparison.Ordinal))
        {
            methodName = methodName[..^"Async".Length];
        }

        return methodName;
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var builder = new System.Text.StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
