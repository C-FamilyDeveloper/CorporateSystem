using FluentMigrator;

namespace CorporateSystem.SharedDocs.Infrastructure.Migrations;

[Migration(20250513171900)]
public class AddDocumentChangeLogTableMigration : Migration
{
    public override void Up()
    {
        Create.Table("document_change_logs")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("document_id").AsInt32().NotNullable()
            .WithColumn("changed_at").AsDateTimeOffset().NotNullable()
            .WithColumn("changes").AsString().NotNullable()
            .WithColumn("line").AsInt32().NotNullable();
        
        Create.ForeignKey("fk_document_change_logs_document_id")
            .FromTable("document_change_logs").ForeignColumn("document_id")
            .ToTable("documents").PrimaryColumn("id");
        
        Create.Index("ix_document_change_logs_user_id").OnTable("document_change_logs").OnColumn("user_id");
        Create.Index("ix_document_change_logs_document_id").OnTable("document_change_logs").OnColumn("document_id");
    }

    public override void Down()
    {
        Delete.Index("ix_document_change_logs_document_id").OnTable("document_change_logs");
        Delete.Index("ix_document_change_logs_user_id").OnTable("document_change_logs");
        
        Delete.ForeignKey("fk_document_change_logs_document_id").OnTable("document_change_logs");
        
        Delete.Table("document_change_logs");
    }
}