namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companydeposit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "Deposit", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "Deposit");
        }
    }
}
