// Business/Account.partial.cs
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using static uds.CRM.Model.ContextMixin;

namespace uds.CRM.Model
{
  // This file sits alongside the modelbuilder-generated "partial class Account : Entity"
  public partial class Account : Entity
  {
    public Account EnsureAccountNameGreaterThanThreeChars()
    {
      // Trace() comes from the mixin (extension on Entity)
      this.Trace("Validating account name length for account {0}", Id);

      if (string.IsNullOrWhiteSpace(Name) || Name.Length <= 3)
      {
        throw new InvalidPluginExecutionException(OperationStatus.Canceled,
            "Account name must be greater than three characters.");
      }
      return this;
    }        

    public Account EnsurePrimaryContactHasCity()
    {
        // Access services from the mixin too:
        Guid userId = this.Pipeline().UserId;
        this.Trace($"User {userId} checking primary contact for account {Id} using {PrimaryContactId}");
        Collection<Contact> contacts = GetContactFromPrimarycontactid(new ColumnSet("address1_city"));
        if (contacts.Count == 0)
        {
            this.Trace("No primary contact set, skipping");
            return this; // no primary contact set
        }
        Contact con = contacts[0];

        if (con is not null && con.Address1_City == null)
        {
            con.Address1_City = "Wellington";
            this.Org().Update(con);
            this.Trace(() => $"Primary contact {con.Id} flagged by {userId}");
        }
        return this;
    }
  }
}
