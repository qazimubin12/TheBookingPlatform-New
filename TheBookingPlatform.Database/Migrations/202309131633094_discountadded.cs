namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class discountadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Discount", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "Discount");
        }
    }
}
