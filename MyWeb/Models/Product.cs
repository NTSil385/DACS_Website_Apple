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
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.Info_BillSales = new HashSet<Info_BillSales>();
            this.Imprt_Details = new HashSet<Imprt_Details>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public string Discription { get; set; }
        public string Images { get; set; }
        public Nullable<double> Price { get; set; }
        public Nullable<double> Disscount { get; set; }
        public string Producer { get; set; }
        public Nullable<int> Id_Category { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Info_BillSales> Info_BillSales { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Imprt_Details> Imprt_Details { get; set; }
    }
}
