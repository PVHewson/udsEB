using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.PluginTelemetry;
using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using uds.CRM.Model;
using static uds.CRM.Model.ContextMixin;

namespace uds
{
    /// <summary>
    /// Base class for all plug-in classes.
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public class PluginAccountUpdatePreOperation : IPlugin
    {
        protected string PluginClassName { get; }

        /// <summary>
        /// Public parameterless constructor required by the CRM runtime.
        /// </summary>
        public PluginAccountUpdatePreOperation() : this(typeof(PluginAccountUpdatePreOperation))
        {
        }

        /// <summary>
        /// Optional constructor used when unsecure/secure configuration is provided at registration.
        /// </summary>
        public PluginAccountUpdatePreOperation(string unsecureConfiguration, string secureConfiguration)
            : this(typeof(PluginAccountUpdatePreOperation))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginAccountUpdatePreOperation"/> class.
        /// </summary>
        /// <param name="pluginClassName">The <see cref="Type"/> of the plugin class.</param>
        internal PluginAccountUpdatePreOperation(Type pluginClassName)
        {
            PluginClassName = pluginClassName.ToString();
        }

        /// <summary>
        /// Main entry point for he business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Execute")]
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException(nameof(serviceProvider));
            }

            // Construct the local plug-in context.
            var localPluginContext = new LocalPluginContext(serviceProvider);

            localPluginContext.Trace($"Entered {PluginClassName}.Execute() " +
                $"Correlation Id: {localPluginContext.PluginExecutionContext.CorrelationId}, " +
                $"Initiating User: {localPluginContext.PluginExecutionContext.InitiatingUserId}");

            try
            {
                var crm = new CrmContext(serviceProvider);

                var ctx = crm.Pipeline;
                Entity post = (Entity)ctx.InputParameters["Target"];   // ensure image configured
                Account acc = post.ToEntity<Account>();

                Entity pre = ctx.PreEntityImages["pre"];   // ensure image configured
                Account old = pre.ToEntity<Account>();

                localPluginContext.Trace("Merge pre image");
                EntityMergeExtensions.MergePreImage(acc, old);

                // Attach context ONCE for this instance
                acc.Use(crm);                

                acc
                .EnsurePrimaryContactHasCity()
                .EnsureAccountNameGreaterThanThreeChars();

                return;
            }
            catch (FaultException<OrganizationServiceFault> orgServiceFault)
            {
                localPluginContext.Trace($"Exception: {orgServiceFault.ToString()}");

                throw new InvalidPluginExecutionException($"OrganizationServiceFault: {orgServiceFault.Message}", orgServiceFault);
            }
            finally
            {
                localPluginContext.Trace($"Exiting {PluginClassName}.Execute()");
            }
        }
    }
}
