# BicepFlex

## What is it?
A layer over Bicep to allow you to orchestrate templates using .Net 6.

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

var bicepFile = new SingleParamFile
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
            o => o,
            bicepFile,
            o => o)
        .ThenDeploy(o => new SingleParam
        {
            Name = o.Item1.Nameout,
            Weathertype = weatherTypeOptions.hail,
            Complex = new SampleComplexObject
            {
                Property1 = "ASASASs",
                Property2 = 123
            }
        });

var nextTest = new TestModule();
var foo = nextTest.Bar;

```
