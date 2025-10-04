// Model/ContextMixin.cs
#nullable enable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Xrm.Sdk;

namespace uds.CRM.Model
{
    /// <summary>Controls how Trace() behaves if no context is attached.</summary>
    public enum MissingContextBehavior { StrictThrow, SafeNoOp }

    /// <summary>
    /// Drop-in mixin that "attaches" ICrmContext to any Microsoft.Xrm.Sdk.Entity instance,
    /// and exposes convenience methods for Trace/Org/Pipeline without storing fields on your partials.
    /// </summary>
    public static class ContextMixin
    {
        // Configure once (e.g., static ctor in your plugin assembly startup)
        public static MissingContextBehavior TraceBehavior { get; set; } = MissingContextBehavior.StrictThrow;

        private sealed class Holder { public ICrmContext? Ctx; }
        private static readonly ConditionalWeakTable<Entity, Holder> Bag = new();

        /// <summary>Attach a context to an entity instance (call once per object).</summary>
        public static void Use(this Entity e, ICrmContext ctx)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            Bag.GetOrCreateValue(e).Ctx = ctx;
        }

        /// <summary>True if a context has been attached to this entity.</summary>
        public static bool HasContext(this Entity e)
            => e is not null && Bag.TryGetValue(e, out var h) && h.Ctx is not null;

        /// <summary>Get the attached context or throw if missing.</summary>
        public static ICrmContext Context(this Entity e)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));
            if (Bag.TryGetValue(e, out var h) && h.Ctx is not null) return h.Ctx;
            throw new InvalidOperationException("Context not set. Call Use(ICrmContext) first.");
        }

        // Shorthands
        public static ICrmContext CRM(this Entity e) => e.Context();
        public static IOrganizationService Org(this Entity e) => e.Context().Org;
        public static ITracingService Tracer(this Entity e) => e.Context().Trace;
        public static IPluginExecutionContext Pipeline(this Entity e) => e.Context().Pipeline;

        // -------- Tracing helpers (ergonomic for partials) --------

        public static void Trace(this Entity e, string message)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));

            if (Bag.TryGetValue(e, out var h) && h.Ctx is not null)
            {
                h.Ctx.Trace.Trace(message);
                return;
            }

            if (TraceBehavior == MissingContextBehavior.StrictThrow)
                throw new InvalidOperationException("Trace called without attached context. Call Use(ctx) first.");
            // SafeNoOp: swallow
        }

        public static void Trace(this Entity e, string format, params object[] args)
        {
            if (format is null) throw new ArgumentNullException(nameof(format));
            e.Trace(string.Format(format, args));
        }

        /// <summary>Lazy message factory: string is built only if tracing is available.</summary>
        public static void Trace(this Entity e, Func<string> messageFactory)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));

            if (Bag.TryGetValue(e, out var h) && h.Ctx is not null)
            {
                h.Ctx.Trace.Trace(messageFactory());
                return;
            }

            if (TraceBehavior == MissingContextBehavior.StrictThrow)
                throw new InvalidOperationException("Trace called without attached context. Call Use(ctx) first.");
        }
    }
}
