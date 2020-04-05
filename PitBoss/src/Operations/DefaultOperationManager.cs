using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using PitBoss.Utils;

namespace PitBoss {
    public class DefaultOperationManager : IOperationManager
    {
        private Dictionary<string, OperationRequest> _previousRequests;
        private Delegate _operation;
        public Type InputType {get; private set;}
        public Type OutputType {get; private set;}
        private const string CompilationOutput = "compiled";
        private const string ResponseUri = "operation/result";
        private IHttpClientFactory _clientFactory;
        private IOperationHealthManager _healthManager;
        private List<OperationRequest> _queuedRequests;

        public DefaultOperationManager(IHttpClientFactory clientFactory, IOperationHealthManager healthManager)
        {
            _clientFactory = clientFactory;
            _previousRequests = new Dictionary<string, OperationRequest>();
            _healthManager = healthManager;
            _queuedRequests = new List<OperationRequest>();
        }

        public bool Ready {get; private set;}


        public void QueueRequest(OperationRequest request)
        {
            _healthManager.RegisterRequest(request);
            _queuedRequests.Add(request);
        }

        public OperationRequest GetNextRequest()
        {
            if(_queuedRequests.Count == 0) return null;
            var request = _queuedRequests.First();
            _queuedRequests.Remove(request);
            return request;
        }

        public OperationRequest DeserialiseRequest(string requestJson)
        {
            var deserialiseType = typeof(OperationRequest<>).MakeGenericType(InputType);
            OperationRequest request = (OperationRequest) JsonConvert.DeserializeObject(requestJson, deserialiseType);
            return request;
        }

        public OperationRequest DeserialiseRequest(Stream requestJson)
        {
            var task = DeserialiseRequestAsync(requestJson);
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<OperationRequest> DeserialiseRequestAsync(Stream requestJson)
        {
            var deserialiseType = typeof(OperationRequest<>).MakeGenericType(InputType);
            using(var reader = new StreamReader(requestJson))
            {
                var json = await reader.ReadToEndAsync();
                OperationRequest request = (await (Task.Factory.StartNew(() => (OperationRequest)JsonConvert.DeserializeObject(json, deserialiseType))));
                return request;
            }
        }

        public async Task<object> ProcessRequest(OperationRequest request)
        {
            _healthManager.SetActiveOperation(request);
            var requestType = request.GetType();
            var propInfo = requestType.GetProperty("Parameter");
            object parameter = propInfo.GetValue(request);
            return (await Task.Run(() => _operation.DynamicInvoke(parameter), _healthManager.GetCancellationToken(request)));
        }

        public void FinishRequest(OperationRequest request, object output, bool failed)
        {
            FinishRequestAsync(request, output, failed).RunSynchronously();
        }

        public async Task FinishRequestAsync(OperationRequest request, object output, bool failed)
        {
            // Get our specific response generic type
            var respType = typeof(OperationResponse<>).MakeGenericType(OutputType);
            // Create our response from our request
            var response = (OperationResponse) Activator.CreateInstance(respType, new object[] { request });
            // Set our result
            // This will throw an error if the output is not the correct type
            response.GetType().GetProperties().Single(x => x.Name == "Result" && x.DeclaringType == respType).GetSetMethod().Invoke(response, new object[] { output });
            response.Success = failed;
            var client = _clientFactory.CreateClient();
            var content = new StringContent(await Task.Factory.StartNew(() => JsonConvert.SerializeObject(response)), Encoding.UTF8, "application/json");
            var postResp = await client.PostAsync($"{request.CallbackUri}/{ResponseUri}", content);
            // TODO: do some error checking / retrying here
            _healthManager.FinishActiveOperation(request);
        }

        public async Task CompileOperationAsync(string location)
        {
            await Compilation.CompileScriptAsync(location, CompilationOutput);
            string compiledOperation = $"{CompilationOutput}/{Path.GetFileNameWithoutExtension(location)}.dll";

            // Load in our dll using the Operation context
            var fullLoaction = Path.GetFullPath(compiledOperation);
            var dir = Path.GetFullPath(Path.GetDirectoryName(compiledOperation));
            OperationLoadContext context = new OperationLoadContext(dir);
            var dll = context.LoadFromAssemblyPath(fullLoaction);

            // Get all types that we care about
            // Then create the Operations from those types
            // Finally set the DLL location so we can send it off to the workers
            var types = dll.GetTypes().Where(x => typeof(IOperation).IsAssignableFrom(x));
            if(types.Count() == 0) return;
            if(types.Count() != 1) throw new Exception($"Too many types that implement IOperation found in {location}, {types.Count()} found, 1 expected");
            var type = types.First();
            var inter = type.GetInterfaces().Where(i => i.IsGenericType).First();
            var generics = inter.GetGenericArguments();
            if(generics.Count() == 0) throw new Exception($"Unable to determine input type for {location}");
            InputType = generics[0];
            if(generics.Count() == 1)
            {
                OutputType = null;
            }
            else
            {
                OutputType = generics[1];
            }
            var op = Activator.CreateInstance(type) as IOperation;
            if(op == null) throw new Exception($"Unable to cast object found in {location} to IOperation");
            var typeArgs = new List<Type>() {InputType};
            if(OutputType != null) typeArgs.Add(OutputType);
            var delegateType = Expression.GetFuncType(typeArgs.ToArray());
            // TODO: this could end up holding memory it shouldn't
            // For now it's fine, but should come back to this and invoke a new object each time
            // FIXME ^
            var operation = op.GetType().GetMethod("Execute").CreateDelegate(delegateType, op);
            _operation = operation;
            Ready = true;
        }

        public void CompileOperation(string location)
        {
            var task = CompileOperationAsync(location);
            task.RunSynchronously();
        }
    }
}