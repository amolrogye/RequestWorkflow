//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RequestWorkflow.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserLog
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> LogDate { get; set; }
        public string LogMsg { get; set; }
        public Nullable<int> ModuleId { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<bool> IsSystemGenerated { get; set; }
        public Nullable<bool> IsViewed { get; set; }
    
        public virtual ModuleMaster ModuleMaster { get; set; }
        public virtual UserMaster UserMaster { get; set; }
    }
}
