﻿using QRZPrinter.Properties;
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

        private QRZPrinter.GlobalClass global = new QRZPrinter.GlobalClass();

        public ADIF_Label_Printer()
        {
            InitializeComponent();
        }

        private void ADIF_Label_Printer_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
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
                call = getvalue(r,"CALL");
                freq = getvalue(r,"FREQ");
                mode = getvalue(r, "MODE");

                power = getvalue(r, "TX_PWR");
                time = getvalue(r, "TIME_ON");
                sent = getvalue(r, "RST_SENT");
                rcv = getvalue(r, "RST_RCVD");
                qth = getvalue(r, "QTH");
                date = getvalue(r, "QSO_DATE");
                //listView1.Columns.Add("Call Sign");
                //listView1.Columns.Add("Date");
                //listView1.Columns.Add("Frequency");
                //listView1.Columns.Add("Mode");
                //listView1.Columns.Add("Power");
                //listView1.Columns.Add("Time");
                //listView1.Columns.Add("Sent RST");
                //listView1.Columns.Add("Received RST");
                //listView1.Columns.Add("QTH");



                ListViewItem li = new ListViewItem();
                li.Text = call;
                li.SubItems.Add(date);
                li.SubItems.Add(freq);
                li.SubItems.Add(mode);
                li.SubItems.Add(power);
                li.SubItems.Add(time);
                li.SubItems.Add(sent);
                li.SubItems.Add(rcv);
                li.SubItems.Add(qth);
                listView1.Items.Add(li);
            }
        }

        public string getvalue(string s, string f)
        {
            string t = "";
            string ns="";
            int cc = 0;
            if (s.IndexOf(f) > -1)
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

                global.qsos = qsos;
                global.PrinterName = this.PrinterName;
                //global.LogoPath = this.LogoPath;
                //global.username = this.username;
                //global.password = this.password;
                global.printQSO();
            }


        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
