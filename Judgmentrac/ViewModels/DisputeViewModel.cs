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
        public decimal PaymentTotal { get; set; }
        public decimal InterestTotal { get; set; }


        /// <summary>
        /// constructor that will do the view model calculations for values NOT stored in database
        /// </summary>
        public DisputeViewModel(Dispute dispute)
        {
            Dispute = dispute;
            // add payment list to PaymentViewModel
            //foreach (Payment payment in dispute.Payments)
            //    PaymentList.Add(new PaymentViewModel(payment));

            // calculate interest total
            decimal principal = dispute.Principal;
            DateTime startDate = dispute.StartDate;
            decimal interestRate = Dispute.Rate * Convert.ToDecimal(.01);
            double totalDays = DateTime.Today.Subtract(startDate).TotalDays;
            if (dispute.Payments != null && dispute.Payments.Count > 0)
            {
                // calculate interest total for each payment increment
                foreach (Payment payment in dispute.Payments)
                {
                    totalDays = payment.PayDate.Subtract(startDate).TotalDays;
                    InterestTotal += principal * interestRate * Convert.ToDecimal(totalDays / 365);
                    principal -= payment.Amount;
                    startDate = payment.PayDate;
                }
            }
            else
            {
                // calculate interest total to date w/o payments
                InterestTotal = principal * interestRate * Convert.ToDecimal(totalDays / 365);
            }

            // calculate payment total
            if (dispute.Payments != null)
            {
                // calculate payment total for all payments
                foreach (Payment payment in dispute.Payments)
                    PaymentTotal += payment.Amount;
            }
            else
            {
                //PaymentList = null;
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