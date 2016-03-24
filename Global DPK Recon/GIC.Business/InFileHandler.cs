using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using GIC.Common;
using GIC.Common.Model;
using GIC.DataAccess;
using log4net;
using log4net.Config;

namespace GIC.Business
{
    public class InFileHandler
    {
        private static ILog log = LogManager.GetLogger(typeof(InFileHandler));
        public static readonly ReportingConfig ReportingConfig = FileUtility.LoadReportingConfig();

        private int TotalVairanceItems = 0;        
        private DateTime ReconDate;
        private DateTime GLVTransDate;

        private DataTable LKM2GloviaDataTable = null;
        private DataTable Glovia2LKMDataTable = null;
        private DataTable GloviaExceptDataTable = null;
        private DataTable UnprocessedFileNameTable = null;
        
        private string Connectionstring = ConfigFileUtility.GetValue("GIC_Database");
        private string ReconResultLink = string.Empty;

        //private DateTime CurrentDate;
        //public DateTime CurrentDate
        //{
        //    get { return CurrentDate; }
        //    set { CurrentDate = value; }
        //}

        public InFileHandler()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }

        public InFileHandler(DateTime curretdate)
        {
            //this.CurrentDate = CurrentDate;
        }

        public void Process()
        {
            MakeDataTableStructure();
            GetUnProcessedFileNameList();
            ReadReportFileContent();
            ExportReconResultReport();
            RemoveDPKHistoricalData();
            //TriggerNotification();
        }

