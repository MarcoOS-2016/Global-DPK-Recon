using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace GIC.DataAccess
{
    public class MSAccessDAO : MSAccessDataBaseAccess
    {
        public MSAccessDAO()
        {
        }

        public MSAccessDAO(string connectionstring)
            : base(connectionstring)
        {
            try
            {
                if (base.connection.State == System.Data.ConnectionState.Closed)
                {
                    base.connection.Open();
                }
            }
            catch
            {
                throw;
            }
        }

        // Insert pulling report log into the Pulling_Report_Log table
        public void InsertGlobalDPKReconHistory(string reconDate, string glvTransDate, int totalVairanceItems, string reconResultLink)
        {
            string currentdate = DateTime.Now.ToShortDateString();

            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Global_DPK_Recon_History";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                cmd.Parameters.Add("ReconDate", OleDbType.Char);
                cmd.Parameters.Add("GLVTransDate", OleDbType.Char);
                cmd.Parameters.Add("TotalVairanceItems", OleDbType.Integer);
                cmd.Parameters.Add("ReconResultLink", OleDbType.Char);

                cmd.Parameters["ReconDate"].Value = reconDate;
                cmd.Parameters["GLVTransDate"].Value = glvTransDate;
                cmd.Parameters["TotalVairanceItems"].Value = totalVairanceItems;
                cmd.Parameters["ReconResultLink"].Value = reconResultLink;

                cmd.ExecuteNonQuery();

                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }
    }
}
