using FluentMigrator;

namespace CorporateSystem.SharedDocs.Infrastructure.Migrations;

[Migration(20250415112700)]
public class InitMigration : Migration
{
    public override void Up()
    {
        Create.Table("documents")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("owner_id").AsInt32().NotNullable()
            .WithColumn("title").AsString().NotNullable()
            .WithColumn("content").AsString().Nullable().WithDefaultValue("")
            .WithColumn("modified_at").AsDateTimeOffset().Nullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();
        
        Create.Table("document_users")
            .WithColumn("id").AsInt32().PrimaryKey().Identity()
            .WithColumn("document_id").AsInt32().NotNullable()
            .ForeignKey("fk_document_users_documents", "documents", "id").OnDelete(System.Data.Rule.Cascade)
            .WithColumn("user_id").AsInt32().NotNullable()
            .WithColumn("access_level").AsInt32().NotNullable();
        
        Create.Index("ix_documents_owner_id").OnTable("documents").OnColumn("owner_id");
        Create.Index("ix_document_users_document_id_user_id")
            .OnTable("document_users")
            .OnColumn("document_id").Ascending()
            .OnColumn("user_id").Ascending();
    }

    public override void Down()
    {
        Delete.Index("ix_document_users_document_id_user_id").OnTable("document_users");
        Delete.Index("ix_documents_owner_id").OnTable("documents");

        Delete.Table("document_users");
        Delete.Table("documents");
    }
}