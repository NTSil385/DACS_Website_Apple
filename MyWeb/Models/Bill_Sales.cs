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
    
    public partial class Bill_Sales
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Bill_Sales()
        {
            this.Info_BillSales = new HashSet<Info_BillSales>();
            this.Promotion_Apply = new HashSet<Promotion_Apply>();
        }
    
        public int ID { get; set; }
        public Nullable<System.DateTime> DateFounded { get; set; }
        public Nullable<int> Id_User { get; set; }
        public Nullable<double> Total { get; set; }
        public Nullable<int> Status { get; set; }
        public string Des { get; set; }
    
        public virtual Status Status1 { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Info_BillSales> Info_BillSales { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Promotion_Apply> Promotion_Apply { get; set; }
    }
}