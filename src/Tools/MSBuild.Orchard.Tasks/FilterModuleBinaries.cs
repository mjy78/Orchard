using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    public class FilterModuleBinaries : Task {
        public ITaskItem[] ModulesBinaries { get; set; }
        public ITaskItem[] OrchardWebBinaries { get; set; }

        [Output]
        public ITaskItem[] ExcludedBinaries { get; set; }

        public override bool Execute() {
            if (ModulesBinaries == null || OrchardWebBinaries == null)
                return true;

            // Exclude binaries that are included in the orchard web bin folder
            var orchardWebAssemblies = new HashSet<string>(
                OrchardWebBinaries.Select(item => Path.GetFileName(item.ItemSpec)),
                StringComparer.InvariantCultureIgnoreCase);

            // Also exclude binaries that are from other modules that have been copied local. 
            var sourceBinaries = ModulesBinaries
                .Where(item => Path.GetDirectoryName(item.ItemSpec).Contains(Path.GetFileNameWithoutExtension(item.ItemSpec)))
                .Select(item => Path.GetFileNameWithoutExtension(item.ItemSpec))
                .Distinct()
                .ToArray();

            ExcludedBinaries = ModulesBinaries
                .Where(item => orchardWebAssemblies.Contains(Path.GetFileName(item.ItemSpec)) || (sourceBinaries.Contains(Path.GetFileNameWithoutExtension(item.ItemSpec)) && !Path.GetDirectoryName(item.ItemSpec).Contains(Path.GetFileNameWithoutExtension(item.ItemSpec))))
                .Select(item => new TaskItem(item))
                .ToArray();

            return true;
        }
    }
}
