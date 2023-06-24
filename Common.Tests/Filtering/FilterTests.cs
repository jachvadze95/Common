using Microsoft.EntityFrameworkCore;

namespace Common.Tests.Filtering
{
    [TestClass]
    public class UnitTest1
    {
        // to have the same Configuration object as in Startup
        private IConfigurationRoot _configuration;

        // represents database's configuration
        private DbContextOptions<CarServiceContext> _options;

        public UnitTest1()
        {
            
        }

        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}