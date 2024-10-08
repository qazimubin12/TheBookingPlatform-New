namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class employeepricechange : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmployeePriceChanges",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        TypeOfChange = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Percentage = c.Single(nullable: false),
                        Repeat = c.Boolean(nullable: false),
                        Frequency = c.String(),
                        Every = c.String(),
                        Days = c.String(),
                        Ends = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Appointments", "EmployeePriceChange", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "EmployeePriceChange");
            DropTable("dbo.EmployeePriceChanges");
        }
    }
}
