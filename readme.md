# BicepFlex

## What is it?
A C# code generation layer over Bicep to allow you to orchestrate templates using .Net 6.

Provides simple type annotation using ``` //@bicepflextype BicepTestTypes.SampleComplexObject ``` next to object and array parameters and outputs.

## Sample

```bicep
param name string
param complex object //@bicepflextype BicepTestTypes.SampleComplexObject

@allowed([
  'rain'
  'hail'
])
param weatherType string

var foo = complex
var too = foo.Property1

module funkyFoo './test-module.bicep' = {
  name: 'funkyfoo'
  params: {
    bar: too
  }
}

output nameout string = name

output strongtype object = { //@bicepflextype BicepTestTypes.SampleComplexObjectOutput
  id: name
  complexProperty1: complex.Property1
  complexProperty2: complex.Property2
  weather: weatherType
}

```

```console
dotnet bicepflex <bicep-files-path> <bicep-flex-output-path> [<reference-assembly-dll>]
 ```

```c#
var stack = new Stack
{
    ComplexOne = new SampleComplexObject
    {
        Property1 = "Hello World!",
        Property2 = 78
    },
    Two = "HELLO"
};

var output =
    await runner
        
        //Deploy a bicep file
        .ExecuteTemplate(
            new SingleParamFile
            {
                Name = "Graeme",
                Weathertype = weatherTypeOptions.rain,
                Complex = stack.ComplexOne
            },
            //capture outputs and optionally transform them
            o => o)
        //Fluent API for chaining Bicep deployments together
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

```
