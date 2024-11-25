namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class failedappointments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FailedAppointments",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        Failed = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FailedAppointments");
        }
    }
}
