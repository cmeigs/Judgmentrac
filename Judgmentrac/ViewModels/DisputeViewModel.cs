using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Judgmentrac.Models;

namespace Judgmentrac.ViewModels
{
    public class DisputeViewModel
    {
        public Dispute Dispute { get; set; }
        public ICollection<Payment> PaymentList { get; set; }
        public decimal PaymentTotal { get; set; }

        public DisputeViewModel(Dispute dispute)
        {
            Dispute = dispute;
            if (dispute.Payments != null)
            {
                PaymentList = dispute.Payments;
                foreach (Payment payment in dispute.Payments)
                    PaymentTotal += payment.Amount;
            }
            else
            {
                PaymentList = null;
                PaymentTotal = 0;
            }
        }

        //public DisputeViewModel(IEnumerable<Dispute> disputes)
        //{
        //    foreach (Dispute dispute in disputes)
        //    {
        //        Dispute = dispute;
        //        if (dispute.Payments != null)
        //        {
        //            PaymentList = dispute.Payments;
        //            foreach (Payment payment in dispute.Payments)
        //                PaymentTotal += payment.Amount;
        //        }
        //        else
        //        {
        //            PaymentList = null;
        //            PaymentTotal = 0;
        //        }
        //    }
        //}
    }
}