namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removelastpaymentDate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "LastPaymentDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "LastPaymentDate", c => c.String());
        }
    }
}
