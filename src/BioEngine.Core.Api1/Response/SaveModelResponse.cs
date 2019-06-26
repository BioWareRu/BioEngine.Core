using JetBrains.Annotations;

namespace BioEngine.Core.Api.Response
{
    public class SaveModelResponse<T> : RestResponse
    {
        public SaveModelResponse(int code, T model) : base(code)
        {
            Model = model;
        }

        [UsedImplicitly] public T Model { get; }
    }
}