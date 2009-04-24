using System;
using System.Collections.Generic;
using System.IO;
using Azazel.FileSystem;
using Action=Azazel.FileSystem.Action;
using File=Azazel.FileSystem.File;

namespace Azazel.PluggingIn {
    public abstract class FileHandler : LaunchableHandler {
        public abstract bool IsAvailable { get; }

        public bool Handles(Launchable launchable) {
            if (launchable is File)
                return Handles(((File) launchable).FileSystemInfo);
            return false;
        }

        public IEnumerable<Action> ActionsFor(Launchable launchable) {
            return ActionsFor(((File) launchable).FileSystemInfo);
        }

        protected abstract bool Handles(FileInfo fileInfo);
        protected abstract Actions ActionsFor(FileInfo fileInfo);
    }
}