using System.Reflection;

namespace BicepParser;

class BicepFileSetPostProcessor
{
    public void PostProcess(string bicepRoot, string? referenceTypesAssembly, BicepMetaFile[] allMetaFiles)
    {
        //Add parents to the dependency tree
        foreach (var file in allMetaFiles)
        {
            file.InferTree(bicepRoot, allMetaFiles);
        }

        //Try to infer types
        var referenceTypeAssembly = referenceTypesAssembly == null ? null : Assembly.LoadFile(referenceTypesAssembly);
        var keepGoing = true;
        while (keepGoing)
        {
            var madeInferences = false;
            foreach (var file in allMetaFiles)
            {
                if (file.InferTypes(bicepRoot, allMetaFiles, referenceTypeAssembly)) madeInferences = true;
            }

            //If we made some inferences, do another pass to see if we can make some more
            keepGoing = madeInferences;
        }
    }
}