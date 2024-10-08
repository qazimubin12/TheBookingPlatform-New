namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class appointmentiscancelledandcancellationtime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "IsCancelled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Companies", "CancellationTime", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "CancellationTime");
            DropColumn("dbo.Appointments", "IsCancelled");
        }
    }
}
