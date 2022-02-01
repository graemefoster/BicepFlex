var bicepPath = args[0];
var bicepOutputPath = args[1];
var referenceTypesAssembly = "/Users/graemefoster1/code/github/graemefoster/BicepFlex/BicepRunner/bin/Debug/netcoreapp3.1/BicepRunner.dll";

var processor = new BicepFlex.BicepFlex(bicepPath, bicepOutputPath, referenceTypesAssembly);
await processor.Process();
