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
using VGCore;
using System.Net.Mail; 


namespace QRZPrinter
{
    public partial class ADIF_Label_Printer : Form
    {
        CorelDRAW.Application app = new CorelDRAW.Application();


        public string PrinterName = "";
        public string LogoPath = "";
        public string username = "";
        public string password = "";
        private System.Collections.ArrayList qsos = new System.Collections.ArrayList();
        public string adifdata = "";
        public string[] qsofields = Settings.Default.QSOFields.Split(',');
        private QRZPrinter.GlobalClassSmall global = new QRZPrinter.GlobalClassSmall();

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

            if (Settings.Default.HRD_SERVER.Length > 0)
            {
                textBox5.Text = Settings.Default.HRD_SERVER;
            }

            if (Settings.Default.HRD_USER.Length > 0)
            {
                textBox3.Text = Settings.Default.HRD_USER;
            }

            //TODO - Secure/encrypt this
            if (Settings.Default.HRD_PASSWORD.Length > 0)
            {
                textBox4.Text = Settings.Default.HRD_PASSWORD;
            }

            listView1.Items.Clear();
            listView1.View = System.Windows.Forms.View.Details;
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
            GlobalClassSmall gc = new GlobalClassSmall();

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


            gc.username = this.username;
            gc.password = this.password;
            string k = gc.GetKey();



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
                //if (gc.getQSLByMail(li.Text,k).Equals("1"))
                //{
                if (li.SubItems[1].Text.Length == 0)
                {
                    li.SubItems[1].Text = gc.getNameOnly(li.Text,k);
                }

                if (!Settings.Default.DONOTEMAIL.ToUpper().Contains("," + li.Text.ToUpper() + ","))
                {
                    li.SubItems[14].Text = gc.getEmailOnly(li.Text, k);
                    listView1.Items.Add(li);
                }
                //}
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

        private void button4_Click(object sender, EventArgs e)
        {
            listView1.Columns.Clear();
            listView1.Items.Clear();
            listView1.View = System.Windows.Forms.View.Details;
            listView1.FullRowSelect = true;
            listView1.MultiSelect = true;

            foreach (string s in qsofields)
            {
                if (s.Length > 0)
                {
                    listView1.Columns.Add(s);
                }
            }


            HRDInterface.callsign = textBox2.Text;
            HRDInterface.u = textBox3.Text;
            HRDInterface.p = textBox4.Text;
            HRDInterface.s = textBox5.Text;

            System.Collections.ArrayList q = HRDInterface.test();


            for (int x = 0; x < q.Count; x++)
            {

                try
                {
                    QSOData d = (QSOData)q[x];


                    ListViewItem li = new ListViewItem();
                    li.Text = d.callsign; 
                    li.SubItems.Add(d.name);
                    li.SubItems.Add(d.date);
                    li.SubItems.Add(d.date);
                    li.SubItems.Add(d.sent);
                    li.SubItems.Add(d.rcvd);
                    li.SubItems.Add(d.mode);
                    li.SubItems.Add(d.qth);
                    li.SubItems.Add(d.band);
                    li.SubItems.Add(d.freq);
                    li.SubItems.Add("");//skcc
                    li.SubItems.Add("");//TXPWR
                    li.SubItems.Add(d.comment);//TXPWR

                    if (d.QSL_SENT.Equals("Y"))
                    {
                        li.BackColor = System.Drawing.Color.OrangeRed;
                    }

                    listView1.Items.Add(li);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            }

            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Settings.Default.HRD_SERVER = textBox5.Text;
            Settings.Default.HRD_PASSWORD = textBox4.Text;
            Settings.Default.HRD_USER = textBox3.Text;
            Settings.Default.Save();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0)
            {
                return;
            }
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }

            System.Collections.ArrayList aaddress = new System.Collections.ArrayList();

