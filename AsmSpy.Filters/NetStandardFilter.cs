using System.Collections.Generic;
using System.Linq;
using AsmSpy.Core;

namespace AsmSpy.Filters
{
    /// All netstandard references use version 0.0.0.0.
    public class NetStandardFilter : IFilter
    {
        public IDependencyAnalyzerResult Filter(IDependencyAnalyzerResult result)
        {
            var filteredResult = new Dictionary<string, AssemblyReferenceInfo>(result.Assemblies.Count);

            foreach (var reference in result.Assemblies)
            {
                if (reference.Value.ReferencedBy.ToList().Exists(e => e.AssemblyName.Name == "netstandard"))
                {
                    var filteredReference = new AssemblyReferenceInfo(reference.Value.AssemblyName);

                    reference.Value.References
                        .ToList()
                        .ForEach(filteredReference.AddReference);

                    reference.Value.ReferencedBy
                        .Where(referencedBy => referencedBy.AssemblyName.Name != "netstandard")
                        .ToList()
                        .ForEach(filteredReference.AddReferencedBy);
                }
                else
                {
                    filteredResult.Add(reference.Key, reference.Value);
                }
            }

            return new DependencyAnalyzerResult(result.AnalyzedFiles, filteredResult);
        }
    }
}