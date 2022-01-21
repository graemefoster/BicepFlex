namespace BicepRunner.Samples;

public class SampleComplexObject
{
    public string? Property1 { get; set; }
    public int? Property2 { get; set; }
}

public class SampleComplexObjectOutput
{
    public string id { get; set; }
    public string? complexProperty1 { get; set; }
    public int? complexProperty2 { get; set; }
}