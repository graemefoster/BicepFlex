// See https://aka.ms/new-console-template for more information

using BicepRunner.Samples;
using Newtonsoft.Json;

var runner = new AzBicepRunner("testy", @"C:\code\github\graemefoster\BicepFlex\TestBicepFiles\");

var stack = new Stack()
{
    ComplexOne =  new SampleComplexObject()
    {
        Property1 = "Hello World!",
        Property2 = 78
    },
    Two = "HELLO"
};

var bicepFile = new SingleParam();
bicepFile.Name = "Graeme";
bicepFile.Complex = stack.ComplexOne;

var output =
    await runner
        .ExecuteTemplate(bicepFile, o => o)
        .ThenDeploy(o => new SingleParam()
        {
            Name = o.Nameout,
            Complex = new SampleComplexObject()
            {
                Property1 = "ASASASs",
                Property2 = 123
            }
        }, o => o);

Console.WriteLine(JsonConvert.SerializeObject(output.Output));
