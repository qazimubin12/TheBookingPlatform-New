﻿namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rosterstartdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TimeTables", "RosterStartDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TimeTables", "RosterStartDate");
        }
    }
}
