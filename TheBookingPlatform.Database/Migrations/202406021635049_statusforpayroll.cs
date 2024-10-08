namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class statusforpayroll : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "StatusForPayroll", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "StatusForPayroll");
        }
    }
}
