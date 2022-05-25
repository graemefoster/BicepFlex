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
                var propertyValue = val.Deserialize(field.FieldType);
                field.SetValue(this, propertyValue);
            }
        }
    }
}