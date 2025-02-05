namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class v3changestipsandbuffer : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Buffers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        Description = c.String(),
                        Time = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        Date = c.DateTime(nullable: false),
                        ServiceID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Appointments", "Tip", c => c.Boolean(nullable: false));
            AddColumn("dbo.Appointments", "TipType", c => c.String());
            AddColumn("dbo.Appointments", "TipAmount", c => c.Single(nullable: false));
            AddColumn("dbo.EmployeeServices", "BufferEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.EmployeeServices", "BufferTime", c => c.String());
            AddColumn("dbo.Invoices", "TipAmount", c => c.Single(nullable: false));
            AddColumn("dbo.Invoices", "TipType", c => c.String());
            AddColumn("dbo.Invoices", "Tip", c => c.Boolean(nullable: false));
            DropColumn("dbo.Services", "BufferTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Services", "BufferTime", c => c.String());
            DropColumn("dbo.Invoices", "Tip");
            DropColumn("dbo.Invoices", "TipType");
            DropColumn("dbo.Invoices", "TipAmount");
            DropColumn("dbo.EmployeeServices", "BufferTime");
            DropColumn("dbo.EmployeeServices", "BufferEnabled");
            DropColumn("dbo.Appointments", "TipAmount");
            DropColumn("dbo.Appointments", "TipType");
            DropColumn("dbo.Appointments", "Tip");
            DropTable("dbo.Buffers");
        }
    }
}
