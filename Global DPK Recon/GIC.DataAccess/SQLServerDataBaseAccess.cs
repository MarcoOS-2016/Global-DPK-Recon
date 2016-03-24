using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace GIC.DataAccess
{
    public class SqlServerDataBaseAccess : IDisposable
    {
        private static ILog log = LogManager.GetLogger(typeof(SqlServerDataBaseAccess));
        //private string connectionString = String.Empty;
        protected int affectrow = 0;
        protected SqlConnection connection = null;

        public int AffectRow
        {
            get
            {
                return this.affectrow;
            }
        }

        public SqlConnection Connection
        {
            get
            {
                return this.connection;
            }
            set
            {
                this.connection = value;
            }
        }

        //public SqlServerDataBaseAccess()
        //{
        //    connection = new SqlConnection();
        //    connection.ConnectionString = ConfigurationManager.AppSettings["SqlConnection"];

        //    try
        //    {
        //        connection.Open();
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(e.Message);
        //        throw e;
        //    }
        //}

        public SqlServerDataBaseAccess(string connectionstring)
        {
            connection = new SqlConnection();
            connection.ConnectionString = connectionstring;

            try
            {
                connection.Open();
            }

            catch (Exception e)
            {
                log.Error(e.Message);
                throw e;
            }
        }

        public int ExecuteNonQuery(string Sql)
        {
            SqlCommand cmd = new SqlCommand(Sql, this.connection);
            cmd.CommandType = CommandType.Text;

            try
            {
                if (this.connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                this.affectrow = cmd.ExecuteNonQuery();

            }

            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(Sql, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
            }

            return this.affectrow;
        }

        public SqlDataReader ExecuteReader(string Sql)
        {
            SqlCommand cmd = new SqlCommand(Sql, this.connection);
            cmd.CommandType = CommandType.Text;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                SqlDataReader reader = cmd.ExecuteReader();

                return reader;
            }

            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(Sql, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
            }
        }

        public DataSet ExecuteQuery(string sql)
        {
            DataSet ds = null;
            SqlDataAdapter adapter = null;
            SqlCommand cmd = new SqlCommand(sql, this.connection);

            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 0;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                ds = new DataSet();
                adapter.Fill(ds);
            }
            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(sql, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
                adapter.Dispose();
            }

            return ds;
        }

        //public DataSet ExecuteQuery(string Sql)
        //{
        //    DataSet ds = null;
        //    SqlDataAdapter adapter = null;

        //    try
        //    {
        //        if (connection.State != ConnectionState.Open)
        //        {
        //            connection.Open();                    
        //        }

        //        adapter = new SqlDataAdapter(Sql, connection);
        //        ds = new DataSet();
        //        adapter.Fill(ds);
        //    }

        //    catch (Exception e)
        //    {
        //        if (connection.State == ConnectionState.Open)
        //        {
        //            connection.Close();
        //            connection.Dispose();
        //        }

        //        log.Error(string.Format(Sql, e.Message));
        //        throw e;
        //    }

        //    finally
        //    {
        //        adapter.Dispose();
        //    }

        //    return ds;
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed objects.
            }
            // Free unmanaged objects.

            if (this.connection != null)
            {
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }

            // Set large fields to null.
        }

        ~SqlServerDataBaseAccess()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                log.Error("Connection is supposed to be closed by clients instead of waiting for GC.");
            }

            Dispose(false);
        }
    }
}
