namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofisrepeatinappointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "IsRepeat", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "IsRepeat");
        }
    }
}
