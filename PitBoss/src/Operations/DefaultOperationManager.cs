using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using PitBoss.Utils;

namespace PitBoss {
    public class DefaultOperationManager : IOperationManager
    {
        private Delegate _operation;
        private Type _parameterType;
        private Type _outputType;
        private const string CompilationOutput = "compiled";
        private const string ResponseUri = "operation/result";
        private IHttpClientFactory _clientFactory;

        public DefaultOperationManager(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public OperationRequest DeserialiseRequest(string requestJson)
        {
            var deserialiseType = typeof(OperationRequest<>).MakeGenericType(_parameterType);
            OperationRequest request = (OperationRequest) JsonSerializer.Deserialize(requestJson, deserialiseType);
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
            var deserialiseType = typeof(OperationRequest<>).MakeGenericType(_parameterType);
            OperationRequest request = (OperationRequest)(await JsonSerializer.DeserializeAsync(requestJson, deserialiseType));
            return request;
        }

        public object ProcessRequest(OperationRequest request)
        {
            var requestType = request.GetType();
            var propInfo = requestType.GetProperty("Parameter");
            object parameter = propInfo.GetValue(request);
            return _operation.DynamicInvoke(parameter);
        }

        public void FinishRequest(OperationRequest request, object output)
        {
            FinishRequestAsync(request, output).RunSynchronously();
        }

        public async Task FinishRequestAsync(OperationRequest request, object output)
        {
            // Get our specific response generic type
            var respType = typeof(OperationResponse<>).MakeGenericType(_outputType);
            // Create our response from our request
            var response = (OperationResponse) Activator.CreateInstance(respType, new object[] { request });
            // Set our result
            // This will throw an error if the output is not the correct type
            response.GetType().GetProperty("Result").GetSetMethod().Invoke(response, new object[] { output });
            var client = _clientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(response, respType), Encoding.UTF8, "application/json");
            var postResp = await client.PostAsync($"{request.CallbackUri}/{ResponseUri}", content);
            // TODO: do some error checking / retrying here
        }

        public OperationStatus GetStatus()
        {
            return new OperationStatus();
        }

        public OperationStatus GetStatus(OperationRequest request)
        {
            return new OperationStatus();
        }

        public Delegate CompileOperation(string location)
        {
            Compilation.CompileScript(location, CompilationOutput);
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
            if(types.Count() == 0) throw new Exception($"No types that implement IOperation found in {location}");
            if(types.Count() != 1) throw new Exception($"Too many types that implement IOperation found in {location}, {types.Count()} found, 1 expected");
            var type = types.First();
            if(type.GenericTypeArguments.Count() == 0) throw new Exception($"Unable to determine input type for {location}");
            _parameterType = type.GenericTypeArguments[0];
            if(type.GenericTypeArguments.Count() == 1)
            {
                _outputType = null;
            }
            else
            {
                _outputType = type.GenericTypeArguments[1];
            }
            var op = Activator.CreateInstance(type) as IOperation;
            if(op == null) throw new Exception($"Unable to cast object found in {location} to IOperation");
            // TODO: this could end up holding memory it shouldn't
            // For now it's fine, but should come back to this and invoke a new object each time
            // FIXME ^
            var operation = Delegate.CreateDelegate(op.GetType(), op, op.GetType().GetMethod("Execute"));
            return operation;
        }
    }
}