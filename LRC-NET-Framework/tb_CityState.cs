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
    
    public partial class tb_CityState
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_CityState()
        {
            this.tb_MemberAddress = new HashSet<tb_MemberAddress>();
        }
    
        public int CityID { get; set; }
        public int StateCodeID { get; set; }
        public string CityName { get; set; }
        public string CityAlias { get; set; }
    
        public virtual tb_States tb_States { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberAddress> tb_MemberAddress { get; set; }
    }
}
