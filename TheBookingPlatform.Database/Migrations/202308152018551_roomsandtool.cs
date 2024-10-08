namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class roomsandtool : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Rooms", newName: "Resources");
            AddColumn("dbo.Resources", "Type", c => c.String());
            AddColumn("dbo.Services", "DoesRequiredProcessing", c => c.Boolean(nullable: false));
            DropTable("dbo.Tools");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Tools",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.Services", "DoesRequiredProcessing");
            DropColumn("dbo.Resources", "Type");
            RenameTable(name: "dbo.Resources", newName: "Rooms");
        }
    }
}
