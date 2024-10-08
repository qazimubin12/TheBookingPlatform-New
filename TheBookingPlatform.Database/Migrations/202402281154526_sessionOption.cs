namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sessionOption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "PaymentSession", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "PaymentSession");
        }
    }
}
