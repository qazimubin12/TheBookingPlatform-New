namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofinvoice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "PaymentMethod", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "PaymentMethod");
        }
    }
}
