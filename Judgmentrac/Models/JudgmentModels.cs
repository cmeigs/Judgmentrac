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
            // turn off lazy loading
            //this.Configuration.LazyLoadingEnabled = false;
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
        [DataType(DataType.Currency)]
        [Range(1, 1000000)]
        public decimal Principal { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int Rate { get; set; }
        
        [Required]
        [Display(Name="Start Date")]
        public DateTime StartDate { get; set; }
        
        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Required]
        public int UserId { get; set; }

        public bool IsActive { get; set; }

        // virtual attribute here will enable Lazy-Loading
        public virtual ICollection<Payment> Payments { get; set; }
        //public ICollection<Payment> Payments { get; set; }
    }


    public class Payment
    {
        [Key]
        public int ID { get; set; }
        
        [Required]
        [Display(Name = "Pay Date")]
        public DateTime PayDate { get; set; }
        
        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        // virtual attribute here will enable Lazy-Loading
        public virtual Dispute Dispute { get; set; }
        //public Dispute Dispute { get; set; }
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