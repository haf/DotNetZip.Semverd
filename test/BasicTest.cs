using Xunit;

namespace Ionic.Zip.Tests
{
    public class BasicTest
    {
        [Fact]
        public void TestCreate()
        {
            var result = new ZipFile();
            Assert.NotNull(result);
        }
    }
}