            if (this.ckAddress.Checked)
            {
               
                foreach (ListViewItem li in listView1.SelectedItems)
                {
                    if (li.Text.Trim().Length > 0)
                    {
                        if (!aaddress.Contains(li.Text))
                        {
                            aaddress.Add(li.Text);
                        }
                        
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

                    //make sure an address was valid - means they accept mail qsl. 
                   
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

        private void button7_Click(object sender, EventArgs e)
        {

            app.OpenDocument(@"c:\tmp\sypk4202.cdr");
            foreach (ListViewItem li in listView1.Items)
            {
                if (li.Text.Equals(""))
                {
                    continue;
                }
                if (li.SubItems[14].Text.ToString().Trim().Length == 0)
                {
                    continue;
                }
                //ActiveView av = new ActiveView();
                string mycall = li.Text;
                string myname = li.SubItems[1].Text;
                string myemail = li.SubItems[14].Text;
                string rst = li.SubItems[4].Text;
                string mytime = li.SubItems[3].Text;
                string mydate = li.SubItems[2].Text;
                string band = li.SubItems[8].Text;
                string mode = li.SubItems[6].Text;


                //string mydate = "2022-10-10";
                //string myband = "20m";
                //string myrst = "599";
                //string mytime = "11:11:!!";
                //string mymode = "CW";
                //MessageBox.Show("TEST3");
                try
                {
                    
                    app.ActivePage.SelectableShapes.All().CreateSelection();

                    ShapeRange sr1 = app.ActiveSelectionRange;

                    Document nd = sr1.CreateDocumentFrom(false);
                    nd.Activate();


                    replacetext(band.Trim(), "<BAND>");
                    replacetext(mode.Trim(), "<MD>");
                    replacetext(mydate.Trim(), "<DATE>");
                    replacetext(mytime.Trim(), "<UTC>");
                    replacetext(rst.Trim(), "<RST>");

                    replacetext(mycall.Trim(), "<CALL>");
                    replacetext(myname.Trim(), "<NAME>");
                    string myfile = "c:\\tmp\\" + mycall + "_.png";
                    nd.Export(myfile , cdrFilter.cdrPNG);
                    nd.Close();
                    //myemail = "wrmichael@Hotmail.com";
                    
                    
                    this.MySendMail(myname, mycall, myfile,myemail);
                    try
                    {

                        System.IO.File.Delete(myfile);

                    }
                    catch (Exception fex)
                    {
                        //ignore this...  
                        System.Console.WriteLine(fex.Message);

                    }



                    //replacetext(myband, "<BAND>");
                    //replacetext(mymode, "<MD>");
                    //replacetext(mydate, "<DATE>");
                    //replacetext(mytime, "<TIME>");
                    //replacetext(myrst, "<RST>");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message);

                }
                
            }
            app.ActiveDocument.Close();
            app.Quit();
        }

        void MySendMail(string name, string call, string attachment, string email)
        {
            System.Net.Mail.MailMessage mm = new System.Net.Mail.MailMessage();

            mm.To.Add(email);
            mm.From = new MailAddress("wrmichael@hotmail.com");
            mm.Subject = "SYP Parks on the air QSO";
            mm.Body = name + "\r\n" + "Thank you for working my station during the Parks On The Air SYP event.  attached is a certificate I made with QSL data.  If you would like receive a paper QSL card,  you can request one via email or send a QSL card in the mail.\r\nIf you would not like to receive emails like this from please let me know and I will add your callsign to my exclusion list.   \r\n\r\n73 DE AC9HP Wayne Michael";
            
            mm.Attachments.Add(new Attachment( attachment));

            SmtpClient smtp = new SmtpClient("smtp-mail.outlook.com", 587);
            System.Net.NetworkCredential nc = new System.Net.NetworkCredential("wrmichael@hotmail.com", "Ankle45DeepSecurity");
            smtp.Credentials = nc;
            try
            {
                smtp.EnableSsl = true;
                smtp.Send(mm);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public void replacetext(string t, string r)
        {

            string txtFIND;
            string txtREPLACE;

            txtFIND = r;

            txtREPLACE = t;
            foreach (Page p in app.ActiveDocument.Pages)
            {
                p.TextReplace(txtFIND, txtREPLACE, true, false);
            }

        }
        public void NewCopy()
        {

        }


    }
}
