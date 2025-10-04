using Microsoft.Xrm.Sdk;
using System;
using uds.CRM.Model;

namespace uds
{
    /// <summary>
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class PluginContactCreate : PluginBase
    {
        public PluginContactCreate(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(PluginContactCreate))
        {
            // TODO: Implement your custom configuration handling
            // https://docs.microsoft.com/powerapps/developer/common-data-service/register-plug-in#set-configuration-data
        }

        // Entry point for custom business logic execution
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }
            var LocalTracingService = localPluginContext.TracingService;
            var context = localPluginContext.PluginExecutionContext;
            LocalTracingService.Trace("Contact Create Plugin Triggered");
            
            //Check for the entity on which the plugin would be registered
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                var entity = (Entity)context.InputParameters["Target"];
                LocalTracingService.Trace($"entity.LogicalName = {entity.LogicalName}");
                // Check for entity name on which this plugin would be registered
                if (entity.LogicalName == "contact")
                {
                    Contact contact = entity.ToEntity<Contact>();
                    LocalTracingService.Trace($"email = {contact.EMailAddress1}");

                    LocalTracingService.Trace("Creating Task");
                    var task = new Task
                    {
                        Subject = "Welcome Email",
                        Description = $"Welcome to our organization, {contact.FirstName}. Your email is {contact.EMailAddress1}",
                        Category = "Email",
                        ActualDurationMinutes = 30,
                        ScheduledStart = DateTime.Now,
                        ScheduledEnd = DateTime.Now.AddHours(1),
                        RegardingObjectId = contact.ToEntityReference()
                    };
                    
                    var service = localPluginContext.PluginUserService;
                    
                    service.Create(task);
                    LocalTracingService.Trace("Task Created");
                }
            }
        }
    }
}
