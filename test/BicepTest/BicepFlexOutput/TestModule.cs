
using BicepRunner;



public class TestModule : BicepTemplate<TestModule.TestModuleOutput> {
    public override string FileName => "test-module.bicep";
    public override string FileHash => "/IF2TbPynB2BJnGCoyF8gfClqSupT7xbBSd2XkYjZJuePmpKzvJjmr4Bn6CmnN5Tvz5O6zUpfrghRIGATFHiIw==";


    private System.String _bar = default!;
    public System.String Bar { get { return this._bar; } set { this._bar = value; } }


    public class TestModuleOutput : BicepOutput {
        

        private string _nameout = default!;
        public string Nameout { get { return this._nameout; } set { this._nameout = value; } }

        public TestModuleOutput(Dictionary<string, object> outputs) {
            base.SetProperties(outputs);
        }
    }

    public override Dictionary<string, object> BuildParameters() {
        var dictionary = new Dictionary<string, object>();
        dictionary["bar"] = new { value = this._bar};
        return dictionary;
    } 

    public override TestModuleOutput BuildOutput(Dictionary<string, object> outputs) {
        return new TestModuleOutput(outputs);
    } 
}
