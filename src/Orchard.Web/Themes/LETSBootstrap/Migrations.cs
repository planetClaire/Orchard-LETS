using Orchard.Data.Migration;

namespace LETSBootstrap
{
    public class Migrations : DataMigrationImpl
    {

        public int Create()
        {
            SchemaBuilder.CreateTable("SettingsRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Logo")
                );
            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable("SettingsRecord", t => t.AddColumn<string>("Tagline"));

            return 2;
        }

    }
}