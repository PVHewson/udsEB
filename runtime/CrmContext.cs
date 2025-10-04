// Runtime/CrmContext.cs
using Microsoft.Xrm.Sdk;

namespace uds.CRM.Model
{
    /// <summary>Creates Org/Trace/Pipeline for the current plugin execution.</summary>
    public sealed class CrmContext : ICrmContext
    {
        public IOrganizationService Org { get; }
        public ITracingService Trace { get; }
        public IPluginExecutionContext Pipeline { get; }

        public CrmContext(System.IServiceProvider sp)
        {
            Pipeline = (IPluginExecutionContext)sp.GetService(typeof(IPluginExecutionContext));
            Trace    = (ITracingService)sp.GetService(typeof(ITracingService));
            var fac  = (IOrganizationServiceFactory)sp.GetService(typeof(IOrganizationServiceFactory));

            // Use the triggering user for transaction/permission correctness
            Org = fac.CreateOrganizationService(Pipeline.UserId);
        }
    }
}
