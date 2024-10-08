namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Servicesthing : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.ServiceCategories",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Category = c.String(),
                        Price = c.Single(nullable: false),
                        VAT = c.String(),
                        Duration = c.String(),
                        Setup = c.String(),
                        Processing = c.String(),
                        Finish = c.String(),
                        BufferTime = c.String(),
                        Employee = c.String(),
                        Tool = c.String(),
                        Room = c.String(),
                        Notes = c.String(),
                        CanBookOnline = c.Boolean(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Tools",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Vats",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Percentage = c.Single(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Vats");
            DropTable("dbo.Tools");
            DropTable("dbo.Services");
            DropTable("dbo.ServiceCategories");
            DropTable("dbo.Rooms");
            DropTable("dbo.Employees");
        }
    }
}
