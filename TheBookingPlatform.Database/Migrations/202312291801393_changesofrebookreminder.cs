﻿namespace TheBookingPlatform.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changesofrebookreminder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "RebookReminderSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "RebookReminderSent");
        }
    }
}
