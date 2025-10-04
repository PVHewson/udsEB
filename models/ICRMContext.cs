// Model/ICrmContext.cs
using Microsoft.Xrm.Sdk;

namespace uds.CRM.Model
{
    /// <summary>
    /// Runtime services available to business logic; created per plugin execution.
    /// </summary>
    public interface ICrmContext
    {
        IOrganizationService Org { get; }
        ITracingService Trace { get; }
        IPluginExecutionContext Pipeline { get; }

        // Optional: if you're using the helper factory pattern as well
        // ICrmFactory Factory { get; }
    }
}
