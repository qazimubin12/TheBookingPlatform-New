namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customerdata : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "AdditionalInformation", c => c.String());
            AddColumn("dbo.Customers", "AdditionalInvoiceInformation", c => c.String());
            AddColumn("dbo.Customers", "WarningInformation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "WarningInformation");
            DropColumn("dbo.Customers", "AdditionalInvoiceInformation");
            DropColumn("dbo.Customers", "AdditionalInformation");
        }
    }
}
