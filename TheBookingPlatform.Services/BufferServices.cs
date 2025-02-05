using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheBookingPlatform.Database;
using TheBookingPlatform.Entities;
using Buffer = TheBookingPlatform.Entities.Buffer;
namespace TheBookingPlatform.Services
{
    public class BufferServices
    {
        #region Singleton
        public static BufferServices Instance
        {
            get
            {
                if (instance == null) instance = new BufferServices();
                return instance;
            }
        }
        private static BufferServices instance { get; set; }
        private BufferServices()
        {
        }
        #endregion




        public Buffer GetBufferWRTBusiness(string Business, int AppointmentID)
        {
            using (var context = new DSContext())
            {

                return context.Buffers.Where(x => x.Business == Business && x.AppointmentID == AppointmentID).FirstOrDefault();

            }
        }

        public List<Buffer> GetBufferWRTBusinessList(string Business, int AppointmentID)
        {
            using (var context = new DSContext())
            {

                return context.Buffers.Where(x => x.Business == Business && x.AppointmentID == AppointmentID).ToList();

            }
        }

        public Buffer GetBuffer(int ID)
        {
            using (var context = new DSContext())
            {

                return context.Buffers.Find(ID);

            }
        }

        public void SaveBuffer(Buffer Buffer)
        {
            using (var context = new DSContext())
            {
                try
                {
                    context.Buffers.Add(Buffer);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }

        public void UpdateBuffer(Buffer Buffer)
        {
            using (var context = new DSContext())
            {
                context.Entry(Buffer).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public string DeleteBuffer(int ID)
        {
            using (var context = new DSContext())
            {
                var Buffer = context.Buffers.Find(ID);
                context.Buffers.Remove(Buffer);
                context.SaveChanges();
                return "Deleted Successfully";

            }
        }

    }

}
