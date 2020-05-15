using QRZPrinter.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace QRZPrinter
{
    public partial class ADIF_Label_Printer : Form
    {

        public string PrinterName = "";
        public string LogoPath = "";
        public string username = "";
        public string password = "";
        private System.Collections.ArrayList qsos = new System.Collections.ArrayList();
        public string adifdata = "";
        public string[] qsofields = Settings.Default.QSOFields.Split(',');
        private QRZPrinter.GlobalClass global = new QRZPrinter.GlobalClass();

        public ADIF_Label_Printer()
        {
            InitializeComponent();
        }

        private void ADIF_Label_Printer_Load(object sender, EventArgs e)
        {
            if (Settings.Default.LabelTemplate.Length > 0)
            {
                textBox1.Text = Settings.Default.LabelTemplate;

            }
            
            listView1.Items.Clear();
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.MultiSelect = true; 

            foreach (string s in qsofields)
            {
                if (s.Length > 0)
                {
                    listView1.Columns.Add(s);  
                }
            }
            /*
            listView1.Columns.Add("Call Sign");
            listView1.Columns.Add("Date");
            listView1.Columns.Add("Frequency");
            listView1.Columns.Add("Mode");
            listView1.Columns.Add("Power");
            listView1.Columns.Add("Time");
            listView1.Columns.Add("Sent RST");
            listView1.Columns.Add("Received RST");
            listView1.Columns.Add("QTH");
            listView1.View = View.Details;
            */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (Settings.Default.ADIFOPath.Trim().Length > 0)
            {
                ofd.FileName = Settings.Default.ADIFOPath;
            }
            ofd.ShowDialog();

            if (ofd.FileName.Trim().Length > 0)
            {
                label1.Text = ofd.FileName;
            }
            else
            { return;
            }
            string adfi = openADFI(label1.Text).ToUpper();

            string call;
            string date;
            string freq;
            string mode;
            string power;
            string time;
            string sent;
            string rcv;
            string qth;
            //string name;
            //string band;

            foreach (string r in adfi.Split(new string[] { "<EOR>" },StringSplitOptions.None))
            {
               

                ListViewItem li = new ListViewItem();

                int x = 0;
                string special="";

                foreach (string s in qsofields)
                {
                    special = "";
                    x++;
                    if (x == 1)
                    {
                        li.Text = getvalue(r, s);
                    }
                    else
                    {

                        if (s.Equals("QSO_DATE"))
                        {

                            //formatting may not be universal so I will do it for the SKCC logger but might want to check your ADFI files.. 
                            if (getvalue(r, s).Length >= 8)
                            {
                                special = getvalue(r, s).Substring(0, 4) + "-" + getvalue(r, s).Substring(4, 2) + "-" + getvalue(r, s).Substring(6);
                            }
                            else
                            {
                                special = getvalue(r, s);
                            }

                        }
                        if (s.Equals("TIME_ON"))
                        {
                            
                            if (getvalue(r,s).Length >= 6)
                            {
                                //reformat 
                                special = getvalue(r, s).Substring(0, 2) + ":" + getvalue(r, s).Substring(2, 2) + ":" + getvalue(r, s).Substring(4) + " Z";  // may not want Z.. I do.. 
                            }
                        }
                        if (special.Length > 0)
                        {
                            li.SubItems.Add(special);
                        }
                        else
                        {
                            li.SubItems.Add(getvalue(r, s));
                        }
                    
                    }
                    
                
                }
                listView1.Items.Add(li);
                //li.Text = call;


            }
        }

        public string getvalue(string s, string f)
        {

            string t = "";
            string ns="";
            int cc = 0;
            if (s.IndexOf("<"+f + ":") > 0)

            {
                //look for value 
                t  = s.Substring(s.IndexOf("<" + f + ":") + f.Length +2);
                
                //t = t.Substring(0, t.IndexOf(":"));
                cc = int.Parse(t.Substring(0,t.IndexOf(">")));
                t = t.Substring(t.IndexOf(">")+1);
                ns = t.Substring(0, cc);
                

            }
            return ns; 
        }

        public string openADFI(string f)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(f);
            string adfi = sr.ReadToEnd();
            sr.Close();
            this.adifdata = adfi; 
            return adfi;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                return;
            }
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }



            if (this.ckAddress.Checked)
            {
                System.Collections.ArrayList aaddress = new System.Collections.ArrayList();
                foreach (ListViewItem li in listView1.SelectedItems)
                {
                    if (li.Text.Trim().Length > 0)
                    {
                        aaddress.Add(li.Text);
                    }
                }

                global.callsigns = aaddress;
                global.PrinterName = this.PrinterName;
                global.LogoPath = this.LogoPath;
                global.username = this.username;
                global.password = this.password;
                global.printLabelsFromQRZ();

            }

            if (this.ckQSO.Checked)
            {


                System.Collections.ArrayList qsos = new System.Collections.ArrayList();
                string label = textBox1.Text; 



                foreach (ListViewItem li in listView1.SelectedItems)
                {
                    label = textBox1.Text;
                    for (int idx = 0; idx < li.SubItems.Count; idx++)
                    {                        
                        label = label.Replace("@" + listView1.Columns[idx].Text + "@", li.SubItems[idx].Text);
                    }

                    // take the value from the list and replace the value in the label. 

                    qsos.Add(label); 
                }

                /*
                System.Collections.ArrayList qsos = new System.Collections.ArrayList(); 

                foreach (ListViewItem li in listView1.SelectedItems)
                {
                    if (li.Text.Trim().Length > 0)
                    {
                        QSOData qd = new QSOData();

                        qd.callsign = li.Text;

                        //formatting may not be universal so I will do it for the SKCC logger but might want to check your ADFI files.. 
                        if (li.SubItems[1].Text.Length>=8)
                        {
                            qd.date = li.SubItems[1].Text.Substring(0, 4) + "-" + li.SubItems[1].Text.Substring(4, 2) + "-" + li.SubItems[1].Text.Substring(6);
                        } else
                        {
                            qd.date = li.SubItems[1].Text;
                        }
                        
                        qd.freq = li.SubItems[2].Text;

                        qd.mode = li.SubItems[3].Text;
                        //li.SubItems.Add(power);
                        //check format of time...
                        
                        qd.time = li.SubItems[5].Text;
                        if (qd.time.Length >= 6)
                        {
                            //reformat 
                            qd.time = qd.time.Substring(0, 2) + ":" + qd.time.Substring(2, 2) + ":" + qd.time.Substring(4) + " Z";  // may not want Z.. I do.. 
                        }

                        qd.sent = li.SubItems[6].Text;
                        qd.rcvd = li.SubItems[7].Text;
                        //li.SubItems.Add(qth);
                        
                        qsos.Add(qd);
                    }
                }
                */

                string line1 = "";
                string line2 = "";
                string line3 = "";
                string line4 = "";

                
                global.qsos = qsos;
                global.PrinterName = this.PrinterName;
                //global.LogoPath = this.LogoPath;
                //global.username = this.username;
                //global.password = this.password;                
                global.printQSOTemplate();


            }


        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Settings.Default.LabelTemplate = textBox1.Text;
            Settings.Default.Save();


        }
    }
}
