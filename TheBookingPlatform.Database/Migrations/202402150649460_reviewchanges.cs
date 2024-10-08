namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class reviewchanges : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CustomerID = c.Int(nullable: false),
                        Rating = c.Single(nullable: false),
                        EmployeeID = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Feedback = c.String(),
                        AppointmentID = c.Int(nullable: false),
                        FeedbackReminder = c.Boolean(nullable: false),
                        EmailOpened = c.Boolean(nullable: false),
                        EmailClicked = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.Customers", "DateAdded", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "DateAdded");
            DropTable("dbo.Reviews");
        }
    }
}
