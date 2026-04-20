namespace ChengYuan.Authorization;

public interface IPermissionDefinitionContributor
{
    void Define(IPermissionDefinitionContext context);
}
