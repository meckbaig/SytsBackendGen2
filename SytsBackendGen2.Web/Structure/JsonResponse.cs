using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SytsBackendGen2.Application.Common.BaseRequests;
using Microsoft.AspNetCore.Http.HttpResults;
using MassTransit.Internals;
using SytsBackendGen2.Application.Common.Attributes;

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
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CustomJsonResponseContractResolver()
            };
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

        public class CustomJsonResponseContractResolver : CamelCasePropertyNamesContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);
                foreach (var property in properties.ToList())
                {
                    property.ShouldSerialize = instance =>
                    {
                        var attribute = property.AttributeProvider.GetAttributes(typeof(IgnoreIfNullAttribute), true).FirstOrDefault();
                        if (attribute != null)
                        {
                            var value = property.ValueProvider.GetValue(instance);
                            return value != null;
                        }
                        return true;
                    };
                }

                return properties;
            }
        }
    }
}
