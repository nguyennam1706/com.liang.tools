using NUnit.Framework;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace LiangTools.Editor.Tests
{
    public class PackageManifestTests
    {
        [Test]
        public void PackageIsResolved()
        {
            var info = PackageInfo.FindForAssembly(typeof(LiangToolsWindow).Assembly);
            Assert.IsNotNull(info);
            Assert.AreEqual(LiangToolsInfo.PackageName, info.name);
        }
    }
}
