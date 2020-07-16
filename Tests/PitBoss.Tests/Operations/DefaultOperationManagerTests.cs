using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Newtonsoft.Json;

namespace PitBoss.Tests
{
    public class DefaultOperationManagerTests : IDisposable
    {
        public void Dispose()
        {

        }

        public DefaultOperationManager GetManager()
        {
            var clientFactory = new Mock<IHttpClientFactory>();
            var healthManager = new Mock<IOperationHealthManager>();
            var loggerFactory = new LoggerFactory();
            var logger = new Logger<DefaultOperationManager>(loggerFactory);
            return new DefaultOperationManager(
                clientFactory.Object,
                healthManager.Object,
                logger
            );
        }

        [Fact]
        public void QueueNullRequest()
        {
            var manager = GetManager();
            Assert.Throws(typeof(NullReferenceException), () => manager.QueueRequest(null));
        }

        [Fact]
        public void GetNextRequest()
        {
            var manager = GetManager();
            var request = new OperationRequest{};
            manager.QueueRequest(request);
            var newRequest = manager.GetNextRequest();
            Assert.Equal(JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(newRequest));
        } 

        
    }
}