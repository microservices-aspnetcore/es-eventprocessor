using Xunit;
using System.IO;
using StatlerWaldorfCorp.EventProcessor;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;

namespace StatlerWaldorfCorp.EventProcessor.Tests.Integration
{
    public class EventProcessingRoundTripTest
    {
        private readonly TestServer testServer;

        public EventProcessingRoundTripTest()
        {
                testServer = new TestServer(new WebHostBuilder()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>());
        }

        [Fact]
        public void TestPlaceholder()
        {            
            Assert.True(true);
        }
    }
}