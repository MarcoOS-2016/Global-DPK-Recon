using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace GIC.DataAccess
{
    public class SQLServerAccessDAO : SqlServerDataBaseAccess
    {
        private string connectionstring = string.Empty;

        public SQLServerAccessDAO(string connectionstring)
            : base(connectionstring)
        {
            this.connectionstring = connectionstring;

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

        public void BulkWriteToServer(DataTable datatable, string tablename)
        {
            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(this.connection))
            {
                bulkcopy.DestinationTableName = tablename;

                try
                {
                    bulkcopy.WriteToServer(datatable);
                }
                catch
                {
                    throw;
                }
            }
        }

        public bool CheckFileNameHistory(string filename)
        {
            string sqlstring = string.Format("SELECT * FROM Report_Uploading_Log WHERE FileName = '{0}'", filename);
            SqlDataReader reader = this.ExecuteReader(sqlstring);

            return reader.HasRows;
        }

        public DataTable GetUnProcessedReportFileNameList()
        {
            string sqlstring = string.Format("SELECT * FROM Report_File_Processing_Log WHERE Status = 'Unprocessed' Order by FileCreatedDate");
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public DataTable GetVarianceItem()
        {
            string sqlstring = "SELECT * FROM View_Variance_Items ORDER BY lkm_ccn";
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public DataTable GetLatestGLVTransDate()
        {
            string sqlstring = "SELECT Latest_GLV_Trans_Date FROM Last_Recon_Date";
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public DataTable GetMaxGLVTransDate()
        {
            string sqlstring = "SELECT Max(glv_trans_date_aus) FROM Glovia_To_LKM_Transaction";
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public DataTable GetLKMDPKTransactionHistory(string ccn, string serialkeynumber, string partnumber, string fromstatus, 
            string tostatus, string fromdate, string todate)
        {
            string sqlstring = string.Format("SELECT * FROM LKM_TO_GLOVIA_TRANSACTION "
                                           + "WHERE (LKM_CCN = '{0}' OR 'NULL' = '{1}') "
                                             + "AND (LKM_DPK_SN IN ({2}) OR 'NULL' IN ({3})) " 
                                             + "AND (LKM_PN IN ({4}) OR 'NULL' IN ({5})) "
                                             + "AND (LKM_FROM_STATUS = '{6}' OR 'NULL' = '{7}') "
                                             + "AND (LKM_TO_STATUS = '{8}' OR 'NULL' = '{9}') "
                                             + "AND (GLV_TRANS_DATE_AUS BETWEEN '{10}' AND '{11}')",
                                             ccn, ccn, 
                                             serialkeynumber, serialkeynumber, 
                                             partnumber, partnumber,
                                             fromstatus, fromstatus,
                                             tostatus, tostatus,
                                             fromdate, todate);

            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public DataTable GetGloviaDPKTransactionHistory(string ccn, string serialkeynumber, string partnumber, string fromstatus,
            string tostatus, string fromdate, string todate)
        {
            string sqlstring = string.Format("SELECT * FROM GLOVIA_TO_LKM_TRANSACTION "
                                           + "WHERE (GLV_CCN = '{0}' OR 'NULL' = '{1}') "
                                             + "AND (GLV_DPK_SN IN ({2}) OR 'NULL' IN ({3})) "
                                             + "AND (GLV_PN IN ({4}) OR 'NULL' IN ({5})) "
                                             + "AND (LKM_FROM_STATUS = '{6}' OR 'NULL' = '{7}') "
                                             + "AND (LKM_TO_STATUS = '{8}' OR 'NULL' = '{9}') "
                                             + "AND (GLV_TRANS_DATE_AUS BETWEEN '{10}' AND '{11}')",
                                             ccn, ccn,
                                             serialkeynumber, serialkeynumber,
                                             partnumber, partnumber,
                                             fromstatus, fromstatus,
                                             tostatus, tostatus,
                                             fromdate, todate);

            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public void InsertVarianceItem()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Variance_Item";

            try
            {   
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                cmd.ExecuteNonQuery();

                //transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public void InsertVarianceHistory(string glvTransDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 300;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Variance_History";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                //transaction = cmd.Connection.BeginTransaction();
                //cmd.Transaction = transaction;
                                
                cmd.Parameters.Add("GLVTransDate", SqlDbType.Char);
                cmd.Parameters["GLVTransDate"].Value = glvTransDate;

                cmd.ExecuteNonQuery();

                //transaction.Commit();
                cmd.Connection.Close();                
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public void RemoveHistoricalData()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 0;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Remove_Historical_Data";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                cmd.ExecuteNonQuery();

                //transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public void UpdateLastReconDate(string latestglvtransdate, string lastrecondate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 120;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Update_Last_Recon_Date";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                //transaction = cmd.Connection.BeginTransaction();
                //cmd.Transaction = transaction;

                cmd.Parameters.Add("LatestGLVTransDate", SqlDbType.Char);
                cmd.Parameters.Add("LastReconDate", SqlDbType.Char);

                cmd.Parameters["LatestGLVTransDate"].Value = latestglvtransdate;
                cmd.Parameters["LastReconDate"].Value = lastrecondate;

                cmd.ExecuteNonQuery();

                //transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public void UpdateReportFileProcessingLog(string reportname, string filename, string status)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 120;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Update_Report_File_Processing_Log";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                //transaction = cmd.Connection.BeginTransaction();
                //cmd.Transaction = transaction;

                cmd.Parameters.Add("ReportName", SqlDbType.Char);
                cmd.Parameters.Add("FileName", SqlDbType.Char);
                cmd.Parameters.Add("Status", SqlDbType.Char);
                cmd.Parameters.Add("UpdatedDate", SqlDbType.Char);

                cmd.Parameters["ReportName"].Value = reportname;
                cmd.Parameters["FileName"].Value = filename;
                cmd.Parameters["Status"].Value = status;
                cmd.Parameters["UpdatedDate"].Value = DateTime.Now.ToString();

                cmd.ExecuteNonQuery();

                //transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        // Insert pulling report log into the Pulling_Report_Log table
        public void InsertReportFileProcessingLog(string filename, long filelength, string filecreateddate, string status)
        {
            string currentdate = DateTime.Now.ToString();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandTimeout = 120;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Report_File_Processing_Log";

            try
            {
                if (filelength != 0)
                {
                    if (this.connection.State == System.Data.ConnectionState.Closed)
                        connection.Open();

                    //transaction = cmd.Connection.BeginTransaction();
                    //cmd.Transaction = transaction;

                    //cmd.Parameters.Add("ReportName", SqlDbType.Char);
                    cmd.Parameters.Add("FileName", SqlDbType.Char);
                    cmd.Parameters.Add("FileLength", SqlDbType.Int);
                    cmd.Parameters.Add("FileCreatedDate", SqlDbType.Char);
                    cmd.Parameters.Add("Status", SqlDbType.Char);
                    cmd.Parameters.Add("CreatedDate", SqlDbType.Char);
                    cmd.Parameters.Add("UpdatedDate", SqlDbType.Char);

                    //cmd.Parameters["ReportName"].Value = reportname;
                    cmd.Parameters["FileName"].Value = filename;
                    cmd.Parameters["FileLength"].Value = filelength;
                    cmd.Parameters["FileCreatedDate"].Value = filecreateddate;
                    cmd.Parameters["Status"].Value = status;
                    cmd.Parameters["CreatedDate"].Value = currentdate;
                    cmd.Parameters["UpdatedDate"].Value = currentdate;

                    cmd.ExecuteNonQuery();

                    //transaction.Commit();
                    cmd.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }
    }
}
