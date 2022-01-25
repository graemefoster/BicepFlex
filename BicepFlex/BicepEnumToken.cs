namespace BicepFlex;

public class BicepEnumToken : BicepParameterToken
{
    public static bool TryParse(IEnumerator<string> reader, out BicepToken? token)
    {
        var line = reader.Current;
        if (line.Contains("@allowed(["))
        {

            if (BicepParameterToken.TryParse(reader, out var parameterToken))
            {
                //grab the values up to the parameter part
                token = new BicepEnumToken(parameterToken.Name, parameterToken.BicepType, parameterToken.CustomType);
                return true;
            }

            //Shouldn't happen
            throw new InvalidOperationException("Detected parameter token but failed to read");
        }

        token = null;
        return false;
    }

    public BicepEnumToken(string name, string bicepType, string? customType) : base(name, bicepType, customType)
    {
    }
}