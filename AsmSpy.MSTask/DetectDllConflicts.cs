using Microsoft.Build.Framework;
using System;

namespace AsmSpy.MSTask
{
    public class DetectDllConflicts : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
