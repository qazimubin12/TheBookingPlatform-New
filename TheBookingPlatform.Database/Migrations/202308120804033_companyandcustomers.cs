namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyandcustomers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Address = c.String(),
                        PostalCode = c.Int(nullable: false),
                        City = c.String(),
                        PhoneNumber = c.String(),
                        Logo = c.String(),
                        NotificationEmail = c.String(),
                        ContactEmail = c.String(),
                        BillingEmail = c.String(),
                        EmployeesLinked = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Gender = c.String(),
                        DateOfBirth = c.DateTime(nullable: false),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        MobileNumber = c.String(),
                        Address = c.String(),
                        PostalCode = c.Int(nullable: false),
                        City = c.String(),
                        ProfilePicture = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Customers");
            DropTable("dbo.Companies");
        }
    }
}
