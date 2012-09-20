namespace Judgmentrac.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserProfileJudgmentIndentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfileJudgments", "invoice", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.UserProfileJudgments", new[] { "UserId" });
            AddPrimaryKey("dbo.UserProfileJudgments", "invoice");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.UserProfileJudgments", new[] { "invoice" });
            AddPrimaryKey("dbo.UserProfileJudgments", "UserId");
            DropColumn("dbo.UserProfileJudgments", "invoice");
        }
    }
}
