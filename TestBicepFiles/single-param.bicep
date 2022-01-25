param name string
param complex object //@bicepflextype BicepRunner.Samples.SampleComplexObject
param names array //@bicepflextype BicepRunner.Samples.SampleComplexObject
param names2 array

@allowed([
  'rain'
  'hail'
])
param weatherType string

var foo = complex
var too = foo
var yoo = 'complex'
var moo = complex.Property1 + "Hello"

module funkyFoo './test-module.bicep' = {
  params: {
    bar: too
  }
}

output nameout string = name

output strongtype object = { //@bicepflextype BicepRunner.Samples.SampleComplexObjectOutput
  id: name
  complexProperty1: complex.Property1
  complexProperty2: complex.Property2
}
