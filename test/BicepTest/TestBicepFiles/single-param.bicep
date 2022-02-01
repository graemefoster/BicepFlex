param name string
param complex object //@bicepflextype BicepTestTypes.SampleComplexObject
param names array //@bicepflextype BicepTestTypes.SampleComplexObject
param names2 array

@allowed([
  'rain'
  'hail'
])
param weatherType string

var foo = complex
var too = foo.Property1
var yoo = 'complex'
var moo = complex.Property1 + "Hello"

module funkyFoo './test-module.bicep' = {
  params: {
    bar: too
  }
}

output nameout string = name

output strongtype object = { //@bicepflextype BicepTestTypes.SampleComplexObjectOutput
  id: name
  complexProperty1: complex.Property1
  complexProperty2: complex.Property2
}
