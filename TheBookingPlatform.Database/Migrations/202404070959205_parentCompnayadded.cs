namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class parentCompnayadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "ParentCompanyID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "ParentCompanyID");
        }
    }
}
