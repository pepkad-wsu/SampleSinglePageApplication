namespace SampleSinglePageApplication;

public class DataMigrations
{
    public List<DataObjects.DataMigration> GetMigrations()
    {
        List<DataObjects.DataMigration> output = new List<DataObjects.DataMigration>();

        // Build the initial database
        List<string> m1 = new List<string>();

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='DataMigrations')" + Environment.NewLine +
            "CREATE TABLE [dbo].[DataMigrations](" + Environment.NewLine +
            "  [MigrationId] [int] NOT NULL," + Environment.NewLine +
            "  [MigrationDate] [datetime] NULL," + Environment.NewLine +
            "  [MigrationApplied] [datetime] NULL," + Environment.NewLine +
            "  [Migration] [ntext] NULL," + Environment.NewLine +
            "  CONSTRAINT [PK_DataMigrations] PRIMARY KEY CLUSTERED ([MigrationId] ASC)" + Environment.NewLine +
            "  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) " + Environment.NewLine +
            "  ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='DepartmentGroups')" + Environment.NewLine +
            "CREATE TABLE [dbo].[DepartmentGroups](" + Environment.NewLine +
            "  [DepartmentGroupId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "  [DepartmentGroupName] [nvarchar](200) NULL," + Environment.NewLine +
            "  [TenantId] [uniqueidentifier] NULL," + Environment.NewLine +
            "  CONSTRAINT [PK_DepartmentGroups] PRIMARY KEY CLUSTERED ([DepartmentGroupId] ASC )" + Environment.NewLine +
            "  WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) " + Environment.NewLine +
            "  ON [PRIMARY]) ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Departments')" + Environment.NewLine +
            "CREATE TABLE [dbo].[Departments](" + Environment.NewLine +
            "  	[DepartmentId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "	[DepartmentName] [nvarchar](100) NULL," + Environment.NewLine +
            "	[ActiveDirectoryNames] [nvarchar](100) NULL," + Environment.NewLine +
            "	[Enabled] [bit] NULL," + Environment.NewLine +
            "	[DepartmentGroupId] [uniqueidentifier] NULL," + Environment.NewLine +
            "	[TenantId] [uniqueidentifier] NULL," + Environment.NewLine +
            "    CONSTRAINT [PK_Departments] PRIMARY KEY CLUSTERED ([DepartmentId] ASC )" + Environment.NewLine +
            "	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "	ON [PRIMARY]) ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='FileStorage')" + Environment.NewLine +
            "CREATE TABLE [dbo].[FileStorage](" + Environment.NewLine +
            "  	[FileId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "	[ItemId] [uniqueidentifier] NULL," + Environment.NewLine +
            "	[FileName] [nvarchar](255) NULL," + Environment.NewLine +
            "	[Extension] [nvarchar](15) NULL," + Environment.NewLine +
            "	[Bytes] [bigint] NULL," + Environment.NewLine +
            "	[Value] [varbinary](max) NULL," + Environment.NewLine +
            "	[UploadDate] [datetime] NULL," + Environment.NewLine +
            "	[UserId] [uniqueidentifier] NULL," + Environment.NewLine +
            "	[SourceFileId] [nvarchar](100) NULL," + Environment.NewLine +
            "	[TenantId] [uniqueidentifier] NULL," + Environment.NewLine +
            "    CONSTRAINT [PK_FileStorage] PRIMARY KEY CLUSTERED ([FileId] ASC)" + Environment.NewLine +
            "	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "	ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Settings')" + Environment.NewLine +
            "CREATE TABLE [dbo].[Settings](" + Environment.NewLine +
            "	[SettingId] [int] IDENTITY(1,1) NOT NULL," + Environment.NewLine +
            "	[SettingName] [nvarchar](100) NOT NULL," + Environment.NewLine +
            "	[SettingType] [nvarchar](100) NULL," + Environment.NewLine +
            "	[SettingNotes] [nvarchar](max) NULL," + Environment.NewLine +
            "	[SettingText] [nvarchar](max) NULL," + Environment.NewLine +
            "	[TenantId] [uniqueidentifier] NULL," + Environment.NewLine +
            "	[UserId] [uniqueidentifier] NULL," + Environment.NewLine +
            "    CONSTRAINT [PK_Settings_1] PRIMARY KEY CLUSTERED ([SettingId] ASC)" + Environment.NewLine +
            "	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "	ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Tenants')" + Environment.NewLine +
            "CREATE TABLE [dbo].[Tenants](" + Environment.NewLine +
            "  	[TenantId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "	[Name] [nvarchar](200) NOT NULL," + Environment.NewLine +
            "	[TenantCode] [nvarchar](50) NOT NULL," + Environment.NewLine +
            "	[Enabled] [bit] NOT NULL," + Environment.NewLine +
            "    CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED ([TenantId] ASC)" + Environment.NewLine +
            "	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "	ON [PRIMARY]) ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='UDFLabels')" + Environment.NewLine +
            "CREATE TABLE [dbo].[UDFLabels](" + Environment.NewLine +
            "	[Id] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "	[Module] [nvarchar](20) NULL," + Environment.NewLine +
            "	[UDF] [nvarchar](10) NULL," + Environment.NewLine +
            "	[Label] [nvarchar](max) NULL," + Environment.NewLine +
            "	[ShowColumn] [bit] NULL," + Environment.NewLine +
            "	[ShowInFilter] [bit] NULL," + Environment.NewLine +
            "	[IncludeInSearch] [bit] NULL," + Environment.NewLine +
            "	[TenantId] [uniqueidentifier] NULL," + Environment.NewLine +
            "   CONSTRAINT [PK_UDFLabels] PRIMARY KEY CLUSTERED ([Id] ASC)" + Environment.NewLine +
            "   WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "    ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

        m1.Add("SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Users')" + Environment.NewLine +
            "CREATE TABLE [dbo].[Users](" + Environment.NewLine +
            "  	[UserId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "	[TenantId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "	[FirstName] [nvarchar](100) NULL," + Environment.NewLine +
            "	[LastName] [nvarchar](100) NULL," + Environment.NewLine +
            "	[Email] [nvarchar](100) NULL," + Environment.NewLine +
            "	[Phone] [nvarchar](20) NULL," + Environment.NewLine +
            "	[Username] [nvarchar](100) NOT NULL," + Environment.NewLine +
            "	[EmployeeId] [nvarchar](50) NULL," + Environment.NewLine +
            "	[DepartmentId] [uniqueidentifier] NULL," + Environment.NewLine +
            "	[Title] [nvarchar](255) NULL," + Environment.NewLine +
            "	[Location] [nvarchar](255) NULL," + Environment.NewLine +
            "	[Enabled] [bit] NULL," + Environment.NewLine +
            "	[LastLogin] [datetime] NULL," + Environment.NewLine +
            "	[Admin] [bit] NULL," + Environment.NewLine +
            "	[Password] [nvarchar](max) NULL," + Environment.NewLine +
            "	[PreventPasswordChange] [bit] NULL," + Environment.NewLine +
            "	[FailedLoginAttempts] [int] NULL," + Environment.NewLine +
            "	[LastLockoutDate] [datetime] NULL," + Environment.NewLine +
            "   [UDF01] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF02] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF03] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF04] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF05] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF06] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF07] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF08] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF09] [nvarchar](500) NULL," + Environment.NewLine +
            "   [UDF10] [nvarchar](500) NULL," + Environment.NewLine +
            "    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserId] ASC)" + Environment.NewLine +
            "	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "	ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

        m1.Add(
            "IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='FK_FileStorage_Users')" + Environment.NewLine +
            "  BEGIN ALTER TABLE[dbo].[FileStorage]  WITH CHECK ADD CONSTRAINT[FK_FileStorage_Users] FOREIGN KEY([UserId])" + Environment.NewLine +
            "  REFERENCES[dbo].[Users]([UserId])" + Environment.NewLine +
            "  ALTER TABLE[dbo].[FileStorage] CHECK CONSTRAINT[FK_FileStorage_Users]" + Environment.NewLine +
            "END");

        m1.Add(
            "IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME='FK_Users_Departments')" + Environment.NewLine +
            "  BEGIN ALTER TABLE[dbo].[Users] WITH CHECK ADD CONSTRAINT[FK_Users_Departments] FOREIGN KEY([DepartmentId])" + Environment.NewLine +
            "  REFERENCES[dbo].[Departments]([DepartmentId])" + Environment.NewLine +
            "  ALTER TABLE[dbo].[Users] CHECK CONSTRAINT[FK_Users_Departments]" + Environment.NewLine +
            "END");

        var m2 = new List<string>();
        m2.Add(
            "SET ANSI_NULLS ON;" + Environment.NewLine +
            "SET QUOTED_IDENTIFIER ON;" + Environment.NewLine +
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Records')" + Environment.NewLine +
            "CREATE TABLE [dbo].[Records](" + Environment.NewLine +
            "   [RecordId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "   [Name] [nvarchar](100) NOT NULL," + Environment.NewLine +
            "   [Number] [int] NULL," + Environment.NewLine +
            "   [Boolean] [bit] NULL," + Environment.NewLine +
            "   [Text] [nvarchar](max) NULL," + Environment.NewLine +
            "   [TenantId] [uniqueidentifier] NULL," + Environment.NewLine +
            "   [UserId] [uniqueidentifier] NOT NULL," + Environment.NewLine +
            "   CONSTRAINT [PK_Records_1] PRIMARY KEY CLUSTERED ([RecordId] ASC)" + Environment.NewLine +
            "   WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)" + Environment.NewLine +
            "   ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]");

        m2.Add(
            "IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_Records_Users')" + Environment.NewLine +
            "BEGIN ALTER TABLE[dbo].[Records]  WITH CHECK ADD CONSTRAINT[FK_Records_Users] FOREIGN KEY([UserId])" + Environment.NewLine +
            "REFERENCES[dbo].[Users]([UserId])" + Environment.NewLine +
            "ALTER TABLE[dbo].Records CHECK CONSTRAINT[FK_Records_Users]" + Environment.NewLine +
            "END");

        output.Add(new DataObjects.DataMigration {
            MigrationId = 1,
            MigrationDate = DateTime.Now,
            Migration = m1
        });

        output.Add(new DataObjects.DataMigration
        {
            MigrationId = 2,
            MigrationDate = DateTime.Now,
            Migration = m2
        });

        return output;
    }
}