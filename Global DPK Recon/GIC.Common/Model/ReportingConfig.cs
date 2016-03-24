using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIC.Common
{
    public class ReportingConfig
    {
        private DPKTranHistoryReport[] dpktranhistoryreports;
        
        public DPKTranHistoryReport[] DPKTranHistoryReports
        {
            get { return dpktranhistoryreports; }
            set { dpktranhistoryreports = value; }
        }

        public DPKTranHistoryReport this[string reportname]
        {
            get
            {
                foreach (DPKTranHistoryReport dpktranhistoryreport in dpktranhistoryreports)
                {
                    if (dpktranhistoryreport.ReportName.Equals(reportname.Trim().ToUpper()))
                        return dpktranhistoryreport;
                }

                return null;
            }
        }
    }

    public class DPKTranHistoryReport
    {
        private string reportname;
        private string keycharsinfilename;
        private string targettablename;
        private string sourcefolder;
        private string dayofweek;
        private string starttime;

        public string ReportName
        {
            get
            {
                return this.reportname;
            }
            set
            {
                if (value != null)
                {
                    this.reportname = value.Trim().ToUpper();
                }
            }
        }

        public string KeyCharsInFileName
        {
            get
            {
                return this.keycharsinfilename;
            }
            set
            {
                if (value != null)
                {
                    this.keycharsinfilename = value.Trim().ToUpper();
                }
            }
        }

        public string TargetTableName
        {
            get
            {
                return this.targettablename;
            }
            set
            {
                if (value != null)
                    this.targettablename = value.Trim().ToUpper();
            }
        }

        public string SourceFolder
        {
            get
            {
                return this.sourcefolder;
            }
            set
            {
                if (value != null)
                    this.sourcefolder = value.Trim().ToUpper();
            }
        }

        public string DayOfWeek
        {
            get
            {
                return this.dayofweek;
            }
            set
            {
                if (value != null)
                    this.dayofweek = value.Trim().ToUpper();
            }
        }

        public string StartTime
        {
            get
            {
                return this.starttime;
            }
            set
            {
                if (value != null)
                    this.starttime = value.Trim().ToUpper();
            }
        }    
    }
}
