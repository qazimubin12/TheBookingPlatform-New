namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promoprice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Services", "PromoPrice", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Services", "PromoPrice");
        }
    }
}
