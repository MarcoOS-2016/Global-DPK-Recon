using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using log4net;

namespace GIC.DataAccess
{
    public class MSAccessDataBaseAccess : IDisposable
    {        
        private static ILog log = LogManager.GetLogger(typeof(MSAccessDataBaseAccess));

        protected Int64 affectrow = 0;
        protected OleDbConnection connection = null;

        public Int64 AffectRow
        {
            get
            {
                return this.affectrow;
            }
        }

        public OleDbConnection Connection
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

        public MSAccessDataBaseAccess()
        {
            connection = new OleDbConnection();
            connection.ConnectionString = ConfigurationManager.AppSettings["MSAccessConnection"];

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

        public MSAccessDataBaseAccess(string connectionstring)
        {
            connection = new OleDbConnection();
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

        public Int64 ExecuteNonQuery(string sql)
        {
            OleDbCommand cmd = new OleDbCommand(sql, this.connection);
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

                log.Error(string.Format(sql, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
            }

            return this.affectrow;
        }

        public OleDbDataReader ExecuteReader(string sql)
        {
            OleDbCommand cmd = new OleDbCommand(sql, this.connection);
            cmd.CommandType = CommandType.Text;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                OleDbDataReader reader = cmd.ExecuteReader();

                return reader;
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
            }
        }

        public DataSet ExecuteQuery(string sql)
        {
            DataSet ds = null;
            OleDbDataAdapter adapter = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                adapter = new OleDbDataAdapter(sql, connection);
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
                adapter.Dispose();
            }

            return ds;
        }

        public Int64 ExecuteProcedureNonQuery(string procedurename)
        {
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procedurename;

            try
            {
                this.affectrow = cmd.ExecuteNonQuery();
            }

            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(procedurename, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
            }

            return this.affectrow;
        }

        public DataSet ExecuteProcedure(string procedurename)
        {
            DataSet ds = new DataSet();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procedurename;

            try
            {
                //cmd.ExecuteNonQuery();
                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                adapter.Fill(ds);
            }

            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(procedurename, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
            }

            return ds;
        }

        public DataSet ExecuteProcedure(string procedurename, IDataParameter[] parameters)
        {
            if (parameters == null)
                throw new Exception("Parameters can't be null when execute a stored proc which takes arguments.");

            DataSet ds = new DataSet();
            OleDbCommand cmd = new OleDbCommand();

            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procedurename;

            //cmd.Parameters.Clear();            

            foreach (OleDbParameter p in parameters)
            {
                cmd.Parameters.Add(p);
            }

            try
            {
                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                adapter.Fill(ds);
            }

            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(procedurename, e.Message));
                throw e;
            }

            finally
            {
                cmd.Dispose();
            }

            return ds;
        }

        public Int64 ExecuteQuery(string procedurename, IDataParameter[] parameters)
        {
            if (parameters == null)
                throw new Exception("parameters can't be null when execute a stored proc which takes arguments.");

            OleDbCommand cmd = new OleDbCommand();
            BuildQueryCommand(cmd, parameters);

            try
            {
                this.affectrow = cmd.ExecuteNonQuery();
            }

            finally
            {
                cmd.Dispose();
            }

            return this.affectrow;
        }

        private OleDbCommand BuildQueryCommand(OleDbCommand command, IDataParameter[] parameters)
        {
            command.Connection = connection;
            command.CommandType = CommandType.StoredProcedure;

            foreach (OleDbParameter p in parameters)
            {
                command.Parameters.Add(p);
            }

            if (this.connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            return command;
        }

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
                if (this.connection.State != ConnectionState.Closed)
                {
                    this.connection.Close();
                }

                this.connection.Dispose();
                this.connection = null;
            }

            // Set large fields to null.
        }

        ~MSAccessDataBaseAccess()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                log.Error("Connection is supposed to be closed by clients instead of waiting for GC.");
            }

            Dispose(false);
        }
    }
}
