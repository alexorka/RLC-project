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
    
    public partial class tb_SemesterName
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_SemesterName()
        {
            this.tb_Semesters = new HashSet<tb_Semesters>();
        }
    
        public int SemesterID { get; set; }
        public string SemesterName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_Semesters> tb_Semesters { get; set; }
    }
}
