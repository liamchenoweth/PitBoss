using System;
using System.Threading;
using System.Collections.Generic;

namespace PitBoss
{
    public class DefaultOperationHealthManager : IOperationHealthManager
    {
        private Dictionary<string, OperationRequestStatus> _requests;
        private string _activeId;
        private Dictionary<string, CancellationTokenSource> _cancelationSources;
        private bool _isShuttingDown = false;
        public DefaultOperationHealthManager()
        {
            _requests = new Dictionary<string, OperationRequestStatus>();
            _cancelationSources = new Dictionary<string, CancellationTokenSource>();
        }

        public bool Available
        {
            get
            {
                return true;
            }
        }

        public void RegisterRequest(OperationRequest request)
        {
            _requests[request.Id] = new OperationRequestStatus
            {
                RequestId = request.Id,
                Status = RequestStatus.Pending
            };
            _cancelationSources[request.Id] = new CancellationTokenSource();
        }

        public OperationRequestStatus GetOperationStatus(OperationRequest request)
        {
            if(_requests.TryGetValue(request.Id, out var status)) return status;
            return null;
        }

        public OperationRequestStatus GetOperationStatus(string request)
        {
            if(_requests.TryGetValue(request, out var status)) return status;
            return null;
        }

        public void SetActiveOperation(OperationRequest request)
        {
            if(!_requests.ContainsKey(request.Id)) RegisterRequest(request);
            _activeId = request.Id;
            _requests[_activeId].Status = RequestStatus.Executing;
        }

        public void FailActiveOperation(OperationRequest request, Exception e)
        {
            if(_activeId != request.Id) throw new Exception("Given request is not active");
            _requests[_activeId].Status = RequestStatus.Failed;
            _activeId = null;
        }

        public void FinishActiveOperation(OperationRequest request)
        {
            if(_activeId != request.Id) throw new Exception("Given request is not active");
            _requests[_activeId].Status = RequestStatus.Complete;
            _activeId = null;
        }

        public OperationRequestStatus GetCurrentActiveOperationStatus()
        {
            return _requests[_activeId];
        }

        public OperationStatus GetContainerStatus()
        {
            var status = ContainerStatus.None;
            if(string.IsNullOrEmpty(_activeId)) status = ContainerStatus.Ready;
            if(!string.IsNullOrEmpty(_activeId)) status = ContainerStatus.Processing;
            if(_isShuttingDown) status = ContainerStatus.ShuttingDown;
            return new OperationStatus
            {
                ContainerStatus = status
            };
        }

        public CancellationToken GetCancellationToken(OperationRequest request)
        {
            if(!_requests.ContainsKey(request.Id)) RegisterRequest(request);
            return _cancelationSources[request.Id].Token;
        }

        public void CancelRequest(OperationRequest request)
        {
            if(_cancelationSources.TryGetValue(request.Id, out var tokenSource))
            {
                tokenSource.Cancel();
                return;
            }
            throw new Exception("Operation could not be cancelled because it was not registered");
        }

        public void CancelActiveRequests()
        {
            if(!string.IsNullOrEmpty(_activeId))
            {
                _cancelationSources[_activeId].Cancel();
            }
        }

        public void SetContainerShutdown()
        {
            foreach(var pair in _cancelationSources)
            {
                pair.Value.Cancel();
            }
            _isShuttingDown = true;
        }
    }
}