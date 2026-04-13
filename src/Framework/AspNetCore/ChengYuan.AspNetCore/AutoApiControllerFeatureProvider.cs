using System;
using System.Linq;
using System.Reflection;
using ChengYuan.Core.Application;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ChengYuan.AspNetCore;

internal sealed class AutoApiControllerFeatureProvider(AutoApiControllerOptions options) : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!typeof(IApplicationService).IsAssignableFrom(typeInfo) ||
            !typeInfo.IsClass ||
            typeInfo.IsAbstract ||
            typeInfo.IsGenericTypeDefinition ||
            !typeInfo.IsPublic)
        {
            return false;
        }

        if (options.TypePredicate is not null && !options.TypePredicate(typeInfo))
        {
            return false;
        }

        if (options.Assemblies.Count > 0 && !options.Assemblies.Contains(typeInfo.Assembly))
        {
            return false;
        }

        return true;
    }
}
