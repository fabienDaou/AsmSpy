using AsmSpy.Core;

namespace AsmSpy.Filters
{
    public interface IFilter
    {
        IDependencyAnalyzerResult Filter(IDependencyAnalyzerResult result);
    }
}
