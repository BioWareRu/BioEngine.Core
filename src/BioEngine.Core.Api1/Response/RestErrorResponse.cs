using BioEngine.Core.Api.Interfaces;

namespace BioEngine.Core.Api.Response
{
    public class RestErrorResponse : IErrorInterface
    {
        public RestErrorResponse(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}