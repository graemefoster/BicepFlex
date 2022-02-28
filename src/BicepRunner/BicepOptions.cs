namespace BicepRunner;

public abstract class BicepOptions
{
    public string BicepValue { get; }

    protected BicepOptions(string bicepValue)
    {
        BicepValue = bicepValue;
    }
}