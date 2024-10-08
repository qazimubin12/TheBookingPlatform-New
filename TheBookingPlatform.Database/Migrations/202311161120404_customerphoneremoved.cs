namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customerphoneremoved : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Customers", "PhoneNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "PhoneNumber", c => c.String());
        }
    }
}
