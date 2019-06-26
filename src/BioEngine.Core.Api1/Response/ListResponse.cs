using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BioEngine.Core.Api.Response
{
    public class ListResponse<T> : RestResponse
    {
        public ListResponse(IEnumerable<T> data, int totalitem) : base(StatusCodes.Status200OK)
        {
            Data = data;
            TotalItems = totalitem;
        }

        [JsonProperty] public IEnumerable<T> Data { get; }

        [JsonProperty] public int TotalItems { get; }
    }
}