using System;
using System.Collections.Generic;
using System.IO;
using Azazel.PluggingIn;

namespace Azazel.FileSystem {
    public class Folders : List<Folder>, LaunchablePlugin {
        internal static readonly Folder QuickLaunch = new Folder(Constants.QuickLaunch);
        internal static readonly Folder StartMenu = new Folder(Constants.StartMenu);
        internal static readonly Folder AllUsersStartMenu = new Folder(Constants.AllUsersStartMenu);
        private DateTime endOfPenaltyTime = DateTime.MinValue;
        private readonly TimeSpan penaltyPeriod = TimeSpan.FromMinutes(10);

        public Folders(params Folder[] folders) : this((IEnumerable<Folder>) folders) {}

        private Folders(IEnumerable<Folder> folders) {
            AddRange(folders);
            foreach (var folder in folders) WatchFolder(folder);
        }

        private void WatchFolder(Folder folder) {
            new FileSystemStalker(folder, FileChangeTypes.Created | FileChangeTypes.Deleted | FileChangeTypes.Renamed, OnFilChanged);
        }

        private void OnFilChanged(File file) {
            if (file.Exists() && !file.IsHidden && DateTime.Now > endOfPenaltyTime) {
                endOfPenaltyTime = DateTime.Now.Add(penaltyPeriod);
                Changed(this);
            }
        }

        public new void Add(Folder folder) {
            if (Contains(folder)) return;
            base.Add(folder);
            WatchFolder(folder);
        }

        public Folders(IEnumerable<DirectoryInfo> directoryInfos) {
            foreach (var directoryInfo in directoryInfos) Add(new Folder(directoryInfo));
        }

        public Launchables Launchables() {
            var files = new Files();
            var copyOfFoldersToIterate = new Folders(this);
            foreach (var folder in copyOfFoldersToIterate) {
                if (!Folder.Exists(folder.FullName)) Remove(folder);
                files.AddRange(folder.GetFiles());
            }
            return files;
        }

        public event PluginChangedDelegate Changed = delegate { };

        public bool IsAvailable {
            get { return true; }
        }
    }
}