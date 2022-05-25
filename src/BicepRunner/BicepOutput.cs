using System.Reflection;
using System.Text.Json;

namespace BicepRunner;

// ReSharper disable once ClassNeverInstantiated.Global
public class BicepOutput
{
    // ReSharper disable once UnusedMember.Global
    protected void SetProperties(Dictionary<string, object>? armOutputs)
    {
        if (armOutputs == null) return;

        foreach (var output in armOutputs)
        {
            var field = GetType().GetField($"_{output.Key}", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                var val = (JsonElement)output.Value;
                if (field.FieldType == typeof(BicepResource))
                {
                    var propertyValue = (string)val.Deserialize(typeof(string))!;
                    field.SetValue(this, new BicepResource(propertyValue));
                }
                else
                {
                    var propertyValue = val.Deserialize(field.FieldType);
                    field.SetValue(this, propertyValue);
                }
            }
        }
    }
}