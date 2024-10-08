namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class depositchanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "Deposit", c => c.Single(nullable: false));
            AddColumn("dbo.Appointments", "DepositMethod", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "DepositMethod");
            DropColumn("dbo.Appointments", "Deposit");
        }
    }
}
