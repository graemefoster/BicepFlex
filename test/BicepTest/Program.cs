// See https://aka.ms/new-console-template for more information

using BicepRunner;
using BicepTestTypes;
using Newtonsoft.Json;

var bicepFlex = new BicepFlex.BicepFlex(
    "./TestBicepFiles", 
    "../../../../TestBicepOutput/",
    typeof(Stack).Assembly.Location);

await bicepFlex.Process();



var runner = new AzBicepRunner.AzBicepRunner(
    "testy", 
    @"C:\code\github\graemefoster\BicepFlex\TestBicepFiles\");

var stack = new Stack()
{
    ComplexOne = new SampleComplexObject
    {
        Property1 = "Hello World!",
        Property2 = 78
    },
    Two = "HELLO"
};

var bicepFile = new SingleParam
{
    Name = "Graeme",
    Complex = stack.ComplexOne,
    Names = Array.Empty<SampleComplexObject>(),
    Names2 = Array.Empty<object>()
};

var output =
    await runner
        .ExecuteTemplate(
            bicepFile,
            o => o)
        .ThenDeploy(o => new SingleParam
        {
            Name = o.Nameout,
            Weathertype = weatherTypeOptions.hail,
            Complex = new SampleComplexObject
            {
                Property1 = "ASASASs",
                Property2 = 123
            }
        });

Console.WriteLine(JsonConvert.SerializeObject(output));

