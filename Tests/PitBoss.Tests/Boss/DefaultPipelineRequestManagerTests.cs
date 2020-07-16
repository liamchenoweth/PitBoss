using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Newtonsoft.Json;

namespace PitBoss.Tests
{
    public class DefaultPipelineRequestManagerTests : IDisposable
    {
        InMemoryContextFactory contextFactory;
        MemoryDistributedService memoryService;

        public DefaultPipelineRequestManagerTests()
        {
            memoryService = new MemoryDistributedService();
            contextFactory = new InMemoryContextFactory("DefaultPipelineRequestManagerTests");
        }

        public void Dispose()
        {
            var context = contextFactory.GetContext();
            context.Database.EnsureDeleted();
            memoryService.CleanCache();
        }

        public DefaultPipelineRequestManager GetManager()
        {
            var pipelineManager = new Mock<IPipelineManager>();
            var operationRequestManager = new Mock<IOperationRequestManager>();
            var testStep = new TestStep<int, int>("test");
            testStep.SetId("test");
            var stepList = new List<PipelineStep>(){testStep};
            // if(steps != null)
            // {
            //     stepList.AddRange(steps);
            // }
            // else
            // {
            //     stepList.Add(testStep);
            // }
            var testPipeline = new Pipeline(new PipelineDescriptor{ Name = "test" }, stepList);
            pipelineManager.Setup(x => x.Pipelines).Returns(new List<Pipeline>(){
               testPipeline 
            });

            pipelineManager.Setup(x => x.GetPipeline("test")).Returns(testPipeline);
            return new DefaultPipelineRequestManager
            (
                memoryService,
                contextFactory,
                pipelineManager.Object,
                operationRequestManager.Object
            );
        }

        [Fact]
        public void QueueRequest()
        {
            var manager = GetManager();
            var request = new PipelineRequest
            {
                Id = "test",
                PipelineName = "test",
                Input = "1"
            };
            manager.QueueRequest(request);
            Assert.True(contextFactory.GetContext().PipelineRequests.Any());
        }

        [Fact]
        public void QueueNullRequest()
        {
            var manager = GetManager();
            Assert.Throws(typeof(NullReferenceException), () => manager.QueueRequest(null));
        }

        [Fact]
        public void FinishRequest()
        {
            var manager = GetManager();
            var request = new PipelineRequest
            {
                Id = "test",
                PipelineName = "test",
                Input = "1"
            };
            var context = contextFactory.GetContext();
            context.PipelineRequests.Add(request);
            context.SaveChanges();
            var response = new OperationResponse
            {
                Id = "test",
                PipelineName = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                Success = true
            };
            manager.FinishRequest(response);
            Assert.True(contextFactory.GetContext().PipelineRequests.First().Status == RequestStatus.Complete);
        }

        [Fact]
        public void FinishBadRequest()
        {
            var manager = GetManager();
            var response = new OperationResponse
            {
                Id = "test",
                PipelineName = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                Success = true
            };
            Assert.Throws(typeof(KeyNotFoundException), () => manager.FinishRequest(response));
        }

        [Fact]
        public void FinishNullRequest()
        {
            var manager = GetManager();
            Assert.Throws(typeof(NullReferenceException), () => manager.FinishRequest(null));
        }
    }
}