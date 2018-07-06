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
    
    public partial class tb_MemberAddress
    {
        public int MemberAddressID { get; set; }
        public int MemberID { get; set; }
        public string HomeStreet1 { get; set; }
        public string HomeStreet2 { get; set; }
        public int CityID { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string HomeLatitude { get; set; }
        public string HomeLongitude { get; set; }
        public bool IsPrimary { get; set; }
        public int AddressTypeID { get; set; }
        public int SourceID { get; set; }
        public string Source { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    
        public virtual tb_AddressSource tb_AddressSource { get; set; }
        public virtual tb_AddressType tb_AddressType { get; set; }
        public virtual tb_CityState tb_CityState { get; set; }
        public virtual tb_MemberMaster tb_MemberMaster { get; set; }
    }
}
