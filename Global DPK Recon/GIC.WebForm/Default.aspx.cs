using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using GIC.Common;
using GIC.DataAccess;

namespace GIC.WebForm
{
    public partial class _Default : System.Web.UI.Page
    {
        private DataTable datatable = null;
        private string selectedsource = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void viewreport_Click(object sender, EventArgs e)
        {
            selectedsource = source.SelectedValue.Trim();
            string selectedccn = ccn.SelectedValue.Trim();

            string serialkeynum = string.Empty;
            if (serialkeynumber.Text.Trim().Length != 0)
                serialkeynum = serialkeynumber.Text.Trim();
            else
                serialkeynum = "NULL";

            string partnum = string.Empty;
            if (partnumber.Text.Trim().Length != 0)
                partnum = partnumber.Text.Trim();
            else
                partnum = "NULL";

            string selectedfromstatus = string.Empty;
            if (fromstatus.SelectedValue.Trim().Length != 0)
                selectedfromstatus = fromstatus.SelectedValue.Trim();
            else
                selectedfromstatus = "NULL";

            string selectedtostatus = string.Empty;
            if (tostatus.SelectedValue.Trim().Length != 0)
                selectedtostatus = tostatus.SelectedValue.Trim();
            else
                selectedtostatus = "NULL";

            //string selectedfromdate = fromdatecalendar.SelectedDate.ToShortDateString() + " 0:0:0";
            //string selectedtodate = todatecalendar.SelectedDate.ToShortDateString() + " 23:59:59";

            var fromdate = fromdatepicker.Value;
            var todate = todatepicker.Value;

            //DataTable datatable = null;
            string connectionstring = ConfigurationManager.ConnectionStrings["GlobaDPKDatabase"].ConnectionString;
            using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(connectionstring)))
            {
                if (selectedsource.ToUpper().Contains("LKM"))
                {
                    datatable =
                        dao.GetLKMDPKTransactionHistory(
                            selectedccn, 
                            MiscUtility.AddSingleQuotation(serialkeynum), 
                            MiscUtility.AddSingleQuotation(partnum), 
                            selectedfromstatus, 
                            selectedtostatus,
                            fromdate,
                            todate);                    
                }
                else if (selectedsource.ToUpper().Contains("GLOVIA"))
                {
                    datatable =
                        dao.GetGloviaDPKTransactionHistory(
                            selectedccn,
                            MiscUtility.AddSingleQuotation(serialkeynum),
                            MiscUtility.AddSingleQuotation(partnum),
                            selectedfromstatus,
                            selectedtostatus,
                            fromdate,
                            todate);
                }

                if (datatable.Rows.Count == 0)
                {
                    ShowClientMessage("No data found!");
                    return;
                }

                if (datatable.Rows.Count <= 1000)
                {
                    dataview.DataSource = datatable;
                    dataview.DataBind();
                }
                else
                {
                    //dataview.DataSource = datatable;
                    //dataview.DataBind();

                    if (!ExportDataGridViewToExcel(selectedsource, string.Format("DPK_Transaction_History_{0}.xls", DateTime.Now.ToString("yyyyMMdd_HHmm"))))
                        ShowClientMessage("Failed to export data!");
                    else
                        ShowClientMessage("Export data successful!");
                }
            }
        }

        protected void exportreport_Button_Click(object sender, EventArgs e)
        {
            if (!ExportDataGridViewToExcel(selectedsource, string.Format("DPK_Transaction_History_{0}.xls", DateTime.Now.ToString("yyyyMMdd_HHmm"))))
                ShowClientMessage("Failed to export data!");
            else
                ShowClientMessage("Export data successful!");
        }

        //private bool ExportDataGridViewToExcel(string filename)
        //{
        //    bool flag = false;

        //    try
        //    {
        //        Response.Clear();
        //        Response.Buffer = true;

        //        HttpContext.Current.Response.Charset = "GB2312";
        //        HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
        //        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
        //        HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=\""
        //            + System.Web.HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8));

        //        dataview.Page.EnableViewState = false;

        //        StringWriter tw = new StringWriter();
        //        HtmlTextWriter hw = new HtmlTextWriter(tw);

        //        dataview.AllowPaging = false;

        //        dataview.RenderControl(hw);

        //        Response.Output.Write(tw.ToString());
        //        Response.Flush();
        //        Response.End();

        //        flag = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        flag = false;
        //        throw ex;
        //    }

        //    return flag;
        //}

        private bool ExportDataGridViewToExcel(string selectedsource, string filename)
        {
            bool flag = false;
            string stringline = string.Empty;

            try
            {
                Response.Clear();
                Response.Buffer = true;

                HttpContext.Current.Response.Charset = "GB2312";
                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=\""
                    + System.Web.HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8));
                                
                StringWriter sw = new StringWriter();

                //if (selectedsource.ToUpper().Contains("GLOVIA"))
                //    sw.WriteLine("GLV_CCN, GLV_PN, GLV_DPK_SN, GLV_REF_NO, LKM_TRANS_TYPE, GLV_TRANS_DATE_AUS, GLV_TRANS_DATE_REG, GLV_FROM_ML, GLV_FROM_SR, GLV_TO_ML, GLV_TO_SR, LKM_FROM_STATUS, LKM_TO_STATUS, LKM_UPDATE_DATE_AUS, LKM_UPDATE_DATE_REG, LKM_TO_FACILITY, LKM_KEY_TYPE, GLV_MSG_ID, SNAP_SHOT_DATE");
                //else if (selectedsource.ToUpper().Contains("LKM"))
                //    sw.WriteLine("LKM_CCN, LKM_PN, LKM_DPK_SN, LKM_FROM_STATUS, LKM_TO_STATUS, LKM_CHANGE_DATE_AUS, LKM_CHANGE_DATE_REG, LKM_TO_FACILITY, LKM_KEY_TYPE, LKM_MSG_ID, GLV_REF_NO, LKM_TRANS_TYPE, GLV_TRANS_DATE_AUS, GLV_TRANS_DATE_REG, GLV_FROM_ML, GLV_FROM_SR, GLV_TO_ML, GLV_TO_SR, SNAP_SHOT_DATE");
                
                //foreach (DataRow datarow in datatable.Rows)
                //{
                //    sw.WriteLine(datarow[0] + "," + datarow[1] + "," + datarow[2] + "," + datarow[3] + "," +
                //                 datarow[4] + "," + datarow[5] + "," + datarow[6] + "," + datarow[7] + "," +
                //                 datarow[8] + "," + datarow[9] + "," + datarow[10] + "," + datarow[11] + "," +
                //                 datarow[12] + "," + datarow[13] + "," + datarow[14] + "," + datarow[15] + "," +
                //                 datarow[16] + "," + datarow[17] + "," + datarow[18] + ",");
                //}

                for (int i = 0; i < datatable.Columns.Count; i++)
                {
                    stringline = stringline + datatable.Columns[i].ColumnName.ToString() + Convert.ToChar(9);

                }

                sw.WriteLine(stringline);
                stringline = "";

                for (int i = 0; i < datatable.Rows.Count; i++)
                {
                    for (int j = 0; j < datatable.Columns.Count; j++)
                    {
                        stringline = stringline + datatable.Rows[i][j].ToString() + Convert.ToChar(9);
                    }

                    sw.WriteLine(stringline);
                    stringline = "";
                }

                sw.Close();
                
                Response.Output.Write(sw);
                Response.Flush();

                Response.End();
                flag = true;
            }
            catch (Exception ex)
            {
                flag = false;
                throw ex;
            }

            return flag;
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
        }

        public void ShowClientMessage(string message)
        {
            ClientScript.RegisterStartupScript(GetType(), "Message", string.Format("<script>alert('{0}');</script>", message));
        }

        protected void dataview_DataBinding(object sender, EventArgs e)
        {   
        }
    }
}
