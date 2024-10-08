namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eventswitches : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventSwitches",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        SwitchStatus = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EventSwitches");
        }
    }
}
