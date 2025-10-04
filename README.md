## How These Pieces Fit Together

### ICrmContext
- Purpose: Defines the runtime services available during a plugin execution: `Org`, `Trace`, and `Pipeline`.
- Where: `models/ICRMContext.cs`
- Role: Implemented per execution (e.g., by `CrmContext`) and passed to business logic so it can call Dataverse, trace, and read pipeline data.

### ContextMixin
- Purpose: Attaches an `ICrmContext` to any `Entity` instance and exposes ergonomic helpers.
- Where: `models/ContextMixin.cs`
- Key parts:
  - `Use(ICrmContext)`: associates context with an `Entity` instance via `ConditionalWeakTable`.
  - Accessors: `CRM()`, `Org()`, `Tracer()`, `Pipeline()` pull services from the attached context.
  - `Trace(...)` overloads: convenient tracing; behavior configurable via `MissingContextBehavior` (`StrictThrow` vs `SafeNoOp`).
- Role: Lets entity partials call `this.Org()`, `this.Pipeline()`, `this.Trace(...)` without storing fields or being tightly coupled to plugin plumbing.

### Account Controller (business logic)
- Purpose: Business rules for the `Account` entity via partial class methods.
- Where: `controllers/account.cs`
- Key methods:
  - `EnsureAccountNameGreaterThanThreeChars()`: validates name length; throws `InvalidPluginExecutionException` to cancel when invalid.
  - `EnsurePrimaryContactHasCity()`: uses `Pipeline()` (user), fetches contact, and uses `Org().Update(...)` to set `address1_city` if missing.
- Role: Pure business operations that rely on services provided by the mixin-attached context.

### How They Work Together
- A plugin creates an `ICrmContext` (e.g., `CrmContext`) and attaches it to the target entity: `account.Use(crm)`.
- From then on, `Account` methods can access services and tracing via the mixin (`Org()`, `Pipeline()`, `Trace(...)`) without carrying service fields.
- Exceptions thrown by `Account` methods (e.g., validation) bubble up to the plugin pipeline to cancel or report errors.

