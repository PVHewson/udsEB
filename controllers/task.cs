using System;
using Microsoft.Xrm.Sdk;

namespace uds.CRM.Model;

public partial class Task : Entity
{
  // Add properties or methods specific to this partial class here
  public void ExecuteTask()
  {
    Console.WriteLine("Executing task...");
  }
}
