//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LRC_NET_Framework
{
    using System;
    using System.Collections.Generic;
    
    public partial class tb_Semesters
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_Semesters()
        {
            this.tb_SemesterTaught = new HashSet<tb_SemesterTaught>();
        }
    
        public int SemesterRecID { get; set; }
        public Nullable<System.DateTime> DateFrom { get; set; }
        public Nullable<System.DateTime> DateTo { get; set; }
        public string FiscalYear { get; set; }
        public Nullable<int> SemesterID { get; set; }
    
        public virtual tb_SemesterName tb_SemesterName { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_SemesterTaught> tb_SemesterTaught { get; set; }
    }
}
