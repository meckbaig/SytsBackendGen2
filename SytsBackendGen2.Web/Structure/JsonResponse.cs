using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SytsBackendGen2.Application.Common.BaseRequests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SytsBackendGen2.Web.Structure
{
    /// <summary>
    /// Class for returning result as Json
    /// </summary>
    public static class JsonResponseClass
    {
        /// <summary>
        /// Returns response as Json or throws exception from response
        /// </summary>
        /// <param name="response">Response model</param>
        /// <returns>Json content result</returns>
        /// <exception cref="Exception">Exception from response</exception>
        public static ContentResult ToJsonResponse(this BaseResponse response)
        {
            var result = new ContentResult();
            if (response.GetException() != null)
                throw response.GetException() ?? new Exception(response.GetMessage());
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            result.Content = JsonConvert.SerializeObject(response, settings);
            if (!string.IsNullOrEmpty(result.Content) && result.Content != "{}")
            {
                result.ContentType = "application/json";
                return result;
            }
            result.ContentType = null;
            result.Content = null;
            return result;
        }
    }
}
