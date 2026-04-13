namespace ChengYuan.Core.GlobalFeatures;

public abstract class GlobalFeature
{
    public abstract string FeatureName { get; }

    public bool IsEnabled { get; internal set; }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
