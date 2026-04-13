using System;
using System.Collections.Generic;
using System.Reflection;

namespace ChengYuan.AspNetCore;

public sealed class AutoApiControllerOptions
{
    public string RoutePrefix { get; set; } = "api/app";

    public IList<Assembly> Assemblies { get; } = [];

    public Func<Type, bool>? TypePredicate { get; set; }
}
