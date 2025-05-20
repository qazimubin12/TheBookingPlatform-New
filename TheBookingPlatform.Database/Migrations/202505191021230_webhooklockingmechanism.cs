namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class webhooklockingmechanism : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HookLocks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IsLocked = c.Boolean(nullable: false),
                        EmployeeID = c.Int(nullable: false),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.HookLocks");
        }
    }
}
