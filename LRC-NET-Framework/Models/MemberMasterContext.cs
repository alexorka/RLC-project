namespace LRC_NET_Framework
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MemberMasterContext : DbContext
    {
        public MemberMasterContext()
            : base("name=LRC_DBConnection")
        {
        }

        public virtual DbSet<MemberMasterModel> tb_MemberMaster { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MemberMasterModel>()
                .Property(e => e.CopeAmount)
                .HasPrecision(19, 4);
        }
    }
}
