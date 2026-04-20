using System.Threading.Tasks;

namespace ChengYuan.Core.UI.Navigation;

public interface IMenuContributor
{
    Task ConfigureMenuAsync(MenuConfigurationContext context);
}
