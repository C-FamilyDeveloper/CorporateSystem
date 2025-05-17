using System.Data;
using FluentMigrator;

namespace CorporateSystem.SharedDocs.Infrastructure.Migrations;

[Migration(20250515100300)]
public class FixForeignKeysMigration : Migration
{
    public override void Up()
    {
        Delete.ForeignKey("fk_document_change_logs_document_id").OnTable("document_change_logs");
        
        Create.ForeignKey("fk_document_change_logs_document_id_cascade")
            .FromTable("document_change_logs").ForeignColumn("document_id")
            .ToTable("documents").PrimaryColumn("id")
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        Delete.ForeignKey("fk_document_change_logs_document_id_cascade").OnTable("document_change_logs");
        
        Create.ForeignKey("fk_document_change_logs_document_id")
            .FromTable("document_change_logs").ForeignColumn("document_id")
            .ToTable("documents").PrimaryColumn("id");
    }
}