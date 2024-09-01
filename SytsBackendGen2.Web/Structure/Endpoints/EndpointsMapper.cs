using System.Reflection;
using SytsBackendGen2.Web.Structure;

namespace SytsBackendGen2.Web.Structure
{
    public static class EndpointsMapper
    {
        public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
        {
            var groupName = group.GetType().Name;

            return app
                .MapGroup($"/api/{groupName}")
                .WithGroupName(groupName)
                .WithTags(groupName)
                .WithOpenApi();
        }

        public static WebApplication MapEndpoints(this WebApplication app)
        {
            var endpointGroupType = typeof(EndpointGroupBase);

            var assembly = Assembly.GetExecutingAssembly();

            var endpointGroupTypes = assembly.GetExportedTypes()
                .Where(t => t.IsSubclassOf(endpointGroupType));

            foreach (var type in endpointGroupTypes)
            {
                if (Activator.CreateInstance(type) is EndpointGroupBase instance)
                {
                    instance.Map(app);
                }
            }

            return app;
        }
    }
}
