namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fileesmessageshistory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Files",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        URL = c.String(),
                        Size = c.String(),
                        UploadedBy = c.String(),
                        DateTime = c.DateTime(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CustomerID = c.Int(nullable: false),
                        EmployeeID = c.Int(nullable: false),
                        Note = c.String(),
                        Date = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        CustomerID = c.Int(nullable: false),
                        EmployeeID = c.Int(nullable: false),
                        GrandTotal = c.Single(nullable: false),
                        OrderDate = c.DateTime(nullable: false),
                        OrderNo = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Type = c.String(),
                        Status = c.String(),
                        Subject = c.String(),
                        Channel = c.String(),
                        SentBy = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Messages");
            DropTable("dbo.Invoices");
            DropTable("dbo.Histories");
            DropTable("dbo.Files");
        }
    }
}
