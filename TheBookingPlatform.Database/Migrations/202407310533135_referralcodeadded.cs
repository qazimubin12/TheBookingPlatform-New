﻿namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class referralcodeadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "ReferralPercentage", c => c.Single(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "ReferralPercentage");
        }
    }
}
