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
    
    public partial class tb_MemberMaster
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_MemberMaster()
        {
            this.tb_Assessment = new HashSet<tb_Assessment>();
            this.tb_Attribute = new HashSet<tb_Attribute>();
            this.tb_MemberActivity = new HashSet<tb_MemberActivity>();
            this.tb_MemberAddress = new HashSet<tb_MemberAddress>();
            this.tb_MemberEmail = new HashSet<tb_MemberEmail>();
            this.tb_MemberNotes = new HashSet<tb_MemberNotes>();
            this.tb_Users = new HashSet<tb_Users>();
        }
    
        public int MemberID { get; set; }
        public int MemberIDNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public int DepartmentID { get; set; }
        public Nullable<int> AreaID { get; set; }
        public Nullable<bool> CopeStatus { get; set; }
        public Nullable<decimal> CopeAmount { get; set; }
        public Nullable<bool> Counselors { get; set; }
        public Nullable<bool> CampaignVolunteer { get; set; }
        public Nullable<byte> LatestUnionAssessment { get; set; }
        public Nullable<int> MailCodeID { get; set; }
        public Nullable<System.DateTime> DuesCategoryEffDate { get; set; }
        public Nullable<System.DateTime> UnionInitiationDate { get; set; }
        public Nullable<System.DateTime> HireDate { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public Nullable<byte> GenderID { get; set; }
        public Nullable<System.DateTime> RetiredEffDate { get; set; }
        public Nullable<System.DateTime> DeactivateEffDate { get; set; }
        public Nullable<byte> DeactivateReasonID { get; set; }
        public Nullable<byte> LeadershipPositionID { get; set; }
        public Nullable<byte> PoliticalAssessmentID { get; set; }
        public Nullable<bool> ParticipatePolitical { get; set; }
        public byte PoliticalActivitiesID { get; set; }
        public Nullable<int> MemberAddressID { get; set; }
        public Nullable<int> PhoneRecID { get; set; }
        public Nullable<int> DuesID { get; set; }
        public Nullable<int> AddedBy { get; set; }
        public Nullable<System.DateTime> AddedDateTime { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDateTime { get; set; }
    
        public virtual tb_Area tb_Area { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_Assessment> tb_Assessment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_Attribute> tb_Attribute { get; set; }
        public virtual tb_Department tb_Department { get; set; }
        public virtual tb_Dues tb_Dues { get; set; }
        public virtual tb_LatestUnionAssessment tb_LatestUnionAssessment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberActivity> tb_MemberActivity { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberAddress> tb_MemberAddress { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberEmail> tb_MemberEmail { get; set; }
        public virtual tb_MemberPhoneNumbers tb_MemberPhoneNumbers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_MemberNotes> tb_MemberNotes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_Users> tb_Users { get; set; }
        public virtual tb_Gender tb_Gender { get; set; }
    }
}