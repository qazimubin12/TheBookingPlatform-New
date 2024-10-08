namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newchangesinshift : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExceptionShifts", "IsNotWorking", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExceptionShifts", "IsNotWorking");
        }
    }
}
