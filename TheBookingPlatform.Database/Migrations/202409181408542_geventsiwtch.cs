namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class geventsiwtch : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GEventSwitches",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Busienss = c.String(),
                        SwitchStatus = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GEventSwitches");
        }
    }
}
