namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class paymentchangessubbscr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Payments", "SubcriptionID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Payments", "SubcriptionID");
        }
    }
}
