namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class employeeRequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmployeeRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        Accepted = c.Boolean(nullable: false),
                        Status = c.String(),
                        CompanyIDFrom = c.Int(nullable: false),
                        CompanyIDFor = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EmployeeRequests");
        }
    }
}
