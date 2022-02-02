
using BicepRunner;


public enum weatherTypeOptions {
    rain,
    hail,
}


public class SingleParam : BicepTemplate<SingleParam.SingleParamOutput> {
    public override string FileName => "single-param.bicep";
    public override string FileHash => "wiYzY1I4ws2TGeLFA8/M1KcNb+iGxRtLpfdWV9O+PAZXkyjrHsB6Ay4Ywe3V++cwsk7c71/PnK+qNM0QepjBTQ==";


    private string _name = default!;
    public string Name { get { return this._name; } set { this._name = value; } }


    private BicepTestTypes.SampleComplexObject _complex = default!;
    public BicepTestTypes.SampleComplexObject Complex { get { return this._complex; } set { this._complex = value; } }


    private BicepTestTypes.SampleComplexObject[] _names = default!;
    public BicepTestTypes.SampleComplexObject[] Names { get { return this._names; } set { this._names = value; } }


    private System.Array _names2 = default!;
    public System.Array Names2 { get { return this._names2; } set { this._names2 = value; } }


    private weatherTypeOptions _weatherType = default!;
    public weatherTypeOptions Weathertype { get { return this._weatherType; } set { this._weatherType = value; } }


    public class SingleParamOutput : BicepOutput {
        

        private string _nameout = default!;
        public string Nameout { get { return this._nameout; } set { this._nameout = value; } }


        private BicepTestTypes.SampleComplexObjectOutput _strongtype = default!;
        public BicepTestTypes.SampleComplexObjectOutput Strongtype { get { return this._strongtype; } set { this._strongtype = value; } }

        public SingleParamOutput(Dictionary<string, object> outputs) {
            base.SetProperties(outputs);
        }
    }

    public override Dictionary<string, object> BuildParameters() {
        var dictionary = new Dictionary<string, object>();
        dictionary["name"] = new { value = this._name};
        dictionary["complex"] = new { value = this._complex};
        dictionary["names"] = new { value = this._names};
        dictionary["names2"] = new { value = this._names2};
        dictionary["weatherType"] = new { value = this._weatherType};
        return dictionary;
    } 

    public override SingleParamOutput BuildOutput(Dictionary<string, object> outputs) {
        return new SingleParamOutput(outputs);
    } 
}