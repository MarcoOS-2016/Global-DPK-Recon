using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Reflection;
using log4net;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;

namespace GIC.Common
{
    public class ExcelFileUtility
    {
        private static ILog log = LogManager.GetLogger(typeof(ExcelFileUtility));

        // Export report into a CSV file
        public static void ExportDataIntoExcelFile(string filename, System.Data.DataTable datatable)
        {
            if (filename.Length != 0)
            {
                FileStream filestream = null;
                StreamWriter streamwriter = null;
                string stringline = string.Empty;

                try
                {
                    filestream = new FileStream(filename, FileMode.Append, FileAccess.Write);
                    streamwriter = new StreamWriter(filestream, System.Text.Encoding.Unicode);

                    for (int i = 0; i < datatable.Columns.Count; i++)
                    {
                        stringline = stringline + datatable.Columns[i].ColumnName.ToString() + Convert.ToChar(9);

                    }

                    streamwriter.WriteLine(stringline);
                    stringline = "";

                    for (int i = 0; i < datatable.Rows.Count; i++)
                    {
                        //stringline = stringline + (i + 1) + Convert.ToChar(9);
                        for (int j = 0; j < datatable.Columns.Count; j++)
                        {
                            stringline = stringline + datatable.Rows[i][j].ToString() + Convert.ToChar(9);
                        }

                        streamwriter.WriteLine(stringline);
                        stringline = "";
                    }

                    streamwriter.Close();
                    filestream.Close();
                }
                catch (Exception ex)
                {
                    log.Info(ex.Message);
                    throw ex;
                }
            }
        }

        public static string GetExcelFileSheetName(string filename, bool flag)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            //Microsoft.Office.Interop.Excel.Worksheet worksheet = null;

            string sheetName = String.Empty;

            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();

                if (excel == null)
                    throw new Exception("There is not an Excel application on your computer!");

                workbook = excel.Workbooks.Open(filename, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                //FileUtility.SaveFile(@"C:\GIC AX Auto Recon Tool\FileFormat.txt", workbook.FileFormat.ToString());
                //if (workbook.FileFormat == Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault)
                sheetName = ((Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1]).Name;

                if (flag)
                {
                    int rawnum = 2;                                                             //Raw data from the second line
                    for (int index = 1; index <= 3; index++)                                    //Select the first 3 column to change 
                        workbook.Worksheets[1].Cells(rawnum, index).NumberFormatLocal = "@";    //Set the values in raw# 2 as text type.
                }

                workbook.Save();
                workbook.Close();
                excel.Quit();

                return sheetName;
            }

            catch (System.Exception e)
            {
                log.Info(e.Message);
                throw e;
            }

            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);

                excel = null;
                workbook = null;
                //worksheet = null;