        private void GetUnProcessedFileNameList()
        {
            DirectoryInfo dir = null;

            try
            {
                Console.WriteLine(string.Format("[{0}] - Starting to search all unprocessed report files...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                MiscUtility.LogHistory("Starting to search all unprocessed report files...");

                dir = new DirectoryInfo(ReportingConfig.DPKTranHistoryReports[0].SourceFolder);

                using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(Connectionstring)))
                {
                    // Insert report file name which is unprocessed into table                    
                    foreach (FileInfo fi in dir.GetFiles())
                    {
                        // If content of report file only contains titles without any row data
                        if (fi.Extension.ToUpper().Equals(".TXT") && fi.Length > 246)
                            dao.InsertReportFileProcessingLog(fi.Name, fi.Length, fi.LastWriteTime.ToShortDateString(), "Unprocessed");
                    }

                    UnprocessedFileNameTable = dao.GetUnProcessedReportFileNameList();
                }

                Console.WriteLine(string.Format("[{0}] - Total of {1} unprocessed report files found!", 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), UnprocessedFileNameTable.Rows.Count.ToString()));
                MiscUtility.LogHistory(string.Format("Total of {0} unprocessed report files found!", UnprocessedFileNameTable.Rows.Count));
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(string.Format("Function name: <GetUnProcessedFileNameList>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }
        }

        // Read content of DPK report file
        private void ReadReportFileContent()
        {
            string reportContent = string.Empty;
            //string currentdate = this.CurrentDate.ToShortDateString();

            //try
            //{
                for (int index = 0; index < UnprocessedFileNameTable.Rows.Count; index++)
                {
                    long fileLength = Convert.ToInt64(UnprocessedFileNameTable.Rows[index][3]);
                    string fileName = UnprocessedFileNameTable.Rows[index][0].ToString();

                    for (int indey = 0; indey < ReportingConfig.DPKTranHistoryReports.Length; indey++)
                    {
                        if (fileName.ToUpper().Contains(ReportingConfig.DPKTranHistoryReports[indey].KeyCharsInFileName))
                        {
                            Console.WriteLine(string.Format("[{0}] - Starting to read {1} report file...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileName));
                            MiscUtility.LogHistory(string.Format("Starting to read {0} report file...", fileName));

                            string fullName = Path.Combine(ReportingConfig.DPKTranHistoryReports[0].SourceFolder, fileName);
                            FillDataIntoDataTable(ReportingConfig.DPKTranHistoryReports[indey].ReportName, fullName);

                            Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                            MiscUtility.LogHistory("Done!");

                            WriteToDatabase(ReportingConfig.DPKTranHistoryReports[indey].ReportName, ReportingConfig.DPKTranHistoryReports[indey].TargetTableName);
                            UpdateReportFileProcessingStatus(ReportingConfig.DPKTranHistoryReports[indey].ReportName, fileName, "Processed");
                        }
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    MiscUtility.LogHistory(string.Format("Function name: <ReadReportContent>, Source:{0},  Error:{1}", ex.Source, ex.Message));
            //    throw;
            //}
        }

        #region discard code
        //// Read content of DPK report
        //private void ReadReportFileContent()
        //{
        //    DirectoryInfo dir = null;
        //    string reportContent = string.Empty;
        //    string currentdate = this.CurrentDate.ToShortDateString();
            
        //    try
        //    {
        //        dir = new DirectoryInfo(reportingconfig.DPKTranHistoryReports[0].SourceFolder);
        //        foreach (FileInfo fi in dir.GetFiles())                
        //        {
        //            if (fi.LastWriteTime.ToShortDateString().Equals(currentdate))
        //            {
        //                // If the report is uploaded into database before, then skip it.
        //                if (IsFileUploaded(fi.Name))
        //                {
        //                    MiscUtility.LogHistory(string.Format("[{0}] - The report file - {1} is uploaded into database before, will skip it.",
        //                               DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fi.Name));
        //                    continue;
        //                }

        //                // If content of report file only contains titles without any row data
        //                if (fi.Length < 247) continue;

        //                for (int index = 0; index < reportingconfig.DPKTranHistoryReports.Length; index++)
        //                {
        //                    if (fi.Name.ToUpper().Contains(reportingconfig.DPKTranHistoryReports[index].KeyCharsInFileName))
        //                    {                                
        //                        Console.WriteLine(string.Format("[{0}] - Starting to read {1} report file...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fi.Name));
        //                        MiscUtility.LogHistory(string.Format("[{0}] - Starting to read {1} report file...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fi.Name));

                                
        //                        FillDataIntoDataTable(reportingconfig.DPKTranHistoryReports[index].ReportName, fi.FullName);

        //                        Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        //                        MiscUtility.LogHistory(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                                
        //                        WriteToDatabase(reportingconfig.DPKTranHistoryReports[index].ReportName, reportingconfig.DPKTranHistoryReports[index].TargetTableName);
        //                        UpdateReportFileProcessingStatus(reportingconfig.DPKTranHistoryReports[index].ReportName, fi.Name, "Processed");
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MiscUtility.LogHistory(string.Format("Function name: <ReadReportContent>, Source:{0},  Error:{1}", ex.Source, ex.Message));
        //        throw;
        //    }
        //}
        #endregion

        // Log pulling report record into MS SQLServer database
        private void UpdateReportFileProcessingStatus(string reportname, string filename, string status)
        {
            using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(Connectionstring)))
            {
                dao.UpdateReportFileProcessingLog(reportname, filename, status);
            }
        }

        // Write DPK raw data into SQL server
        private void WriteToDatabase(string reportname, string tablename)
        {
            try
            {
                Console.WriteLine(string.Format("[{0}] - Starting to write {1} report into SQL Server...",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), reportname));

                MiscUtility.LogHistory(string.Format("Starting to write {0} report into SQL Server...", reportname));

                using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(Connectionstring)))
                {
                    if (reportname.ToUpper().Equals("LKM_TO_GLOVIA") && LKM2GloviaDataTable.Rows.Count != 0)
                    {
                        dao.BulkWriteToServer(LKM2GloviaDataTable, tablename);
                        LKM2GloviaDataTable.Clear();                        
                    }
                    else if (reportname.ToUpper().Equals("GLOVIA_TO_LKM") && Glovia2LKMDataTable.Rows.Count != 0)
                    {
                        dao.BulkWriteToServer(Glovia2LKMDataTable, tablename);
                        Glovia2LKMDataTable.Clear();
                    }
                    else if (reportname.ToUpper().Equals("GLOVIA_TO_LKM_EXCEPTION") && GloviaExceptDataTable.Rows.Count != 0)
                    {
                        dao.BulkWriteToServer(GloviaExceptDataTable, tablename);
                        GloviaExceptDataTable.Clear();
                    }
                }

                Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                MiscUtility.LogHistory("Done!");
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(string.Format("Function name: <WriteToDatabase>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }
        }

        // Fill raw data into data table
        private void FillDataIntoDataTable(string reportname, string fullname)
        {
            DataRow dataRow = null;
            DateTime currentDay = DateTime.Now.Date;
            List<string> fieldValueList = new List<string>();

            //try
            //{
                string text = string.Empty;
                using (StreamReader reader = new FileInfo(fullname).OpenText())
                {
                    while ((text = reader.ReadLine()) != null)
                    {
                        foreach (string tempString in text.Split('|'))
                        {
                            fieldValueList.Add(tempString.Trim());
                        }
                    }

                    reader.Close();
                }
                                
                if (reportname.ToUpper().Equals("LKM_TO_GLOVIA"))
                {
                    // index #0 ~ 17 are field names
                    for (int index = 18; index < fieldValueList.Count; index += 18)
                    {
                        #region ----- Log -----
                        //FileUtility.SaveFile("Log.txt", string.Format("Index of Start: {0}", index.ToString()));

                        //FileUtility.SaveFile("Log.txt",
                        //    string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}",
                        //    fieldvalue[index].ToString().Trim(),
                        //    fieldvalue[index + 1].ToString().Trim(),
                        //    fieldvalue[index + 2].ToString().Trim(),
                        //    fieldvalue[index + 3].ToString().Trim(),
                        //    fieldvalue[index + 4].ToString().Trim(),
                        //    Convert.ToDateTime(fieldvalue[index + 5].ToString().Trim()),
                        //    Convert.ToDateTime(fieldvalue[index + 6].ToString().Trim()),
                        //    fieldvalue[index + 7].ToString().Trim(),
                        //    fieldvalue[index + 8].ToString().Trim(),
                        //    fieldvalue[index + 9].ToString().Trim(),
                        //    fieldvalue[index + 10].ToString().Trim(),
                        //    fieldvalue[index + 11].ToString().Trim(),                            
                        //    Convert.ToDateTime(fieldvalue[index + 12].ToString().Trim()),
                        //    Convert.ToDateTime(fieldvalue[index + 13].ToString().Trim()),
                        //    fieldvalue[index + 14].ToString().Trim(),
                        //    fieldvalue[index + 15].ToString().Trim(),
                        //    fieldvalue[index + 16].ToString().Trim(),
                        //    fieldvalue[index + 17].ToString().Trim()
                        //    ));
                        #endregion

                        dataRow = LKM2GloviaDataTable.NewRow();

                        dataRow["LKM_CCN"] = fieldValueList[index].ToString().Trim();
                        dataRow["LKM_PN"] = fieldValueList[index + 1].ToString().Trim();
                        dataRow["LKM_DKP_SN"] = fieldValueList[index + 2].ToString().Trim();
                        dataRow["LKM_FROM_STATUS"] = fieldValueList[index + 3].ToString().Trim();
                        dataRow["LKM_TO_STATUS"] = fieldValueList[index + 4].ToString().Trim();

                        if (fieldValueList[index + 5].ToString().Trim().Length != 0)
                            dataRow["LKM_CHANGE_DATE_AUS"] = Convert.ToDateTime(fieldValueList[index + 5].ToString().Trim());
                        else
                            dataRow["LKM_CHANGE_DATE_AUS"] = DBNull.Value;

                        if (fieldValueList[index + 6].ToString().Trim().Length != 0)
                            dataRow["LKM_CHANGE_DATE_REG"] = Convert.ToDateTime(fieldValueList[index + 6].ToString().Trim());
                        else
                            dataRow["LKM_CHANGE_DATE_REG"] = DBNull.Value;

                        dataRow["LKM_TO_FACILITY"] = fieldValueList[index + 7].ToString().Trim();
                        dataRow["LKM_KEY_TYPE"] = fieldValueList[index + 8].ToString().Trim();
                        dataRow["LKM_MSG_ID"] = fieldValueList[index + 9].ToString().Trim();
                        dataRow["GLV_REF_NO"] = fieldValueList[index + 10].ToString().Trim();
                        dataRow["LKM_TRANS_TYPE"] = fieldValueList[index + 11].ToString().Trim();

                        if (fieldValueList[index + 12].ToString().Trim().Length != 0)
                            dataRow["GLV_TRANS_DATE_AUS"] = Convert.ToDateTime(fieldValueList[index + 12].ToString().Trim());
                        else
                            dataRow["GLV_TRANS_DATE_AUS"] = DBNull.Value;

                        if (fieldValueList[index + 13].ToString().Trim().Length != 0)
                            dataRow["GLV_TRANS_DATE_REG"] = Convert.ToDateTime(fieldValueList[index + 13].ToString().Trim());
                        else
                            dataRow["GLV_TRANS_DATE_REG"] = DBNull.Value;

                        dataRow["GLV_FROM_ML"] = fieldValueList[index + 14].ToString().Trim();
                        dataRow["GLV_FROM_SR"] = fieldValueList[index + 15].ToString().Trim();
                        dataRow["GLV_TO_ML"] = fieldValueList[index + 16].ToString().Trim();
                        dataRow["GLV_TO_SR"] = fieldValueList[index + 17].ToString().Trim();
                        dataRow["SNAP_SHOT_DATE"] = currentDay;

                        LKM2GloviaDataTable.Rows.Add(dataRow);
                    }
                }
                else if (reportname.ToUpper().Equals("GLOVIA_TO_LKM"))
                {
                    //FileUtility.SaveFile("Log.txt", string.Format("Length of total: {0}", fieldvaluelist.Count.ToString()));
                    // index #0 ~ 17 are field names
                    for (int index = 18; index < fieldValueList.Count; index += 18)
                    {
                        #region ----- Log -----
                        //FileUtility.SaveFile("Log.txt", string.Format("Index of Start: {0}", index.ToString()));
                        //FileUtility.SaveFile("Log.txt",
                        //    string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}",
                        //    fieldvaluelist[index].ToString().Trim(),
                        //    fieldvaluelist[index + 1].ToString().Trim(),
                        //    fieldvaluelist[index + 2].ToString().Trim(),
                        //    fieldvaluelist[index + 3].ToString().Trim(),
                        //    fieldvaluelist[index + 4].ToString().Trim(),
                        //    Convert.ToDateTime(fieldvaluelist[index + 5].ToString().Trim()),
                        //    Convert.ToDateTime(fieldvaluelist[index + 6].ToString().Trim()),
                        //    fieldvaluelist[index + 7].ToString().Trim(),
                        //    fieldvaluelist[index + 8].ToString().Trim(),
                        //    fieldvaluelist[index + 9].ToString().Trim(),
                        //    fieldvaluelist[index + 10].ToString().Trim(),
                        //    fieldvaluelist[index + 11].ToString().Trim(),
                        //    fieldvaluelist[index + 12].ToString().Trim(),
                        //    Convert.ToDateTime(fieldvaluelist[index + 13].ToString().Trim()),
                        //    Convert.ToDateTime(fieldvaluelist[index + 14].ToString().Trim()),
                        //    fieldvaluelist[index + 15].ToString().Trim(),
                        //    fieldvaluelist[index + 16].ToString().Trim(),
                        //    fieldvaluelist[index + 17].ToString().Trim()
                        //    ));
                        #endregion

                        dataRow = Glovia2LKMDataTable.NewRow();

                        dataRow["GLV_CCN"] = fieldValueList[index].ToString().Trim();
                        dataRow["GLV_PN"] = fieldValueList[index + 1].ToString().Trim();
                        dataRow["GLV_DPK_SN"] = fieldValueList[index + 2].ToString().Trim();
                        dataRow["GLV_REF_NO"] = fieldValueList[index + 3].ToString().Trim();
                        dataRow["LKM_TRANS_TYPE"] = fieldValueList[index + 4].ToString().Trim();

                        if (fieldValueList[index + 5].ToString().Trim().Length != 0)
                            dataRow["GLV_TRANS_DATE_AUS"] = Convert.ToDateTime(fieldValueList[index + 5].ToString().Trim());
                        else
                            dataRow["GLV_TRANS_DATE_AUS"] = DBNull.Value;

                        if (fieldValueList[index + 6].ToString().Trim().Length != 0)
                            dataRow["GLV_TRANS_DATE_REG"] = Convert.ToDateTime(fieldValueList[index + 6].ToString().Trim());
                        else
                            dataRow["GLV_TRANS_DATE_REG"] = DBNull.Value;

                        dataRow["GLV_FROM_ML"] = fieldValueList[index + 7].ToString().Trim();
                        dataRow["GLV_FROM_SR"] = fieldValueList[index + 8].ToString().Trim();
                        dataRow["GLV_TO_ML"] = fieldValueList[index + 9].ToString().Trim();
                        dataRow["GLV_TO_SR"] = fieldValueList[index + 10].ToString().Trim();
                        dataRow["LKM_FROM_STATUS"] = fieldValueList[index + 11].ToString().Trim();
                        dataRow["LKM_TO_STATUS"] = fieldValueList[index + 12].ToString().Trim();

                        if (fieldValueList[index + 13].ToString().Trim().Length != 0)
                            dataRow["LKM_UPDATE_DATE_AUS"] = Convert.ToDateTime(fieldValueList[index + 13].ToString().Trim());
                        else
                            dataRow["LKM_UPDATE_DATE_AUS"] = DBNull.Value;

                        if (fieldValueList[index + 14].ToString().Trim().Length != 0)
                            dataRow["LKM_UPDATE_DATE_REG"] = Convert.ToDateTime(fieldValueList[index + 14].ToString().Trim());
                        else
                            dataRow["LKM_UPDATE_DATE_REG"] = DBNull.Value;

                        dataRow["LKM_TO_FACILITY"] = fieldValueList[index + 15].ToString().Trim();
                        dataRow["LKM_KEY_TYPE"] = fieldValueList[index + 16].ToString().Trim();
                        dataRow["GLV_MSG_ID"] = fieldValueList[index + 17].ToString().Trim();
                        dataRow["SNAP_SHOT_DATE"] = currentDay;

                        Glovia2LKMDataTable.Rows.Add(dataRow);
                    }
                }
                else if (reportname.ToUpper().Equals("GLOVIA_TO_LKM_EXCEPTION"))
                {
                    //FileUtility.SaveFile("Log.txt", string.Format("Length of total: {0}", fieldvaluelist.Count.ToString()));
                    // index #0 ~ 17 are field names
                    for (int index = 18; index < fieldValueList.Count; index += 18)
                    {
                        #region ----- Log -----
                        //FileUtility.SaveFile("Log.txt", string.Format("Index of Start: {0}", index.ToString()));
                        //FileUtility.SaveFile("Log.txt",
                        //    string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}",
                        //    fieldvalue[index].ToString().Trim(),
                        //    fieldvalue[index + 1].ToString().Trim(),
                        //    fieldvalue[index + 2].ToString().Trim(),
                        //    fieldvalue[index + 3].ToString().Trim(),
                        //    fieldvalue[index + 4].ToString().Trim(),
                        //    Convert.ToDateTime(fieldvalue[index + 5].ToString().Trim()),
                        //    Convert.ToDateTime(fieldvalue[index + 6].ToString().Trim()),
                        //    fieldvalue[index + 7].ToString().Trim(),
                        //    fieldvalue[index + 8].ToString().Trim(),
                        //    fieldvalue[index + 9].ToString().Trim(),
                        //    fieldvalue[index + 10].ToString().Trim(),
                        //    fieldvalue[index + 11].ToString().Trim(),
                        //    fieldvalue[index + 12].ToString().Trim(),
                        //    Convert.ToDateTime(fieldvalue[index + 13].ToString().Trim()),
                        //    Convert.ToDateTime(fieldvalue[index + 14].ToString().Trim()),
                        //    fieldvalue[index + 15].ToString().Trim(),
                        //    fieldvalue[index + 16].ToString().Trim(),
                        //    fieldvalue[index + 17].ToString().Trim()
                        //    ));
                        #endregion

                        //FileUtility.SaveFile("Log.txt", string.Format("Index of Start: {0}", (index + 17).ToString()));
                        dataRow = GloviaExceptDataTable.NewRow();

                        dataRow["GLV_CCN"] = fieldValueList[index].ToString().Trim();
                        dataRow["GLV_PN"] = fieldValueList[index + 1].ToString().Trim();
                        dataRow["GLV_DPK_SN"] = fieldValueList[index + 2].ToString().Trim();
                        dataRow["GLV_REF_NO"] = fieldValueList[index + 3].ToString().Trim();
                        dataRow["LKM_TRANS_TYPE"] = fieldValueList[index + 4].ToString().Trim();

                        if (fieldValueList[index + 5].ToString().Trim().Length != 0)
                            dataRow["GLV_TRANS_DATE_AUS"] = Convert.ToDateTime(fieldValueList[index + 5].ToString().Trim());
                        else
                            dataRow["GLV_TRANS_DATE_AUS"] = DBNull.Value;

                        if (fieldValueList[index + 6].ToString().Trim().Length != 0)
                            dataRow["GLV_TRANS_DATE_REG"] = Convert.ToDateTime(fieldValueList[index + 6].ToString().Trim());
                        else
                            dataRow["GLV_TRANS_DATE_REG"] = DBNull.Value;

                        dataRow["GLV_FROM_ML"] = fieldValueList[index + 7].ToString().Trim();
                        dataRow["GLV_FROM_SR"] = fieldValueList[index + 8].ToString().Trim();
                        dataRow["GLV_TO_ML"] = fieldValueList[index + 9].ToString().Trim();
                        dataRow["GLV_TO_SR"] = fieldValueList[index + 10].ToString().Trim();
                        dataRow["LKM_FROM_STATUS"] = fieldValueList[index + 11].ToString().Trim();
                        dataRow["LKM_TO_STATUS"] = fieldValueList[index + 12].ToString().Trim();

                        if (fieldValueList[index + 13].ToString().Trim().Length != 0)
                            dataRow["LKM_UPDATE_DATE_AUS"] = Convert.ToDateTime(fieldValueList[index + 13].ToString().Trim());
                        else
                            dataRow["LKM_UPDATE_DATE_AUS"] = DBNull.Value;

                        if (fieldValueList[index + 14].ToString().Trim().Length != 0)
                            dataRow["LKM_UPDATE_DATE_REG"] = Convert.ToDateTime(fieldValueList[index + 14].ToString().Trim());
                        else
                            dataRow["LKM_UPDATE_DATE_REG"] = DBNull.Value;

                        dataRow["LKM_TO_FACILITY"] = fieldValueList[index + 15].ToString().Trim();
                        dataRow["LKM_KEY_TYPE"] = fieldValueList[index + 16].ToString().Trim();
                        dataRow["GLV_MSG_ID"] = fieldValueList[index + 17].ToString().Trim();
                        dataRow["SNAP_SHOT_DATE"] = currentDay;

                        GloviaExceptDataTable.Rows.Add(dataRow);
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    MiscUtility.LogHistory(string.Format("Function name: <FillDataIntoDataTable>, Source:{0},  Error:{1}", ex.Source, ex.Message));
            //    throw;
            //}
        }

        private void ExportReconResultReport()
        {                               
            DateTime latestGLVTransDate = DateTime.Now;
            DateTime maxGLVTransDate = DateTime.Now;
            DateTime currentDate = DateTime.Now;

            try
            {
                using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(Connectionstring)))
                {
                    latestGLVTransDate = Convert.ToDateTime(dao.GetLatestGLVTransDate().Rows[0][0]);
                    maxGLVTransDate = Convert.ToDateTime(dao.GetMaxGLVTransDate().Rows[0][0]);

                    int diffDays = MiscUtility.DiffDay(maxGLVTransDate, latestGLVTransDate);

                    for (int index = 1; index < diffDays; index++)
                    {
                        Console.WriteLine(string.Format("[{0}] - Starting to call SQL Server procedure for DPK reconciliation...",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        MiscUtility.LogHistory("Starting to call SQL Server procedure for DPK reconciliation...");

                        currentDate = latestGLVTransDate.AddDays(index);

                        DataTable datatable = null;
                        //using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(Connectionstring)))
                        //{
                            dao.InsertVarianceHistory(currentDate.ToShortDateString());
                            datatable = dao.GetVarianceItem();
                            dao.UpdateLastReconDate(currentDate.ToShortDateString(), DateTime.Now.ToShortDateString());
                        //}

                        Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        MiscUtility.LogHistory("Done!");

                        Console.WriteLine(string.Format("[{0}] - Starting to export DPK recon result report for {1}...",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), currentDate.ToShortDateString()));
                        MiscUtility.LogHistory(string.Format("Starting to export DPK recon result report for {0}...", currentDate.ToShortDateString()));

                        string filename = "DPK_LKM_Traceability_Trx_Exception";
                        string fullfilename = Path.Combine(ConfigFileUtility.GetValue("Output_Folder"),
                            string.Format("{0}_{1}.csv", filename, currentDate.ToString("yyyyMMdd_HHmm")));

                        ExcelFileUtility.ExportDataIntoExcelFile(fullfilename, datatable);

                        this.ReconDate = DateTime.Now;
                        this.GLVTransDate = currentDate;
                        this.TotalVairanceItems = datatable.Rows.Count;
                        this.ReconResultLink = string.Format(@"file://\\wn7-hgt6822\{0}", fullfilename.Remove(0, 3));   // Remove "D:\"                    

                        Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        MiscUtility.LogHistory("Done!");

                        TriggerNotification();
                    }
                }
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(string.Format("Function name: <ExportReconResultReport>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }
        }

        private void RemoveDPKHistoricalData()
        {
            try
            {
                using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(Connectionstring)))
                {
                    Console.WriteLine(string.Format("[{0}] - Starting to remove old DPK transaction data from database...",
                                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    MiscUtility.LogHistory("Starting to remove old DPK transaction data from database...");

                    dao.RemoveHistoricalData();

                    Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    MiscUtility.LogHistory("Done!");
                }
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(
                    string.Format("Function name: <RemoveDPKHistoricalData>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }
        }

        private void TriggerNotification()
        {
            try
            {
                //this.ReconDate = DateTime.Now;
                //this.GLVTransDate = DateTime.Now.AddDays(-3);
                //this.TotalVairanceItems = 12345;
                //this.ReconResultLink = @"file://\\wn7-hgt6822\DPK_Recon_Result\DPK_Recon_Result_20150331_230126.csv";

                using (MSAccessDAO dao = new MSAccessDAO())
                {
                    dao.InsertGlobalDPKReconHistory(this.ReconDate.ToShortDateString(), this.GLVTransDate.ToShortDateString(), this.TotalVairanceItems, this.ReconResultLink);
                }
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(
                    string.Format("Function name: <TriggerNotification>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }
        }

        #region --- Discard code <ExportReconReport> ---
        //private void ExportReconReport()
        //{
        //    try
        //    {
        //        Console.WriteLine(string.Format("[{0}] - Starting to call SQL Server procedure for DPK reconciliation...",
        //                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

        //        MiscUtility.LogHistory(string.Format("[{0}] - Starting to call SQL Server procedure for DPK reconciliation...",
        //                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

        //        DataTable datatable = null;
        //        using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(connectstring)))
        //        {
        //            dao.InsertVarianceItem();
        //            datatable = dao.GetVarianceItem();
        //        }

        //        Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        //        MiscUtility.LogHistory(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

        //        Console.WriteLine(string.Format("[{0}] - Starting to export DPK recon result report...",
        //                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

        //        MiscUtility.LogHistory(string.Format("[{0}] - Starting to export DPK recon result report...",
        //                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        //        string filename = "DPK_Recon_Result";
        //        string fullfilename = Path.Combine(ConfigFileUtility.GetValue("Output_Folder"),
        //            string.Format("{0}_{1}.csv", filename, DateTime.Now.ToString("yyyyMMdd_HHmmss")));
        //        ExcelFileUtility.ExportDataIntoExcelFile(fullfilename, datatable);

        //        Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        //        MiscUtility.LogHistory(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
        //    }
        //    catch (Exception ex)
        //    {
        //        MiscUtility.LogHistory(string.Format("Function name: <ExportReconReport>, Source:{0},  Error:{1}", ex.Source, ex.Message));
        //        throw;
        //    }
        //}
        #endregion

        #region --- Discard code <IsFileUpload> ----
        //private bool IsFileUploaded(string filename)
        //{
        //    bool flag = false;

        //    try
        //    {
        //        using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(connectstring)))
        //        {
        //            flag = dao.CheckFileNameHistory(filename);
        //        }

        //        return flag;
        //    }
        //    catch (Exception ex)
        //    {
        //        MiscUtility.LogHistory(string.Format("Function name: <IsFileUploaded>, Source:{0},  Error:{1}", ex.Source, ex.Message));
        //        throw;
        //    }
        //}
        #endregion

        #region --- Discard code <UploadExcelFile> ---
        //public void UploadExcelFile(InFileParameter infileparameter, ref DataSet ds)
        //{
        //    foreach (FileReport filereport in reportconfig.FileReports)
        //    {
        //        if (infileparameter.ReportName.ToUpper().Contains(filereport.ReportFileName)
        //            && infileparameter.DataBaseName.ToUpper().Equals(filereport.DataBaseName))
        //        {
        //            FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"),
        //                string.Format("[{0}] - Starting read report file - {1}...", DateTime.Now.ToString(), filereport.ReportFileName));

        //            ds = ReadExcelFile(infileparameter.ReportFileName, infileparameter.ReportName, filereport.DataBaseName, filereport.TableField);

        //            FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"),
        //                string.Format("[{0}] - Done!", DateTime.Now.ToString()));

        //            ds = AddSnapShotDate(ds, infileparameter.SnapShotDate.ToShortDateString());

        //            ImportReportFile(ds, infileparameter.ReportName, infileparameter.DataBaseName, filereport.TableName, filereport.TableField, infileparameter.SnapShotDate.ToShortDateString());

        //            if (infileparameter.ReportName.ToUpper().Contains("STOCK STATUS"))
        //                SynchronizedPartAttribution(infileparameter.SnapShotDate);
        //        }
        //    }
        //}

        //// Log pulling report record into Access database
        //private void LogPullingReportRecord(string reportname, string starttime, string endtime, string category, Int32 totalrecordnumber)
        //{
        //    using (MSAccessDAO dao = new MSAccessDAO())
        //    {
        //        dao.InsertPullingReportLog(reportname, category, starttime, endtime, totalrecordnumber);
        //    }
        //}

        ////Read content of Excel file, then save into a DataSet
        //private DataSet ReadExcelFile(string filename, string reportname, string databasename, string fieldlist)
        //{
        //    DataSet rawdata = null;

        //    string sheetname = string.Empty;

        //    if (databasename.Equals("AX") && reportname.Contains("TRANSACTION"))        //Don't need to change original AX_Transaction report
        //        sheetname = ExcelFileUtility.GetExcelFileSheetName(filename, false, 1);
        //    else
        //        sheetname = ExcelFileUtility.GetExcelFileSheetName(filename, true, 1);

        //    try
        //    {
        //        using (ExcelAccessDAO dao = new ExcelAccessDAO(filename))
        //        {
        //            //string sheetname = dao.GetExcelSheetName();
        //            rawdata = dao.ReadExcelFile(sheetname, fieldlist);
        //        }

        //        return rawdata;
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Info(ex.Message);
        //        throw ex;
        //    }
        //}

        ////Save content of Excel file with a snapshot date into database
        //private void ImportReportFile(DataSet ds, string reportname, string databasename, string tablename, string fieldlist, string snapshotdate)
        //{
        //    try
        //    {
        //        using (MSAccessDAO dao = new MSAccessDAO())
        //        {
        //            int recordcount = dao.GetRecordCountBySnapShotDate(tablename, snapshotdate);

        //            if (recordcount > 0)
        //            {
        //                dao.DeleteDateBySnapShotDate(tablename, snapshotdate);
        //            }

        //            //dao.InsertDataIntoDataBase(tablename, ds);
        //            FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"),
        //                string.Format("[{0}] - Starting import report file - {1} into MS Access database...", DateTime.Now.ToString(), reportname));

        //            if (databasename.Equals("AX"))
        //            {
        //                if (reportname.ToUpper().Contains("RECEIPT"))
        //                    dao.InsertAXReceipt(ds);
        //                else if (reportname.ToUpper().Contains("RELIEF"))
        //                    dao.InsertAXRelief(ds);
        //                else if (reportname.ToUpper().Contains("STOCK STATUS"))
        //                    dao.InsertAXStockStatus(ds);
        //                else if (reportname.ToUpper().Contains("TRANSACTION"))
        //                    dao.InsertAXTransaction(ds);
        //            }
        //            else if (databasename.Equals("GLOVIA"))
        //            {
        //                if (reportname.ToUpper().Contains("RECEIPT"))
        //                    dao.InsertGloviaReceipt(ds);
        //                else if (reportname.ToUpper().Contains("RELIEF"))
        //                    dao.InsertGloviaRelief(ds);
        //                else if (reportname.ToUpper().Contains("STOCK STATUS"))
        //                    dao.InsertGloviaStockStatus(ds);
        //                else if (reportname.ToUpper().Contains("TRANSACTION"))
        //                    dao.InsertGloviaTransaction(ds);
        //            }

        //            FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"),
        //                string.Format("[{0}] - Done!", DateTime.Now.ToString()));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Info(ex.Message);
        //        throw ex;
        //    }
        //}

        //private DataSet AddSnapShotDate(DataSet rawdata, string snapshotdate)
        //{
        //    rawdata.Tables[0].Columns.Add("SNAPSHOT_DATE", typeof(string));
        //    int snapshot_data_columnnum = rawdata.Tables[0].Columns.Count;

        //    for (int index = 0; index < rawdata.Tables[0].Rows.Count; index++)
        //    {
        //        rawdata.Tables[0].Rows[index][snapshot_data_columnnum - 1] = snapshotdate;
        //    }

        //    return rawdata;
        //}
        #endregion

        #region Make data table structure
        private void MakeDataTableStructure()
        {
            MakeLKM2GloviaDataTable();
            MakeGlovia2LKMDataTable();
            MakeGloviaExceptDataTable();
        }

        // Make datatable structure "LKM2GloviaDataTable"
        private void MakeLKM2GloviaDataTable()
        {
            try
            {
                LKM2GloviaDataTable = new DataTable("LKM2GloviaDataTable");

                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_CCN", 0);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_PN", 1);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_DKP_SN", 2);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_FROM_STATUS", 3);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_TO_STATUS", 4);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.DateTime", "LKM_CHANGE_DATE_AUS", 5);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.DateTime", "LKM_CHANGE_DATE_REG", 6);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_TO_FACILITY", 7);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_KEY_TYPE", 8);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_MSG_ID", 9);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "GLV_REF_NO", 10);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "LKM_TRANS_TYPE", 11);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.DateTime", "GLV_TRANS_DATE_AUS", 12);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.DateTime", "GLV_TRANS_DATE_REG", 13);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "GLV_FROM_ML", 14);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "GLV_FROM_SR", 15);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "GLV_TO_ML", 16);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.String", "GLV_TO_SR", 17);
                MiscUtility.InsertNewColumn(ref LKM2GloviaDataTable, "System.DateTime", "SNAP_SHOT_DATE", 18);
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(string.Format("Function name: <MakeLKM2GloviaDataTable>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }

            #region Discard code
            //DataColumn lkmCCN = new DataColumn();
            //lkmCCN.DataType = System.Type.GetType("System.String");
            //lkmCCN.ColumnName = "LKM_CCN";
            //LKM2GloviaDataTable.Columns.Add(lkmCCN);

            //DataColumn lkmPN = new DataColumn();
            //lkmPN.DataType = System.Type.GetType("System.String");
            //lkmPN.ColumnName = "LKM_PN";
            //LKM2GloviaDataTable.Columns.Add(lkmPN);

            //DataColumn lkmDpkSn = new DataColumn();
            //lkmDpkSn.DataType = System.Type.GetType("System.String");
            //lkmDpkSn.ColumnName = "LKM_DKP_SN";
            //LKM2GloviaDataTable.Columns.Add(lkmDpkSn);

            //DataColumn lkmFromStatus = new DataColumn();
            //lkmFromStatus.DataType = System.Type.GetType("System.String");
            //lkmFromStatus.ColumnName = "LKM_FROM_STATUS";
            //LKM2GloviaDataTable.Columns.Add(lkmFromStatus);

            //DataColumn lkmToStatus = new DataColumn();
            //lkmToStatus.DataType = System.Type.GetType("System.String");
            //lkmToStatus.ColumnName = "LKM_TO_STATUS";
            //LKM2GloviaDataTable.Columns.Add(lkmToStatus);

            //DataColumn lkmChangeDateAus = new DataColumn();
            //lkmChangeDateAus.DataType = System.Type.GetType("System.DateTime");
            //lkmChangeDateAus.ColumnName = "LKM_CHANGE_DATE_AUS";
            //LKM2GloviaDataTable.Columns.Add(lkmChangeDateAus);

            //DataColumn lkmChangeDateReg = new DataColumn();
            //lkmChangeDateReg.DataType = System.Type.GetType("System.DateTime");
            //lkmChangeDateReg.ColumnName = "LKM_CHANGE_DATE_REG";
            //LKM2GloviaDataTable.Columns.Add(lkmChangeDateReg);

            //DataColumn lkmToFacility = new DataColumn();
            //lkmToFacility.DataType = System.Type.GetType("System.String");
            //lkmToFacility.ColumnName = "LKM_TO_FACILITY";
            //LKM2GloviaDataTable.Columns.Add(lkmToFacility);

            //DataColumn lkmKeyType = new DataColumn();
            //lkmKeyType.DataType = System.Type.GetType("System.String");
            //lkmKeyType.ColumnName = "LKM_KEY_TYPE";
            //LKM2GloviaDataTable.Columns.Add(lkmKeyType);

            //DataColumn lkmMsgId = new DataColumn();
            //lkmMsgId.DataType = System.Type.GetType("System.String");
            //lkmMsgId.ColumnName = "LKM_MSG_ID";
            //LKM2GloviaDataTable.Columns.Add(lkmMsgId);

            //DataColumn glvRefNo = new DataColumn();
            //glvRefNo.DataType = System.Type.GetType("System.String");
            //glvRefNo.ColumnName = "GLV_REF_NO";
            //LKM2GloviaDataTable.Columns.Add(glvRefNo);

            //DataColumn lkmTransType = new DataColumn();
            //lkmTransType.DataType = System.Type.GetType("System.String");
            //lkmTransType.ColumnName = "LKM_TRANS_TYPE";
            //LKM2GloviaDataTable.Columns.Add(lkmTransType);

            //DataColumn glvTransDateAus = new DataColumn();
            //glvTransDateAus.DataType = System.Type.GetType("System.DateTime");
            //glvTransDateAus.ColumnName = "GLV_TRANS_DATE_AUS";
            //LKM2GloviaDataTable.Columns.Add(glvTransDateAus);

            //DataColumn glvTransDateReg = new DataColumn();
            //glvTransDateReg.DataType = System.Type.GetType("System.DateTime");
            //glvTransDateReg.ColumnName = "GLV_TRANS_DATE_REG";
            //LKM2GloviaDataTable.Columns.Add(glvTransDateReg);

            //DataColumn glvFromML = new DataColumn();
            //glvFromML.DataType = System.Type.GetType("System.String");
            //glvFromML.ColumnName = "GLV_FROM_ML";
            //LKM2GloviaDataTable.Columns.Add(glvFromML);

            //DataColumn glvFromSR = new DataColumn();
            //glvFromSR.DataType = System.Type.GetType("System.String");
            //glvFromSR.ColumnName = "GLV_FROM_SR";
            //LKM2GloviaDataTable.Columns.Add(glvFromSR);

            //DataColumn glvToML = new DataColumn();
            //glvToML.DataType = System.Type.GetType("System.String");
            //glvToML.ColumnName = "GLV_TO_ML";
            //LKM2GloviaDataTable.Columns.Add(glvToML);

            //DataColumn glvToSR = new DataColumn();
            //glvToSR.DataType = System.Type.GetType("System.String");
            //glvToSR.ColumnName = "GLV_TO_SR";
            //LKM2GloviaDataTable.Columns.Add(glvToSR);
            #endregion
        }

        // Make datatable structure "MakeGlovia2LKMDataTable"
        private void MakeGlovia2LKMDataTable()
        {
            try
            {
                Glovia2LKMDataTable = new DataTable("Glovia2LKMDataTable");

                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_CCN", 0);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_PN", 1);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_DPK_SN", 2);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_REF_NO", 3);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "LKM_TRANS_TYPE", 4);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.DateTime", "GLV_TRANS_DATE_AUS", 5);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.DateTime", "GLV_TRANS_DATE_REG", 6);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_FROM_ML", 7);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_FROM_SR", 8);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_TO_ML", 9);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_TO_SR", 10);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "LKM_FROM_STATUS", 11);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "LKM_TO_STATUS", 12);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.DateTime", "LKM_UPDATE_DATE_AUS", 13);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.DateTime", "LKM_UPDATE_DATE_REG", 14);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "LKM_TO_FACILITY", 15);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "LKM_KEY_TYPE", 16);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.String", "GLV_MSG_ID", 17);
                MiscUtility.InsertNewColumn(ref Glovia2LKMDataTable, "System.DateTime", "SNAP_SHOT_DATE", 18);
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(string.Format("Function name: <MakeGlovia2LKMDataTable>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }

            #region Discard code
            //DataColumn glvCCN = new DataColumn();
            //glvCCN.DataType = System.Type.GetType("System.String");
            //glvCCN.ColumnName = "GLV_CCN";
            //Glovia2LKMDataTable.Columns.Add(glvCCN);
            
            //DataColumn glvPN = new DataColumn();
            //glvPN.DataType = System.Type.GetType("System.String");
            //glvPN.ColumnName = "GLV_PN";
            //Glovia2LKMDataTable.Columns.Add(glvPN);
            
            //DataColumn glvDPKSn = new DataColumn();
            //glvDPKSn.DataType = System.Type.GetType("System.String");
            //glvDPKSn.ColumnName = "GLV_DPK_SN";
            //Glovia2LKMDataTable.Columns.Add(glvDPKSn);

            //DataColumn glvRefNo = new DataColumn();
            //glvRefNo.DataType = System.Type.GetType("System.String");
            //glvRefNo.ColumnName = "GLV_REF_NO";
            //Glovia2LKMDataTable.Columns.Add(glvRefNo);

            //DataColumn lkmTransType = new DataColumn();
            //lkmTransType.DataType = System.Type.GetType("System.String");
            //lkmTransType.ColumnName = "LKM_TRANS_TYPE";
            //Glovia2LKMDataTable.Columns.Add(lkmTransType);

            //DataColumn glvTransDateAus = new DataColumn();
            //glvTransDateAus.DataType = System.Type.GetType("System.DateTime");
            //glvTransDateAus.ColumnName = "GLV_TRANS_DATE_AUS";
            //Glovia2LKMDataTable.Columns.Add(glvTransDateAus);

            //DataColumn glvTransDateReg = new DataColumn();
            //glvTransDateReg.DataType = System.Type.GetType("System.DateTime");
            //glvTransDateReg.ColumnName = "GLV_TRANS_DATE_REG";
            //Glovia2LKMDataTable.Columns.Add(glvTransDateReg);

            //DataColumn glvFromML = new DataColumn();
            //glvFromML.DataType = System.Type.GetType("System.String");
            //glvFromML.ColumnName = "GLV_FROM_ML";
            //Glovia2LKMDataTable.Columns.Add(glvFromML);

            //DataColumn glvFromSR = new DataColumn();
            //glvFromSR.DataType = System.Type.GetType("System.String");
            //glvFromSR.ColumnName = "GLV_FROM_SR";
            //Glovia2LKMDataTable.Columns.Add(glvFromSR);

            //DataColumn glvToML = new DataColumn();
            //glvToML.DataType = System.Type.GetType("System.String");
            //glvToML.ColumnName = "GLV_TO_ML";
            //Glovia2LKMDataTable.Columns.Add(glvToML);

            //DataColumn glvToSR = new DataColumn();
            //glvToSR.DataType = System.Type.GetType("System.String");
            //glvToSR.ColumnName = "GLV_TO_SR";
            //Glovia2LKMDataTable.Columns.Add(glvToSR);

            //DataColumn lkmFromStatus = new DataColumn();
            //lkmFromStatus.DataType = System.Type.GetType("System.String");
            //lkmFromStatus.ColumnName = "LKM_FROM_STATUS";
            //Glovia2LKMDataTable.Columns.Add(lkmFromStatus);

            //DataColumn lkmToStatus = new DataColumn();
            //lkmToStatus.DataType = System.Type.GetType("System.String");
            //lkmToStatus.ColumnName = "LKM_TO_STATUS";
            //Glovia2LKMDataTable.Columns.Add(lkmToStatus);

            //DataColumn lkmUpdateDateAus = new DataColumn();
            //lkmUpdateDateAus.DataType = System.Type.GetType("System.DateTime");
            //lkmUpdateDateAus.ColumnName = "LKM_UPDATE_DATE_AUS";
            //Glovia2LKMDataTable.Columns.Add(lkmUpdateDateAus);

            //DataColumn lkmUpdateDateReg = new DataColumn();
            //lkmUpdateDateReg.DataType = System.Type.GetType("System.DateTime");
            //lkmUpdateDateReg.ColumnName = "LKM_UPDATE_DATE_REG";
            //Glovia2LKMDataTable.Columns.Add(lkmUpdateDateReg);

            //DataColumn lkmToFacility = new DataColumn();
            //lkmToFacility.DataType = System.Type.GetType("System.String");
            //lkmToFacility.ColumnName = "LKM_TO_FACILITY";
            //Glovia2LKMDataTable.Columns.Add(lkmToFacility);

            //DataColumn lkmKeyType = new DataColumn();
            //lkmKeyType.DataType = System.Type.GetType("System.String");
            //lkmKeyType.ColumnName = "LKM_KEY_TYPE";
            //Glovia2LKMDataTable.Columns.Add(lkmKeyType);

            //DataColumn lkmMsgId = new DataColumn();
            //lkmMsgId.DataType = System.Type.GetType("System.String");
            //lkmMsgId.ColumnName = "LKM_MSG_ID";
            //Glovia2LKMDataTable.Columns.Add(lkmMsgId);
            #endregion
        }

        // Make datatable structure "MakeGloviaExceptDataTable"
        private void MakeGloviaExceptDataTable()
        {
            try
            {
                GloviaExceptDataTable = new DataTable("GloviaExceptDataTable");

                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_CCN", 0);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_PN", 1);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_DPK_SN", 2);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_REF_NO", 3);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "LKM_TRANS_TYPE", 4);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.DateTime", "GLV_TRANS_DATE_AUS", 5);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.DateTime", "GLV_TRANS_DATE_REG", 6);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_FROM_ML", 7);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_FROM_SR", 8);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_TO_ML", 9);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_TO_SR", 10);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "LKM_FROM_STATUS", 11);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "LKM_TO_STATUS", 12);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.DateTime", "LKM_UPDATE_DATE_AUS", 13);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.DateTime", "LKM_UPDATE_DATE_REG", 14);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "LKM_TO_FACILITY", 15);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "LKM_KEY_TYPE", 16);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.String", "GLV_MSG_ID", 17);
                MiscUtility.InsertNewColumn(ref GloviaExceptDataTable, "System.DateTime", "SNAP_SHOT_DATE", 18);
            }
            catch (Exception ex)
            {
                MiscUtility.LogHistory(string.Format("Function name: <MakeGloviaExceptDataTable>, Source:{0},  Error:{1}", ex.Source, ex.Message));
                throw;
            }

            #region Discard code
            //DataColumn glvCCN = new DataColumn();
            //glvCCN.DataType = System.Type.GetType("System.String");
            //glvCCN.ColumnName = "GLV_CCN";
            //GloviaExceptDataTable.Columns.Add(glvCCN);

            //DataColumn glvPN = new DataColumn();
            //glvPN.DataType = System.Type.GetType("System.String");
            //glvPN.ColumnName = "GLV_PN";
            //GloviaExceptDataTable.Columns.Add(glvPN);

            //DataColumn glvDPKSn = new DataColumn();
            //glvDPKSn.DataType = System.Type.GetType("System.String");
            //glvDPKSn.ColumnName = "GLV_DPK_SN";
            //GloviaExceptDataTable.Columns.Add(glvDPKSn);

            //DataColumn glvRefNo = new DataColumn();
            //glvRefNo.DataType = System.Type.GetType("System.String");
            //glvRefNo.ColumnName = "GLV_REF_NO";
            //GloviaExceptDataTable.Columns.Add(glvRefNo);

            //DataColumn lkmTransType = new DataColumn();
            //lkmTransType.DataType = System.Type.GetType("System.String");
            //lkmTransType.ColumnName = "LKM_TRANS_TYPE";
            //GloviaExceptDataTable.Columns.Add(lkmTransType);

            //DataColumn glvTransDateAus = new DataColumn();
            //glvTransDateAus.DataType = System.Type.GetType("System.DateTime");
            //glvTransDateAus.ColumnName = "GLV_TRANS_DATE_AUS";
            //GloviaExceptDataTable.Columns.Add(glvTransDateAus);

            //DataColumn glvTransDateReg = new DataColumn();
            //glvTransDateReg.DataType = System.Type.GetType("System.DateTime");
            //glvTransDateReg.ColumnName = "GLV_TRANS_DATE_REG";
            //GloviaExceptDataTable.Columns.Add(glvTransDateReg);

            //DataColumn glvFromML = new DataColumn();
            //glvFromML.DataType = System.Type.GetType("System.String");
            //glvFromML.ColumnName = "GLV_FROM_ML";
            //GloviaExceptDataTable.Columns.Add(glvFromML);

            //DataColumn glvFromSR = new DataColumn();
            //glvFromSR.DataType = System.Type.GetType("System.String");
            //glvFromSR.ColumnName = "GLV_FROM_SR";
            //GloviaExceptDataTable.Columns.Add(glvFromSR);

            //DataColumn glvToML = new DataColumn();
            //glvToML.DataType = System.Type.GetType("System.String");
            //glvToML.ColumnName = "GLV_TO_ML";
            //GloviaExceptDataTable.Columns.Add(glvToML);

            //DataColumn glvToSR = new DataColumn();
            //glvToSR.DataType = System.Type.GetType("System.String");
            //glvToSR.ColumnName = "GLV_TO_SR";
            //GloviaExceptDataTable.Columns.Add(glvToSR);

            //DataColumn lkmFromStatus = new DataColumn();
            //lkmFromStatus.DataType = System.Type.GetType("System.String");
            //lkmFromStatus.ColumnName = "LKM_FROM_STATUS";
            //GloviaExceptDataTable.Columns.Add(lkmFromStatus);

            //DataColumn lkmToStatus = new DataColumn();
            //lkmToStatus.DataType = System.Type.GetType("System.String");
            //lkmToStatus.ColumnName = "LKM_TO_STATUS";
            //GloviaExceptDataTable.Columns.Add(lkmToStatus);

            //DataColumn lkmUpdateDateAus = new DataColumn();
            //lkmUpdateDateAus.DataType = System.Type.GetType("System.DateTime");
            //lkmUpdateDateAus.ColumnName = "LKM_UPDATE_DATE_AUS";
            //GloviaExceptDataTable.Columns.Add(lkmUpdateDateAus);

            //DataColumn lkmUpdateDateReg = new DataColumn();
            //lkmUpdateDateReg.DataType = System.Type.GetType("System.DateTime");
            //lkmUpdateDateReg.ColumnName = "LKM_UPDATE_DATE_REG";
            //GloviaExceptDataTable.Columns.Add(lkmUpdateDateReg);

            //DataColumn lkmToFacility = new DataColumn();
            //lkmToFacility.DataType = System.Type.GetType("System.String");
            //lkmToFacility.ColumnName = "LKM_TO_FACILITY";
            //GloviaExceptDataTable.Columns.Add(lkmToFacility);

            //DataColumn lkmKeyType = new DataColumn();
            //lkmKeyType.DataType = System.Type.GetType("System.String");
            //lkmKeyType.ColumnName = "LKM_KEY_TYPE";
            //GloviaExceptDataTable.Columns.Add(lkmKeyType);

            //DataColumn lkmMsgId = new DataColumn();
            //lkmMsgId.DataType = System.Type.GetType("System.String");
            //lkmMsgId.ColumnName = "LKM_MSG_ID";
            //GloviaExceptDataTable.Columns.Add(lkmMsgId);
            #endregion
        }
        #endregion

    }
}
