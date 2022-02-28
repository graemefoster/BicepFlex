// See https://aka.ms/new-console-template for more information

using BicepRunner;
using BicepTestTypes;
using Newtonsoft.Json;

var bicepFlex = new BicepFlex.BicepFlex(
    "./TestBicepFiles/",
    "../../../BicepFlexOutput/",
    typeof(Stack).Assembly.Location);

await bicepFlex.Process();


var runner = new AzBicepRunner.AzBicepRunner(
    "testy",
    @"./TestBicepFiles/");

var stack = new Stack()
{
    ComplexOne = new SampleComplexObject
    {
        Property1 = "Hello World!",
        Property2 = 78
    },
    Two = "HELLO"
};

var bicepFile = new SingleParamModule()
{
    Name = "Graeme",
    Complex = stack.ComplexOne,
    Names = new[]
    {
        new SampleComplexObject() { Property1 = "HELLO", Property2 = 38 },
        new SampleComplexObject() { Property1 = "HELLO2", Property2 = 39 }
    },
    Names2 = Array.Empty<object>(),
    Weathertype = SingleParamWeathertypeOptions.hail
};

var output =
    await runner
        .ExecuteTemplate(bicepFile)
        .ThenDeploy(o => new SingleParamModule
        {
            Name = o.Nameout,
            Weathertype = SingleParamWeathertypeOptions.hail,
            Names = Array.Empty<SampleComplexObject>(),
            Names2 = Array.Empty<object>(),
            Complex = new SampleComplexObject
            {
                Property1 = "ASASASs",
                Property2 = 123
            }
        });

Console.WriteLine(JsonConvert.SerializeObject(output.Output));
