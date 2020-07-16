using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Moq;
using PitBoss;

namespace PitBoss.Tests
{
    public class DefaultDistributedRequestManagerTests
    {
        [Fact]
        public void GenerateDistributedRequest()
        {
        //Given
            var pipelineManager = new Mock<IPipelineManager>();
            var contextFactory = new InMemoryContextFactory("DefaultDistributedRequestManagerTests");
            var requestManager = new DefaultDistributedRequestManager(
                pipelineManager.Object,
                contextFactory
            );
            var pipelineRequest = new PipelineRequest
            {
                Id = "test",
                PipelineName = "test",
                PipelineVersion = "test",
                PipelineModel = null,
                Status = RequestStatus.Executing,
                CurrentRequest = null,
                ResponseId = null,
                Response = null,
                Input = "test"
            };
            var operationRequest = new OperationRequest
            {
                Id = "test",
                PipelineName = "test",
                PipelineId = "test",
                PipelineStepId = "test",
                CallbackUri = "test",
                Status = RequestStatus.Complete,
                ParentRequestId = "test",
                InstigatingRequestId = "test",
                IsParentOperation = false,
                RetryCount = 0
            };
            var response = new OperationResponse<List<int>>(operationRequest)
            {
                Success = true,
                Result = new List<int>() { 1, 2, 3 }
            };
            var step = new PipelineStep<int, int>("test", null);
        //When
            var distributedRequest = requestManager.GenerateDistributedRequest(
                pipelineRequest,
                response,
                operationRequest,
                step
            );
        //Then
            Assert.Equal(distributedRequest.Count(), 3);
        }
    }
}