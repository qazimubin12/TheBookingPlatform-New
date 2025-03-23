namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class servicecategoryandservicesessions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ServiceSessions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AppointmentID = c.Int(nullable: false),
                        Remaining = c.Int(nullable: false),
                        Done = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.ServiceCategories", "Type", c => c.String());
            AddColumn("dbo.Services", "NumberofSessions", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "NumberofSessions");
            DropColumn("dbo.ServiceCategories", "Type");
            DropTable("dbo.ServiceSessions");
        }
    }
}
