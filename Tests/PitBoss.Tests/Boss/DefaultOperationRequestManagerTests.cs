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
    public class DefaultOperationRequestManagerTests : IDisposable
    {
        MemoryDistributedService memoryService;
        InMemoryContextFactory contextFactory;
        LoggerFactory loggerFactory;

        public DefaultOperationRequestManagerTests()
        {
            contextFactory = new InMemoryContextFactory("DefaultOperationRequestManagerTests");
            memoryService = new MemoryDistributedService();
            loggerFactory = new LoggerFactory();
        }

        public void Dispose()
        {
            var context = contextFactory.GetContext();
            context.Database.EnsureDeleted();
            memoryService.CleanCache();
        }

        public DefaultOperationRequestManager GetManager(List<PipelineStep> steps = null)
        {
            var pipelineManager = new Mock<IPipelineManager>();
            var configuration = new Mock<IConfiguration>();
            var distributedRequestManager = new Mock<IDistributedRequestManager>();
            var testStep = new TestStep<int, int>("test");
            testStep.SetId("test");
            var stepList = new List<PipelineStep>();
            if(steps != null)
            {
                stepList.AddRange(steps);
            }
            else
            {
                stepList.Add(testStep);
            }
            var testPipeline = new Pipeline(new PipelineDescriptor{ Name = "test" }, stepList);
            pipelineManager.Setup(x => x.Pipelines).Returns(new List<Pipeline>(){
               testPipeline 
            });

            pipelineManager.Setup(x => x.GetPipeline("test")).Returns(testPipeline);
            var requestManager = new DefaultOperationRequestManager(
                memoryService,
                contextFactory,
                pipelineManager.Object,
                configuration.Object,
                distributedRequestManager.Object,
                new Logger<IOperationRequestManager>(loggerFactory)
            );
            return requestManager;
        }

        [Fact]
        public void QueueValidRequest()
        {
        //Given
            var requestManager = GetManager();

            var request = new OperationRequest()
            {
                Id = "test",
                PipelineName = "test",
                PipelineStepId = "test"
            };

            requestManager.QueueRequest(request);
        }

        [Fact]
        public void QueueInvalidRequest()
        {
            var requestManager = GetManager();

            var request = new OperationRequest()
            {
                Id = "test",
                PipelineName = "invalid-test",
                PipelineStepId = "test"
            };

            Assert.Throws(typeof(KeyNotFoundException), () => requestManager.QueueRequest(request));
        }

        [Fact]
        public void QueueNullRequest()
        {
            var requestManager = GetManager();

            OperationRequest request = null;
            
            Assert.Throws(typeof(NullReferenceException), () => requestManager.QueueRequest(request));
        }

        [Fact]
        public void ProcessValidResponse()
        {
            var multipleStepList = new List<TestStep<int,int>>()
            {
                new TestStep<int, int>("test"),
                new TestStep<int, int>("test1")
            };
            multipleStepList.ForEach(x => x.SetId(x.Name));
            multipleStepList.First().NextSteps.Add("test1");
            var requestManager = GetManager(multipleStepList.Cast<PipelineStep>().ToList());
            var context = contextFactory.GetContext();
            var pipeRequest = new PipelineRequest
            {
                Id = "test",
                PipelineName = "test",
                Status = RequestStatus.Executing
            };
            context.PipelineRequests.Add(pipeRequest);

            OperationRequest request = new OperationRequest
            {
                Id = "test",
                PipelineId = "test",
                PipelineName = "test",
                PipelineStepId = "test"
            };
            context.OperationRequests.Add(request);
            context.SaveChanges();

            OperationResponse response = new OperationResponse<int>()
            {
                Id = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                PipelineName = "test",
                Success = true,
                Result = 0
            };

            var continueProcess = requestManager.ProcessResponse(response);
            Assert.False(continueProcess);
            requestManager = GetManager();
            continueProcess = requestManager.ProcessResponse(response);
            Assert.True(continueProcess);
        }

        [Fact]
        public void ProcessFailedResponse()
        {
            var context = contextFactory.GetContext();
            var pipeRequest = new PipelineRequest
            {
                Id = "test",
                PipelineName = "test",
                Status = RequestStatus.Executing
            };
            context.PipelineRequests.Add(pipeRequest);

            OperationRequest request = new OperationRequest
            {
                Id = "test",
                PipelineId = "test",
                PipelineName = "test",
                PipelineStepId = "test"
            };
            context.OperationRequests.Add(request);
            context.SaveChanges();

            OperationResponse response = new OperationResponse<int>()
            {
                Id = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                PipelineName = "test",
                Success = false,
                Result = 0
            };
            var requestManager = GetManager();
            Assert.False(requestManager.ProcessResponse(response));
        }

        [Fact]
        public void ProcessNonPipelineResponse()
        {
            OperationResponse response = new OperationResponse<int>()
            {
                Id = "non-test",
                PipelineId = "non-test",
                PipelineStepId = "non-test",
                PipelineName = "non-test",
                Success = true,
                Result = 0
            };
            var requestManager = GetManager();
            Assert.Throws(typeof(KeyNotFoundException), () => requestManager.ProcessResponse(response));
        }

        [Fact]
        public void ProcessNullResponse()
        {
            OperationResponse response = null;
            var requestManager = GetManager();
            Assert.Throws(typeof(NullReferenceException), () => requestManager.ProcessResponse(response));
        }

        [Fact]
        public void FetchNextRequest()
        {
            var step = new TestStep<int, int>("test");
            step.SetId("test");
            var requestManager = GetManager(new List<PipelineStep>{ step });
            var queue = memoryService.GetQueue<OperationRequest<int>>($"{DefaultOperationRequestManager.CachePrefix}:test"); 
            var request = new OperationRequest<int>
            {
                Id = "test",
                PipelineName = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                Parameter = 1
            };
            queue.Push(request);
            var newRequest = requestManager.FetchNextRequest(step);
            Assert.Equal(JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(newRequest));
        }

        [Fact]
        public void FetchNextRequestEmpty()
        {
            var step = new TestStep<int, int>("test");
            step.SetId("test");
            var requestManager = GetManager(new List<PipelineStep>{ step });
            Assert.Null(requestManager.FetchNextRequest(step));
        }

        [Fact]
        public void FetchNullRequest()
        {
            var requestManager = GetManager();
            Assert.Throws(typeof(NullReferenceException), () => requestManager.FetchNextRequest(null));
        }

        [Fact]
        public void ReturnRequest()
        {
            var request = new OperationRequest<int>
            {
                PipelineName = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                Parameter = 1
            };
            var manager = GetManager();
            manager.ReturnRequest(request);
            var queue = memoryService.GetQueue<OperationRequest<int>>($"{DefaultOperationRequestManager.CachePrefix}:test");
            var newRequest = queue.Pop();
            Assert.Equal(JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(newRequest));
        }

        [Fact]
        public void ReturnNullRequest()
        {
            var manager = GetManager();
            Assert.Throws(typeof(NullReferenceException), () => manager.ReturnRequest(null));
        }
    }
}