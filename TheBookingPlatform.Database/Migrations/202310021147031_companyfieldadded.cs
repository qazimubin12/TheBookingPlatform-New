namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyfieldadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "CountryName", c => c.String());
            AddColumn("dbo.Companies", "Currency", c => c.String());
            AddColumn("dbo.Companies", "InvoiceLine", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "InvoiceLine");
            DropColumn("dbo.Companies", "Currency");
            DropColumn("dbo.Companies", "CountryName");
        }
    }
}
