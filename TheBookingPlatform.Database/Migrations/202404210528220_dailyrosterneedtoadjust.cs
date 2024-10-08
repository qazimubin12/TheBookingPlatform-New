namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dailyrosterneedtoadjust : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTables", "Type", c => c.String());
            DropTable("dbo.Rosters");
        }
        
        public override void Down()
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
    }
}
