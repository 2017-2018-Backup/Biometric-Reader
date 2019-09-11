using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using System.IO;

namespace Biometric_Reader
{
    public partial class BiometricReader : Form
    {
        public BiometricReader()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = ".CSV|*.CSV";
            openFileDialog.ShowDialog();
            if (string.IsNullOrWhiteSpace(openFileDialog.FileName))
                return;
            txtInputFilePath.Text = openFileDialog.FileName;
            openFileDialog.Dispose();
        }

        private void btnFolderBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();
            if (string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                return;
            txtOpDirectory.Text = folderBrowser.SelectedPath;
            folderBrowser.Dispose();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnProceed_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtInputFilePath.Text) || (string.IsNullOrWhiteSpace(txtOpDirectory.Text)) || (string.IsNullOrWhiteSpace(txtOpFileName.Text)))
                    MessageBox.Show("Field values should not be empty.", GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Information);


                var lines = File.ReadAllLines(txtInputFilePath.Text);



                string reportFormat = string.Empty;
                string durationFrom = string.Empty;


                DataTable dt = new DataTable();
                dt.Columns.Add("Card Id");
                dt.Columns.Add("Employee Name");
                dt.Columns.Add("Department");
                dt.Columns.Add("Date");
                dt.Columns.Add("First In");
                dt.Columns.Add("Last Out");
                dt.Columns.Add("Total Hours");

                for (int i = 4; i < lines.Count(); i++)
                {
                    try
                    {
                        var details = lines[i].ToString().Split(',');
                        if (details.Count() == 0 || details[0] == string.Empty)
                            continue;

                        string dateVal = details[3].Replace('"', ' ');
                        if (!string.IsNullOrEmpty(dateVal) && !string.IsNullOrWhiteSpace(dateVal))
                            dateVal = Convert.ToDateTime(dateVal).ToString("dd/MM/yyyy");

                        string inTime = details[4].Replace('"', ' '); ;
                        if (!string.IsNullOrEmpty(inTime) && !string.IsNullOrWhiteSpace(inTime))
                            inTime = Convert.ToDateTime(inTime).ToString("HH:mm");

                        string outTime = details[5].Replace('"', ' '); ;
                        if (!string.IsNullOrEmpty(outTime) && !string.IsNullOrWhiteSpace(outTime))
                            outTime = Convert.ToDateTime(outTime).ToString("HH:mm");

                        dt.Rows.Add(details[0].Replace('"', ' '), details[1].Replace('"', ' '), details[2].Replace('"', ' '), dateVal, inTime, outTime, details[6].Replace('"', ' '));

                    }
                    catch { }
                }

                if (dt.Rows.Count == 0)
                    return;

                // xlApp.Quit();

                WriteCSV(dt, "Format1");
                WriteCSV(dt, "Format2");

                MessageBox.Show("Successfully generated." + Environment.NewLine + "Output File - Format1  exists in :" + Path.Combine(txtOpDirectory.Text, txtOpFileName.Text + "_Format1.csv") + Environment.NewLine + "Output File - Format2 exists in " + Path.Combine(txtOpDirectory.Text, txtOpFileName.Text + "_Format2.csv"), GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtInputFilePath.Clear();
                txtOpDirectory.Clear();
                txtOpFileName.Clear();
            }
            catch (Exception ex)
            {

            }

        }

        private void WriteCSV(DataTable dt, string format)
        {
            StringBuilder sbCSV = new StringBuilder();

            //sbCSV.Append(reportFormat.Substring(reportFormat.IndexOf(":"), reportFormat.Length - reportFormat.IndexOf(":")));
            //sbCSV.Append(durationFrom.Substring(durationFrom.LastIndexOf(":"), durationFrom.Length - durationFrom.LastIndexOf(":")));

            //sbCSV.Append(Environment.NewLine);
            if (format.Equals("Format1"))
                sbCSV.Append("Employee Number,In time with date,Out time with date,Date");
            else
                sbCSV.Append("Employee Number,Punch DateTime,Type");
            sbCSV.Append(Environment.NewLine);

            foreach (DataRow dr in dt.Rows)
            {
                if (string.IsNullOrWhiteSpace(dr["Card Id"].ToString()))
                    continue;
                if (format.Equals("Format1"))
                    sbCSV.Append(string.Format("{0},{1} {2},{3} {4},{5}", dr["Card Id"].ToString(), !string.IsNullOrWhiteSpace(dr["Date"].ToString()) ? Convert.ToDateTime(dr["Date"].ToString()).ToString("dd-MMM-yyyy") : string.Empty, !string.IsNullOrWhiteSpace(dr["First In"].ToString()) ? Convert.ToDateTime(dr["First In"].ToString()).ToString("HH:mm") : string.Empty, Convert.ToDateTime(dr["Date"].ToString()).ToString("dd-MMM-yyyy"), !string.IsNullOrWhiteSpace(dr["Last Out"].ToString()) ? Convert.ToDateTime(dr["Last Out"].ToString()).ToString("HH:mm") : string.Empty, !string.IsNullOrWhiteSpace(dr["Date"].ToString()) ? Convert.ToDateTime(dr["Date"].ToString()).ToString("dd-MMM-yyyy") : string.Empty));
                else
                {
                    sbCSV.Append(string.Format("{0},{1} {2},{3}", dr["Card Id"].ToString(), !string.IsNullOrWhiteSpace(dr["Date"].ToString()) ? Convert.ToDateTime(dr["Date"].ToString()).ToString("dd-MMM-yyyy") : string.Empty, !string.IsNullOrWhiteSpace(dr["First In"].ToString()) ? Convert.ToDateTime(dr["First In"].ToString()).ToString("HH:mm") : string.Empty, "IN"));
                    sbCSV.Append(Environment.NewLine);
                    sbCSV.Append(string.Format("{0},{1} {2},{3}", dr["Card Id"].ToString(), !string.IsNullOrWhiteSpace(dr["Date"].ToString()) ? Convert.ToDateTime(dr["Date"].ToString()).ToString("dd-MMM-yyyy") : string.Empty, !string.IsNullOrWhiteSpace(dr["Last Out"].ToString()) ? Convert.ToDateTime(dr["Last Out"].ToString()).ToString("HH:mm") : string.Empty, "OUT"));
                }
                sbCSV.Append(Environment.NewLine);
            }

            sbCSV.Append(Environment.NewLine);

            File.WriteAllText(Path.Combine(txtOpDirectory.Text, txtOpFileName.Text + "_" + format + ".csv"), sbCSV.ToString());

            sbCSV.Clear();
        }
    }
}
