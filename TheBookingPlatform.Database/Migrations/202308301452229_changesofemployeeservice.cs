namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofemployeeservice : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmployeeServices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        ServiceID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.Employees", "Services");
            DropColumn("dbo.Services", "Employee");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Services", "Employee", c => c.String());
            AddColumn("dbo.Employees", "Services", c => c.String());
            DropTable("dbo.EmployeeServices");
        }
    }
}
