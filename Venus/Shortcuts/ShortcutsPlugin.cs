using System;
using System.Windows.Media;
using System.Xml.Serialization;
using Azazel.FileSystem;
using Azazel.PluggingIn;

namespace Venus.Shortcuts {
    public class ShortcutsPlugin : LaunchablePlugin {
        public bool IsAvailable {
            get { return true; }
        }

        public Launchables Launchables() {
            var launchables = new Launchables();
            var specialFolders = new[]
                                     {
                                         Environment.SpecialFolder.Personal, Environment.SpecialFolder.MyMusic, Environment.SpecialFolder.Desktop,
                                         Environment.SpecialFolder.Recent, Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.MyPictures,
                                     };
            foreach (var specialFolder in specialFolders)
                if (Folder.Exists(Environment.GetFolderPath(specialFolder))) launchables.Add(new Shortcut(specialFolder));
            launchables.Add(new Shortcut("Add or Remove Programs", "appwiz.cpl"));
            launchables.Add(new Shortcut("Windows Explorer", "explorer.exe"));
            return launchables;
        }

        public event PluginChangedDelegate Changed = delegate { };

        private class Shortcut : Launchable {
            private readonly string name;
            private readonly string command;
            [XmlIgnore] private readonly ImageSource icon;
            private Shortcut() {}

            public Shortcut(Environment.SpecialFolder specialFolder) {
                var folder = new Folder(Environment.GetFolderPath(specialFolder));
                name = folder.Name;
                command = folder.FullName;
                icon = new PluginIconLoader().Png(specialFolder.ToString());
            }

            public Shortcut(string name, string command) : this() {
                this.name = name;
                this.command = command;
                icon = new PluginIconLoader().Png(name.Replace(" ", ""));
            }

            public string Name {
                get { return name; }
            }

            public ImageSource Icon {
                get { return icon; }
            }

            public Actions Actions {
                get { return new Actions(); }
            }

            public void Launch(string arguments) {
                new Runner(command, arguments).AsyncStart();
            }

            public bool ShouldStoreHistory {
                get { return true; }
            }

            private bool Equals(Shortcut obj) {
                return Equals(obj.name, name) && Equals(obj.command, command);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (Shortcut)) return false;
                return Equals((Shortcut) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return (name.GetHashCode()*397) ^ command.GetHashCode();
                }
            }

            public override string ToString() {
                return "Shortcut to " + Name;
            }
        }
    }
}