                GC.Collect();
            }
        }

        // Export report to an excel file
        public static void SaveExcelFile(string filename, System.Data.DataTable datatable)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            Microsoft.Office.Interop.Excel.Worksheet worksheet = null;

            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();

                if (excel == null)
                    throw new Exception("There is not an Excel application on your computer!");

                excel.Application.Workbooks.Add(true);
                //excel.Visible = true;

                workbook = excel.Workbooks.Add();
                worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.ActiveSheet;

                //wks.Visible = XlSheetVisibility.xlSheetVisible;

                int rowIndex = 1;
                int colIndex = 0;

                //DataTable datatable = ds.Tables[0];

                foreach (DataColumn col in datatable.Columns)
                {
                    colIndex++;
                    excel.Cells[1, colIndex] = col.ColumnName;
                }

                foreach (DataRow row in datatable.Rows)
                {
                    rowIndex++;
                    colIndex = 0;

                    foreach (DataColumn col in datatable.Columns)
                    {
                        colIndex++;
                        excel.Cells[rowIndex, colIndex] = row[col.ColumnName].ToString();
                    }
                }

                worksheet.Cells.EntireColumn.AutoFit();

                //workbook.SaveAs(String.Format("{0}_{1}.xls", filename, DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                SetTitusClassification(ref workbook);
                workbook.SaveAs(filename);
                workbook.Close();
                excel.Quit();
            }

            catch (System.Exception e)
            {
                log.Info(e.Message);
                throw e;
            }

            finally
            {
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);

                excel = null;
                workbook = null;
                worksheet = null;

                GC.Collect();
            }
        }

        // Export multiple reports to an excel file
        public static void SaveExcelFile(string filename, List<System.Data.DataTable> datatablelist)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
            Microsoft.Office.Interop.Excel.Worksheet worksheet = null;
            Microsoft.Office.Interop.Excel.Sheets sheets = null;

            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();

                if (excel == null)
                    throw new Exception("There is not an Excel application on your computer!");

                excel.Application.Workbooks.Add(true);
                workbook = excel.Workbooks.Add();
                sheets = workbook.Worksheets;

                //foreach (DataTable dt in datatablelist)
                //{

                //for (int index = 0; index < dt.Length; index++)
                //foreach (DataTable datatable in ds.Tables)
                for (int index = 0; index < datatablelist.Count; index++)
                {
                    if (worksheet == null)
                    {
                        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];
                    }
                    else
                    {
                        worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets.Add(Type.Missing, worksheet, 1, Type.Missing);
                    }

                    //worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[index + 1];
                    worksheet.Name = datatablelist[index].TableName;

                    int rowIndex = 1;
                    int colIndex = 0;

                    foreach (DataColumn col in datatablelist[index].Columns)
                    {
                        colIndex++;
                        excel.Cells[1, colIndex] = col.ColumnName;
                    }

                    foreach (DataRow row in datatablelist[index].Rows)
                    {
                        rowIndex++;
                        colIndex = 0;

                        string content = string.Empty;
                        foreach (DataColumn col in datatablelist[index].Columns)
                        {
                            colIndex++;
                            content = row[col.ColumnName].ToString();

                            if (col.ColumnName.ToUpper() == "PART" || col.ColumnName.ToUpper() == "PART#" || col.ColumnName.ToUpper() == "SALES ORDER#")
                            {
                                excel.Cells[rowIndex, colIndex].NumberFormatLocal = "@";
                                excel.Cells[rowIndex, colIndex] = content;
                            }
                            else
                            {
                                excel.Cells[rowIndex, colIndex] = content;
                            }
                        }
                    }

                    //if (datatablelist[index].TableName.Equals("Variance_Detail_Items"))
                    //excel.Cells.Sort(excel.Cells.Columns[3], Microsoft.Office.Interop.Excel.XlSortOrder.xlDescending); // Column[3] indicates to "Type" column.

                    worksheet.Cells.EntireColumn.AutoFit();
                }

                //workbook.SaveAs(String.Format("{0}_{1}.xls", filename, DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                SetTitusClassification(ref workbook);
                workbook.SaveAs(filename);
            }

            catch (System.Exception e)
            {
                log.Info(e.Message);
                throw e;
            }

            finally
            {
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                //System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);

                if (workbook != null) workbook.Close();
                if (excel != null) excel.Quit();

                worksheet = null;
                workbook = null;
                excel = null;

                GC.Collect();
            }
        }

        //Save non-standard excel file format as standard one(.xlsx)
        public static string SaveAsStandardFileFormat(string fullfilename)
        {
            Microsoft.Office.Interop.Excel.Application excel = null;
            Microsoft.Office.Interop.Excel.Workbook workbook = null;

            try
            {
                string newfilename = string.Empty;
                excel = new Microsoft.Office.Interop.Excel.Application();

                if (excel == null)
                    throw new Exception("There is not an Excel application on your computer!");

                workbook = excel.Workbooks.Open(fullfilename, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                if (workbook.FileFormat != Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault)
                {
                    FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"),
                        string.Format("[{0}] - Starting convert file format of excel report - {1}...", DateTime.Now.ToString(), fullfilename));

                    string filepath = Path.GetDirectoryName(fullfilename);
                    newfilename = Path.Combine(filepath, String.Format("{0}_{1}",
                        Path.GetFileNameWithoutExtension(fullfilename),
                        DateTime.Now.ToString("yyyyMMdd_HHmmss")));

                    SetTitusClassification(ref workbook);
                    workbook.SaveAs(newfilename, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault);

                    fullfilename = Path.ChangeExtension(newfilename, ".xlsx");          // Change the extension file name as Excel 2010's format

                    FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"),
                        string.Format("[{0}] - Done!", DateTime.Now.ToString()));
                }

                workbook.Close();
                excel.Quit();

                return fullfilename;
            }

            catch (System.Exception e)
            {
                log.Info(e.Message);
                throw e;
            }

            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);

                excel = null;
                workbook = null;

                GC.Collect();
            }
        }

        // Add some properties for Titus Classification into Excel file.
        public static void SetTitusClassification(ref Workbook workBook)
        {
            SetDocumentProperty(ref workBook, "DellClassification", "Internal Use");
            SetDocumentProperty(ref workBook, "TitusReset", "Reset");
        }

        // Setup a customer property for excel file.
        public static void SetDocumentProperty(ref Workbook workBook,
            string propertyName, string propertyValue)
        {
            dynamic oDocCustomProps = workBook.CustomDocumentProperties;
            Type typeDocCustomProps = oDocCustomProps.GetType();

            dynamic[] oArgs = { propertyName, false, MsoDocProperties.msoPropertyTypeString, propertyValue };

            typeDocCustomProps.InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod, null,
                oDocCustomProps, oArgs);
        }

        public static dynamic GetDocumentProperty(ref Workbook workBook,
            string propertyName, MsoDocProperties type)
        {
            dynamic returnVal = null;

            dynamic oDocCustomProps = workBook.CustomDocumentProperties;
            Type typeDocCustomProps = oDocCustomProps.GetType();

            dynamic returned = typeDocCustomProps.InvokeMember("Item",
                BindingFlags.Default |
                BindingFlags.GetProperty, null,
                oDocCustomProps, new object[] { propertyName });

            Type typeDocAuthorProp = returned.GetType();
            returnVal = typeDocAuthorProp.InvokeMember("Value",
                BindingFlags.Default |
                BindingFlags.GetProperty,
                null, returned,
                new object[] { }).ToString();

            return returnVal;
        }
    }
}
