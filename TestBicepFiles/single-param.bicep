param name string
param complex object //@bicepflextype BicepRunner.Samples.SampleComplexObject
param names array //@bicepflextype BicepRunner.Samples.SampleComplexObject
param names2 array

output nameout string = name

output strongtype object = { //@bicepflextype BicepRunner.Samples.SampleComplexObjectOutput
  id: name
  complexProperty1: complex.Property1
  complexProperty2: complex.Property2
}
