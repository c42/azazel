using NUnit.Framework;

namespace Venus.Browser {
    [TestFixture]
    public class InternetExplorerTest {
        private const string urlFileContents =
            @"[DEFAULT]
BASEURL=http://www.google.co.in/firefox
[InternetShortcut]
URL=http://www.google.co.in/firefox
IDList=
IconFile=http://www.google.co.in/favicon.ico
IconIndex=1
[{000214A0-0000-0000-C000-000000000046}]
Prop3=19,2
";

        [Test]
        public void ReadsAFavourite() {
            var bookmark = InternetExplorer.CreateBookmark("Mozilla Firefox Start Page", urlFileContents);
            Assert.AreEqual("http://www.google.co.in/firefox", bookmark.FullName);
            Assert.AreEqual("Mozilla Firefox Start Page", bookmark.Name);
        }
    }
}