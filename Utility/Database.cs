using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;

namespace QA_LISSummary.Utility
{
    public class Database
    {
        private SqlConnectionStringBuilder connectionStringBuilder;
        private SqlConnection _connection;
        public SqlConnection Connection { get { return _connection; } }

        public enum Source
        {
            REDBOW
        }

        public enum Catalog
        {
            Thailis,
            ThailisArc
        }

        public Database(Source source, Catalog catalog)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.Clear();
            switch (source)
            {
                case Source.REDBOW:
                    {
                        connectionStringBuilder.DataSource = "redbow";
                        connectionStringBuilder.UserID = "thrftest";
                        connectionStringBuilder.Password = "thrftest";
                        break;
                    }
            };
            switch (catalog)
            {
                case Catalog.Thailis:
                    {

                        connectionStringBuilder.InitialCatalog = "Thailis";
                        break;
                    }
                case Catalog.ThailisArc:
                    {
                        connectionStringBuilder.InitialCatalog = "ThailisArc";
                        break;
                    }
            }


            _connection = new SqlConnection();
            _connection.ConnectionString = connectionStringBuilder.ConnectionString;

        }

        public Database(string server, string username, string password, string databaseName)
        {
            connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.Clear();
            connectionStringBuilder.DataSource = server;
            connectionStringBuilder.UserID = username;
            connectionStringBuilder.Password = password;
            connectionStringBuilder.InitialCatalog = databaseName;
            _connection = new SqlConnection();
            _connection.ConnectionString = connectionStringBuilder.ConnectionString;
        }

        public DateTime? ServerDateTime()
        {
            DateTime? result = new DateTime(1991, 1, 1);
            using (var cmd = new SqlCommand("SELECT GETDATE() AS servDateTime", _connection))
            {
                cmd.Connection.Open();
                result = (cmd.ExecuteScalar() ?? new DateTime(1991, 1, 1)).ToTypeDateTIme();
                cmd.Connection.Close();
            }
            return result;
        }

        public DataTable DoQuery(string sql)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            return DoQuery(sql, sqlParameters.ToArray());
        }

        public DataTable DoQuery(string sql, SqlParameter[] sqlParameters)
        {
            DataTable result = new DataTable("result");
            using (var sqlAdp = new SqlDataAdapter(sql, _connection))
            {
                sqlAdp.SelectCommand.Parameters.AddRange(sqlParameters);
                sqlAdp.Fill(result);
            }
            return result;
        }
    }
}