using System.Reflection;

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
                var val = (Dictionary<string, object>)output.Value;
                if (val["value"] is Dictionary<string, object> fieldProperties)
                {
                    var complexObject = Activator.CreateInstance(field.FieldType);
                    foreach (var property in fieldProperties)
                    {
                        var complexProperty =
                            field.FieldType.GetProperty(property.Key, BindingFlags.Instance | BindingFlags.Public);
                        if (complexProperty != null) complexProperty.SetValue(complexObject, property.Value);
                    }

                    field.SetValue(this, complexObject);
                }
                else
                {
                    field.SetValue(this, val["value"]);
                }
            }
        }
    }
}