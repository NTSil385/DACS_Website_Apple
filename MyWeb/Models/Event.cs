//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Event
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Event()
        {
            this.Promotion_Apply = new HashSet<Promotion_Apply>();
        }
    
        public int ID { get; set; }
        public string Event_Name { get; set; }
        public Nullable<System.DateTime> DateStart { get; set; }
        public Nullable<System.DateTime> DateEnd { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string Fomular { get; set; }
        public Nullable<double> DisscountPercent { get; set; }
        public bool Status { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Promotion_Apply> Promotion_Apply { get; set; }
    }
}
