using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using GIC.Common.Model;
//using log4net;

namespace GIC.Common
{
    public class FileUtility
    {
        //private static ILog log = LogManager.GetLogger(typeof(FileUtility));

        public static bool IsValidTime(string fileName)
        {
            int waitTimeSpan = 10;
            FileInfo fi = new FileInfo(fileName);
            DateTime lastModified = fi.LastWriteTime;
            TimeSpan timeSpan = DateTime.Now - lastModified;

            if (timeSpan.TotalSeconds >= waitTimeSpan) return true;
            return false;
        }

        public static bool IsFileExisting(string filename, string targetfolder)
        {
            DirectoryInfo dir = new DirectoryInfo(targetfolder);

            foreach (FileInfo fi in dir.GetFiles())
            {
                if (filename == fi.Name) return true;
            }

            return false;
        }

        public static string LoadTextFile(string path)
        {
            string text = null;

            try
            {
                using (StreamReader reader = new FileInfo(path).OpenText())
                {
                    text = reader.ReadToEnd();
                }

                return text;
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(
                    string.Format("The file: {0} cannot be found due to {1}", path, ex.Message));
            }
            catch (FileLoadException ex)
            {
                throw new FileLoadException(
                    string.Format("Loading the file {0} failed with {1}", path, ex.Message));
            }
            catch
            {
                throw;
            }
        }

        public static void MoveFile(string targefolder, FileInfo filename)
        {
            string newFileName = String.Empty;

            if (string.IsNullOrEmpty(targefolder))
            {
                throw new ArgumentNullException("The targe folder of file cannot be empty!");
            }

            if (filename == null)
            {
                throw new ArgumentNullException("File name cannot be empty!");
            }

            try
            {
                newFileName = string.Format("{0}_{1}{2}",
                    filename.Name.Substring(0, filename.Name.Length),
                    DateTime.Now.ToFileTime().ToString(),
                    filename.Extension);
                filename.MoveTo(Path.Combine(targefolder, newFileName));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Moving the file - {0} failed: {1}", filename.Name, ex.Message));
            }
        }

        public static void MoveFile(string targetfolder, string fullfilename)
        {
            string newFileName = String.Empty;

            if (!Directory.Exists(targetfolder))
            {
                Directory.CreateDirectory(targetfolder);
            }

            if (fullfilename == null)
            {
                throw new ArgumentNullException("File name cannot be empty!");
            }

            try
            {
                string filename = Path.GetFileName(fullfilename);
                string fileextension = Path.GetExtension(fullfilename);
                string newfilename = string.Format("{0}_{1}{2}", filename, DateTime.Now.ToFileTime().ToString(), fileextension);

                File.Move(fullfilename, Path.Combine(targetfolder, newfilename));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Moving the file - {0} failed: {1}", fullfilename, ex.Message));
            }
        }

        public static void MoveFile(string targetfolder, string fullfilename, string newfilename)
        {
            string newFileName = String.Empty;

            if (!Directory.Exists(targetfolder))
            {
                Directory.CreateDirectory(targetfolder);
            }

            if (fullfilename == null)
            {
                throw new ArgumentNullException("File name cannot be empty!");
            }

            try
            {
                //newFileName = string.Format("{0}_{1}", filename.Name, DateTime.Now.ToFileTime().ToString());
                //string filename = Path.GetFileName(fullfilename);
                File.Move(fullfilename, Path.Combine(targetfolder, newfilename));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Moving the file - {0} failed: {1}", fullfilename, ex.Message));
            }
        }

        public static void CopyFile(string targetfolder, string sourcefilename, string targetfilename)
        {
            if (!Directory.Exists(targetfolder))
            {
                Directory.CreateDirectory(targetfolder);
            }

            if (sourcefilename == null)
            {
                throw new ArgumentNullException("File name cannot be empty!");
            }

            try
            {
                File.Copy(sourcefilename, Path.Combine(targetfolder, targetfilename));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Copy the file - {0} failed: {1}", sourcefilename, ex.Message));
            }
        }

        //public static ReportFile LoadReportFileConfig()
        //{
        //    string xml = LoadTextFile(@".\ReportFile.xml");
        //    return (ReportFile)SerializationUtility.DeSerialize(typeof(ReportFile), xml);
        //}

        public static ReportingConfig LoadReportingConfig()
        {
            try
            {
                string xml = LoadTextFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportingConfig.xml"));
                return (ReportingConfig)SerializationUtility.DeSerialize(typeof(ReportingConfig), MiscUtility.Char2HTML(xml));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void SaveFile(string filename, string text)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filename, true))
                {
                    writer.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Saving the file - {0} failed: {1}", filename, ex.Message));
            }
        }

        public static List<string> GetFileNameList(string folderpath)
        {
            string filename = string.Empty;
            string fullfilename = string.Empty;

            string keystring = "STOCKSTATUS";
            List<string> filenamelist = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(folderpath);
            foreach (FileInfo fi in dir.GetFiles())
            {
                if ((fi.Name.ToUpper().Contains(keystring)))
                {
                    fullfilename = ExcelFileUtility.SaveAsStandardFileFormat(fi.FullName);
                    filenamelist.Add(fullfilename);
                }
            }

            return filenamelist;
        }
    }
}
