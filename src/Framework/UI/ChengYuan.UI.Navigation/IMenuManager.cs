namespace ChengYuan.Core.UI.Navigation;

public interface IMenuManager
{
    Task<ApplicationMenu> GetAsync(string menuName);
}
