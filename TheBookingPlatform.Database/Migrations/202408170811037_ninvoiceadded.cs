namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ninvoiceadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NInvoices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        CustomerName = c.String(),
                        CustomerEmail = c.String(),
                        CustomerPhone = c.String(),
                        CustomerAddress = c.String(),
                        CompanyLogo = c.String(),
                        CompanyName = c.String(),
                        CompanyEmail = c.String(),
                        CompanyPhone = c.String(),
                        CompanyAddress = c.String(),
                        IssueDate = c.DateTime(nullable: false),
                        VAT = c.Int(nullable: false),
                        InvoiceNo = c.String(),
                        DueDate = c.DateTime(nullable: false),
                        PaymentMethod = c.String(),
                        ItemDetails = c.String(),
                        Remarks = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.NInvoices");
        }
    }
}
