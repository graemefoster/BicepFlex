var bicepPath = args[0];
var bicepOutputPath = args[1];
var referenceTypesAssembly = args.Length > 2 ? args[2] : null;

var processor = new BicepFlex.BicepFlex(bicepPath, bicepOutputPath, referenceTypesAssembly);
await processor.Process();
