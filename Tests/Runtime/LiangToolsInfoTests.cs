using NUnit.Framework;

namespace LiangTools.Tests
{
    public class LiangToolsInfoTests
    {
        [Test]
        public void PackageName_MatchesManifest()
        {
            Assert.AreEqual("com.liang.tools", LiangToolsInfo.PackageName);
        }
    }
}
