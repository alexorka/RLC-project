﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class LRCEntities : DbContext
    {
        public LRCEntities()
            : base("name=LRCEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tb_Area> tb_Area { get; set; }
        public virtual DbSet<tb_Attribute> tb_Attribute { get; set; }
        public virtual DbSet<tb_CityState> tb_CityState { get; set; }
        public virtual DbSet<tb_College> tb_College { get; set; }
        public virtual DbSet<tb_Department> tb_Department { get; set; }
        public virtual DbSet<tb_District> tb_District { get; set; }
        public virtual DbSet<tb_Division> tb_Division { get; set; }
        public virtual DbSet<tb_Dues> tb_Dues { get; set; }
        public virtual DbSet<tb_Gender> tb_Gender { get; set; }
        public virtual DbSet<tb_LatestUnionAssessment> tb_LatestUnionAssessment { get; set; }
        public virtual DbSet<tb_MemberAddress> tb_MemberAddress { get; set; }
        public virtual DbSet<tb_MemberEmail> tb_MemberEmail { get; set; }
        public virtual DbSet<tb_MemberMaster> tb_MemberMaster { get; set; }
        public virtual DbSet<tb_MemberNotes> tb_MemberNotes { get; set; }
        public virtual DbSet<tb_MemberPhoneNumbers> tb_MemberPhoneNumbers { get; set; }
        public virtual DbSet<tb_NoteType> tb_NoteType { get; set; }
        public virtual DbSet<tb_States> tb_States { get; set; }
        public virtual DbSet<tb_Users> tb_Users { get; set; }
        public virtual DbSet<tb_UsersCredential> tb_UsersCredential { get; set; }
        public virtual DbSet<tb_SemesterName> tb_SemesterName { get; set; }
        public virtual DbSet<tb_Semesters> tb_Semesters { get; set; }
        public virtual DbSet<tb_SemesterTaught> tb_SemesterTaught { get; set; }
        public virtual DbSet<tb_Building> tb_Building { get; set; }
        public virtual DbSet<tb_Campus> tb_Campus { get; set; }
        public virtual DbSet<tb_Class> tb_Class { get; set; }
        public virtual DbSet<tb_Room> tb_Room { get; set; }
        public virtual DbSet<tb_WeekDay> tb_WeekDay { get; set; }
        public virtual DbSet<tb_Activity> tb_Activity { get; set; }
        public virtual DbSet<tb_ActivityStatus> tb_ActivityStatus { get; set; }
        public virtual DbSet<tb_MemberActivity> tb_MemberActivity { get; set; }
        public virtual DbSet<tb_Assessment> tb_Assessment { get; set; }
        public virtual DbSet<tb_AssessmentName> tb_AssessmentName { get; set; }
    }
}
