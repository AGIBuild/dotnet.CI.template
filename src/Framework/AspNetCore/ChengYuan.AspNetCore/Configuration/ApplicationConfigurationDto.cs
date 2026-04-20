using System.Collections.Generic;
using ChengYuan.Core.UI.Navigation;

namespace ChengYuan.AspNetCore.Configuration;

public sealed class ApplicationConfigurationDto
{
    public CurrentUserDto CurrentUser { get; init; } = new();

    public ApplicationMenu? Menu { get; init; }

    public IReadOnlyList<string> GrantedPermissions { get; init; } = [];
}
