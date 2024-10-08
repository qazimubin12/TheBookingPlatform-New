namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companycode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "CompanyCode", c => c.String());
            DropColumn("dbo.Companies", "ParentCompanyID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Companies", "ParentCompanyID", c => c.Int(nullable: false));
            DropColumn("dbo.Companies", "CompanyCode");
        }
    }
}
