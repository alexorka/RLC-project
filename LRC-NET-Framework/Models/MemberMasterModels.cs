namespace LRC_NET_Framework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tb_MemberMaster")]
    public partial class MemberMasterModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MemberID { get; set; }

        public int MemberIDNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        public int DepartmentID { get; set; }

        public int? AreaID { get; set; }

        public bool? CopeStatus { get; set; }

        [Column(TypeName = "money")]
        public decimal? CopeAmount { get; set; }

        public bool? Counselors { get; set; }

        public bool? CampaignVolunteer { get; set; }

        public byte? LatestUnionAssessment { get; set; }

        public int? MailCodeID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DuesCategoryEffDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? UnionInitiationDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? HireDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        public byte? GenderID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RetiredEffDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DeactivateEffDate { get; set; }

        public byte? DeactivateReasonID { get; set; }

        public byte? LeadershipPositionID { get; set; }

        public byte? PoliticalAssessmentID { get; set; }

        public bool? ParticipatePolitical { get; set; }

        public byte PoliticalActivitiesID { get; set; }

        public int? MemberAddressID { get; set; }

        public int? PhoneRecID { get; set; }

        public int? DuesID { get; set; }

        public int? AddedBy { get; set; }

        public DateTime? AddedDateTime { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedDateTime { get; set; }
    }
}
