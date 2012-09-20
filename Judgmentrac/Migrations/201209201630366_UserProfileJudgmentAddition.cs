namespace Judgmentrac.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserProfileJudgmentAddition : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfileJudgments",
                c => new
                    {
                        invoice = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        JudgmentCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.invoice);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Payments", new[] { "Dispute_ID" });
            DropForeignKey("dbo.Payments", "Dispute_ID", "dbo.Disputes");
            DropTable("dbo.UserProfileJudgments");
            DropTable("dbo.Payments");
            DropTable("dbo.Disputes");
        }
    }
}
