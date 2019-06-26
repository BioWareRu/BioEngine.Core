using BioEngine.Core.Api.Interfaces;
using Newtonsoft.Json;

namespace BioEngine.Core.Api.Response
{
    public class ValidationErrorResponse : IErrorInterface
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; }

        public string Message { get; }

        public ValidationErrorResponse(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }
}