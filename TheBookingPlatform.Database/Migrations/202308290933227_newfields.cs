namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newfields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Description", c => c.String());
            AddColumn("dbo.Employees", "Specialization", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Specialization");
            DropColumn("dbo.Employees", "Description");
        }
    }
}
