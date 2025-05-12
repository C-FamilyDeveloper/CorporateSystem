using FluentMigrator;

namespace CorporateSystem.SharedDocs.Infrastructure.Migrations;

[Migration(20250508130200)]
public class AddForeignKeyToDocumentUsers : Migration
{
    private string ForeignKey { get; } = "fk_document_users_document_id";
    
    public override void Up()
    {
        Execute.Sql(@$"
            do $$
            begin
                if not exists (
                    select 1
                    from information_schema.table_constraints tc
                    join information_schema.key_column_usage kcu
                        on tc.constraint_name = kcu.constraint_name
                    where tc.constraint_type = 'FOREIGN KEY'
                      and tc.constraint_name = '{ForeignKey}'
                      and tc.table_name = 'document_users'
                ) then
                    alter table document_users
                    add constraint {ForeignKey}
                    foreign key (document_id) references documents(id)
                    on delete cascade;
                end if;
            end $$;
        ");
    }

    public override void Down()
    {
        Execute.Sql(@$"
            do $$
            begin
                if exists (
                    select 1
                    from information_schema.table_constraints tc
                    where tc.constraint_type = 'FOREIGN KEY'
                      and tc.constraint_name = '{ForeignKey}'
                      and tc.table_name = 'document_users'
                ) then
                    alter table document_users
                    drop constraint {ForeignKey};
                end if;
            end $$;
        ");
    }
}