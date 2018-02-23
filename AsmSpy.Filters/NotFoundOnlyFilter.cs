using System.Collections.Generic;
using System.Linq;
using AsmSpy.Core;

namespace AsmSpy.Filters
{
    /// Filtering out all GAC/Resolvable conflicts.
    public class NotFoundOnlyFilter : IFilter
    {
        public IDependencyAnalyzerResult Filter(IDependencyAnalyzerResult result)
        {
            var filteredResult = new Dictionary<string, AssemblyReferenceInfo>(result.Assemblies.Count);

            result.Assemblies
                .Where(r => r.Value.AssemblySource == AssemblySource.NotFound)
                .ToList()
                .ForEach(r => filteredResult.Add(r.Key, r.Value));

            return new DependencyAnalyzerResult(result.AnalyzedFiles, filteredResult);
        }
    }
}