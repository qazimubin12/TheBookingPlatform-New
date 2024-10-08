namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyintegrationofpaymentoption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "PaymentMethodIntegration", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "PaymentMethodIntegration");
        }
    }
}
