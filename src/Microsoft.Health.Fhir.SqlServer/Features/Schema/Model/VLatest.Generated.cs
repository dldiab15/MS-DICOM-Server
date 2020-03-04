//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Microsoft.Health.Fhir.SqlServer.Features.Schema.Model
{
    internal class VLatest
    {
        internal readonly static SchemaVersionTable SchemaVersion = new SchemaVersionTable();
        internal readonly static SelectCurrentSchemaVersionProcedure SelectCurrentSchemaVersion = new SelectCurrentSchemaVersionProcedure();
        internal readonly static UpsertSchemaVersionProcedure UpsertSchemaVersion = new UpsertSchemaVersionProcedure();
        internal class SchemaVersionTable : Table
        {
            internal SchemaVersionTable(): base("dbo.SchemaVersion")
            {
            }

            internal readonly IntColumn Version = new IntColumn("Version");
            internal readonly VarCharColumn Status = new VarCharColumn("Status", 10);
        }

        internal class SelectCurrentSchemaVersionProcedure : StoredProcedure
        {
            internal SelectCurrentSchemaVersionProcedure(): base("dbo.SelectCurrentSchemaVersion")
            {
            }

            public void PopulateCommand(global::System.Data.SqlClient.SqlCommand command)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.SelectCurrentSchemaVersion";
            }
        }

        internal class UpsertSchemaVersionProcedure : StoredProcedure
        {
            internal UpsertSchemaVersionProcedure(): base("dbo.UpsertSchemaVersion")
            {
            }

            private readonly ParameterDefinition<System.Int32> _version = new ParameterDefinition<System.Int32>("@version", global::System.Data.SqlDbType.Int, false);
            private readonly ParameterDefinition<System.String> _status = new ParameterDefinition<System.String>("@status", global::System.Data.SqlDbType.VarChar, false, 10);
            public void PopulateCommand(global::System.Data.SqlClient.SqlCommand command, System.Int32 version, System.String status)
            {
                command.CommandType = global::System.Data.CommandType.StoredProcedure;
                command.CommandText = "dbo.UpsertSchemaVersion";
                _version.AddParameter(command.Parameters, version);
                _status.AddParameter(command.Parameters, status);
            }
        }
    }
}