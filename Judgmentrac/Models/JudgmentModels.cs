using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judgmentrac.Models
{
    public class JudgmentDB : DbContext
    {
        // constructor which will define which connection string to use (defined in web.config)
        public JudgmentDB()
            : base("JudgmentDB")
        {
        }

        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserProfileJudgment> UserProfileJudgments { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    //configure model with fluent API to account for last minute database modifications
        //}
    }

    public class Dispute
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Display(Name = "Person or Company")]
        public  string Name { get; set; }
        [Required]
        public decimal Principal { get; set; }
        [Required]
        public int Rate { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public int UserId { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }

    public class Payment
    {
        [Key]
        public int ID { get; set; }
        //[Required]
        //public int DisputeID { get; set; }
        [Required]
        public DateTime PayDate { get; set; }
        [Required]
        public decimal Amount { get; set; }

        public virtual Dispute Dispute { get; set; }
    }

    public class UserProfileJudgment
    {
        [Key]
        public int invoice { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int JudgmentCount { get; set; }
    }

}