// See https://aka.ms/new-console-template for more information

using BicepRunner.Samples;
using Newtonsoft.Json;

var runner = new AzBicepRunner("testy", @"C:\code\github\graemefoster\BicepFlex\TestBicepFiles\");
var bicepFile = new SingleParam();
bicepFile.Name = "Graeme";
bicepFile.Complex = new SampleComplexObject()
{
    Property1 = "Hello World!",
    Property2 = 78
};

var output = await runner.ExecuteTemplate(bicepFile);

Console.WriteLine(JsonConvert.SerializeObject(output));

