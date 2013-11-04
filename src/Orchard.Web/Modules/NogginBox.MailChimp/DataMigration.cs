using System;
using System.Data;
using NogginBox.MailChimp.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace NogginBox.MailChimp
{
	public class DataMigration : DataMigrationImpl
	{
		public int Create()
		{
			#region Table creation

			// SettingsRecord Table
			SchemaBuilder.CreateTable("SettingsRecord", table => table
				.ContentPartRecord()
				.Column("ApiKey", DbType.String)
			);

			// FormRecord Table
			SchemaBuilder.CreateTable("FormRecord", table => table
				.ContentPartRecord()
				.Column("ListId", DbType.String)
				.Column("Message", DbType.String)
				.Column("ThankyouMessage", DbType.String)
			);

			// MergeVariableRecord table
			SchemaBuilder.CreateTable("MergeVariableRecord", table => table
				.Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
				.Column("Tag", DbType.String)
				.Column("Label", DbType.String)
				.Column("Type", DbType.Byte)
				.Column("Required", DbType.Boolean)
				.Column("DisplayOrder", DbType.Int32)
				.Column("Choices", DbType.String)
				.Column("FormRecord_Id", DbType.Int32)
			);

			// InterestGroupingRecord
			SchemaBuilder.CreateTable("InterestGroupingsRecord", table => table
				.Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
				.Column("GroupId", DbType.Int32)
				.Column("Name", DbType.String)
				.Column("Type", DbType.String)
				.Column("Groups", DbType.String)
				.Column("Show", DbType.Boolean)
				.Column("FormRecord_Id", DbType.Int32)
			);

			#endregion

			ContentDefinitionManager.AlterPartDefinition(typeof(MailChimpFormPart).Name, cfg => cfg
				.Attachable());

			ContentDefinitionManager.AlterTypeDefinition("Mailchimp Subscribe Page", cfg => cfg
				.WithPart("TitlePart")
				.WithPart("AutoroutePart", builder => builder
						.WithSetting("AutorouteSettings.AllowCustomPattern", "true")
						.WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
						.WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-place'}]")
						.WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
				.WithPart("MenuPart")
				.WithPart("CommonPart")
				.WithPart("MailChimpFormPart")
				.WithPart("LocalizationPart")
				.Creatable()
			);

			return 1;
		}

		public int UpdateFrom1()
		{
			ContentDefinitionManager.AlterTypeDefinition("Mailchimp Subscribe Widget", cfg => cfg
				.WithPart("CommonPart")
				.WithPart("MailChimpFormPart")
				.WithPart("WidgetPart")
				.WithSetting("Stereotype", "Widget")
			);
			return 2;
		}
	}
}