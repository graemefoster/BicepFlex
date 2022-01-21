// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;

var runner = new AzBicepRunner("testy", @"C:\code\github\graemefoster\BicepFlex\TestBicepFiles\");
var bicepFile = new SingleParam();
bicepFile.Name = "Graeme";
var output = await runner.ExecuteTemplate(bicepFile);

Console.WriteLine(JsonConvert.SerializeObject(output));
