using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;

namespace TheBookingPlatform.Services
{
    public class PaymentServices
    {
        #region Singleton
        public static PaymentServices Instance
        {
            get
            {
                if (instance == null) instance = new PaymentServices();
                return instance;
            }
        }
        private static PaymentServices instance { get; set; }
        private PaymentServices()
        {
        }
        #endregion



        public List<Payment> GetPayment(string UserID)
        {
            using (var context = new DSContext())
            {
                return context.Payments.Where(x => x.UserID == UserID).OrderBy(x => x.LastPaidDate).ToList();

            }
        }
        public List<Payment> GetPayment()
        {
            using (var context = new DSContext())
            {
                return context.Payments.OrderBy(x => x.LastPaidDate).ToList();

            }
        }
        public List<Payment> GetPaymentWRTBusiness(string Business)
        {
            using (var context = new DSContext())
            {
                return context.Payments.Where(x => x.Business == Business).OrderBy(x => x.LastPaidDate).ToList();

            }
        }


        public Payment GetPayment(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Payments.Find(ID);

            }
        }

        public void SavePayment(Payment Payment)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.Payments.Add(Payment);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdatePayment(Payment Payment)
        {
            using (var context = new DSContext())
            {
                context.Entry(Payment).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeletePayment(int ID)
        {
            using (var context = new DSContext())
            {
                var Payment = context.Payments.Find(ID);
                context.Payments.Remove(Payment);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }


    }
}
