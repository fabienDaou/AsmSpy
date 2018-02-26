using Task = Microsoft.Build.Utilities.Task;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AsmSpy.Core;
using AsmSpy.Filters;

namespace AsmSpy.MSTask
{
    public class DetectNotFoundAssembliesTask : Task
    {
        private ILogger logger;

        public string Directory { get; set; }
        public bool IgnoreSubDirectories { get; set; }

        public bool NotFoundOnly { get; set; }
        public bool IgnoreNetStandard { get; set; }
        public bool IgnoreSystem { get; set; }

        public override bool Execute()
        {
            logger = new MsBuildLogger(Log);

            var directoryInfo = new DirectoryInfo(Directory);
            logger.LogMessage(string.Format(CultureInfo.InvariantCulture, "Checking assemblies in: {0}", directoryInfo));

            var fileList = GetFilesToAnalyze(directoryInfo);

            IDependencyAnalyzerResult analyzerResult = new DependencyAnalyzer(fileList).Analyze(logger);
            var filteredAnalyzerResult = ApplyFilters(analyzerResult);

            if (HasNotFoundAssembly(filteredAnalyzerResult))
            {
                logger.LogError($"Some dll references could not be found. Run \"AsmSpy {Directory} -s -i -n\", and check the red entries for more info.");
                return false;
            }

            return true;
        }

        private IEnumerable<FileInfo> GetFilesToAnalyze(DirectoryInfo directoryInfo)
        {
            var configuredSearchOption = IgnoreSubDirectories ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
            return directoryInfo
                .GetFiles("*.dll", configuredSearchOption)
                .Concat(directoryInfo.GetFiles("*.exe", configuredSearchOption));
        }

        private IDependencyAnalyzerResult ApplyFilters(IDependencyAnalyzerResult result)
        {
            var filters = new List<IFilter>();
            if (IgnoreNetStandard) filters.Add(new NetStandardFilter());
            if (NotFoundOnly) filters.Add(new NotFoundOnlyFilter());

            filters.ForEach(f => result = f.Filter(result));
            return result;
        }

        private bool HasNotFoundAssembly(IDependencyAnalyzerResult result)
        {
            var dllNotFound = false;

            var orderedAssemblyGroups = result.Assemblies.Values
                .GroupBy(ari => ari.AssemblyName.Name)
                .OrderBy(g => g.Key);

            foreach (IGrouping<string, AssemblyReferenceInfo> assemblyGroup in orderedAssemblyGroups)
            {
                if (!ShouldAssemblyBeIgnored(assemblyGroup.Key))
                {
                    List<AssemblyReferenceInfo> assemblyInfos =
                        assemblyGroup.OrderBy(x => x.AssemblyName.ToString()).ToList();
                    if (assemblyInfos.Count <= 1)
                    {
                        if (assemblyInfos.Count == 1 && assemblyInfos[0].AssemblySource == AssemblySource.Local)
                        {
                            continue;
                        }

                        if (assemblyInfos.Count <= 0)
                        {
                            continue;
                        }
                    }

                    logger.LogWarning($"Reference \"{assemblyGroup.Key}\" could not be resolved. ");
                    dllNotFound = true;
                }
            }

            return dllNotFound;
        }

        private bool ShouldAssemblyBeIgnored(string assemblyName) => IgnoreSystem && (LooksLikeMscorlib(assemblyName) || LooksLikeSystem(assemblyName));

        private static bool LooksLikeSystem(string assemblyName) => Compare(assemblyName, "SYSTEM");

        private static bool LooksLikeMscorlib(string assemblyName) => Compare(assemblyName, "MSCORLIB");

        private static bool Compare(string toCompare, string comparedTo)
        {
            return toCompare.ToUpperInvariant().StartsWith(comparedTo, StringComparison.OrdinalIgnoreCase);
        }

        //var directory = commandLineApplication.Argument("directory", "The directory to search for assemblies");
        //var dgmlExport = commandLineApplication.Option("-dg|--dgml <filename>", "Export to a dgml file", CommandOptionType.SingleValue);
        //var nonsystem = commandLineApplication.Option("-n|--nonsystem", "Ignore 'System' assemblies", CommandOptionType.NoValue);
        //var all = commandLineApplication.Option("-a|--all", "List all assemblies and references.", CommandOptionType.NoValue);
        //var noconsole = commandLineApplication.Option("-nc|--noconsole", "Do not show references on console.", CommandOptionType.NoValue);
        //var silent = commandLineApplication.Option("-s|--silent", "Do not show any message, only warnings and errors will be shown.", CommandOptionType.NoValue);
        //var bindingRedirect = commandLineApplication.Option("-b|--bindingredirect", "Create binding-redirects", CommandOptionType.NoValue);
        //var referencedStartsWith = commandLineApplication.Option("-rsw|--referencedstartswith", "Referenced Assembly should start with <string>. Will only analyze assemblies if their referenced assemblies starts with the given value.", CommandOptionType.SingleValue);
        //var includeSubDirectories = commandLineApplication.Option("-i|--includesub", "Include subdirectories in search", CommandOptionType.NoValue);
    }
}
