namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cityremovedfromincorrect : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Employees", "City");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "City", c => c.String());
        }
    }
}
