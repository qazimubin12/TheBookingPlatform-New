namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rosteradded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Rosters",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        EmployeeID = c.Int(nullable: false),
                        Type = c.String(),
                        StartDate = c.String(),
                        EndDate = c.String(),
                        Business = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            DropColumn("dbo.TimeTables", "Type");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TimeTables", "Type", c => c.String());
            DropTable("dbo.Rosters");
        }
    }
}
