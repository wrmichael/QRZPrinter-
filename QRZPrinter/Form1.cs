using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Configuration;
using QRZPrinter.Properties;

namespace QRZPrinter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Settings.Default.CallSign = textBox1.Text;
            Settings.Default.PrinterName = sPrinter.Text;
            Settings.Default.ImageName = logopath.Text;
            Settings.Default.Save();


                WebClient client = new WebClient();

                // Add a user agent header in case the
                // requested URI contains a query.

                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                Stream data = client.OpenRead("http://xmldata.qrz.com/xml/current/?username=" + textBox1.Text + ";password=" + textBox2.Text + ";agent=q5.0");
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                
                data.Close();
                reader.Close();

            string key = getKey(s);
            label2.Text = "Key: " + key;

            if (key.ToUpper().Contains("<ERROR>"))
            {
                MessageBox.Show("An error occurred logging in.  Check password and/or internet connection");
                return;
            }

            System.Collections.ArrayList addresses = new System.Collections.ArrayList();
            foreach(string t in listBox1.Items)
            {

                if (t.Trim().Length == 0)
                { 
                    continue;
                }
                string a = getAddress(t,key);
                if (a.Contains("<ERROR>"))
                {
                    listBox2.Items.Add(t);
                } else
                {
                    addresses.Add(a);
                    listBox3.Items.Add(t);
                }

            }
            listBox1.Items.Clear();

            QRZPrinter.PrinterObejct p = new PrinterObejct();
            foreach (string amateur in addresses)
            {
                //System.Windows.Forms.MessageBox.Show(amateur);
                if (amateur.Length > 0)
                {
                    p.text = amateur;
                    p.photopath = logopath.Text;
                    p.PrinterName = sPrinter.Text;
                    p.Printing();
                }
            }
            


    }
        public string getAddress(string t, string k)
        {
            string newt = "";
            string url = "http://xmldata.qrz.com/xml/current/?s=" + k + ";callsign=" +t ;
            
            WebClient client = new WebClient();

            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            //Console.WriteLine(s);
            data.Close();
            reader.Close();
            string name = getvalue(s, "FNAME") + " " + getvalue(s, "NAME");

            newt = t.ToUpper() + "\r\n" + name.Trim() + "\r\n" ;
            if (newt.Equals(t.ToUpper() + "\r\n" + " " + "\r\n"))
            {
                newt = t.ToUpper() + "\r\n" + this.getvalue(s, "NAME") + "\r\n";

            }
            newt = newt + getAmateurAddress(s) + "\r\n";
            newt = newt + getAmateurCity(s) + " " + getAmateurState(s) + " " + getAmateurZip(s) + "\r\n";
            newt = newt + getAmateurCountry(s);
            
            return newt;
               
        }


        public string getvalue(string s, string f)
        {

            string t = "";
            string ns = "";
            int cc = 0;
            if (s.ToUpper().IndexOf("<" + f + ">") > 0)

            {

                ns = s.Substring(s.ToUpper().IndexOf("<" + f.ToUpper() + ">") + f.Length + 2);
                ns = ns.ToUpper().Split(new string[] { "</"+ f + ">" }, StringSplitOptions.RemoveEmptyEntries)[0];

            }
            return ns;
        }
        public string getfName(string s)
        {
            string news = "";
            if (s.ToUpper().IndexOf("<FNAME>") <= 0)
            {
                return "";
            }
            news = s.Substring(s.ToUpper().IndexOf("<FNAME>")+7);
            news = news.ToUpper().Split(new string[] { "</FNAME>"},StringSplitOptions.RemoveEmptyEntries)[0];
     //       < fname > Raymond G </ fname >
   //< name > Grob, Jr </ name >

            return news;
        }


        public string getAmateurAddress(string s)
        {
            string news = "";

            if (s.ToUpper().IndexOf("<ADDR1>") <= 0)
            {
                return "";
            }

                news = s.Substring(s.ToUpper().IndexOf("<ADDR1>") + 7);
            news = news.ToUpper().Split(new string[] { "</ADDR1>" }, StringSplitOptions.RemoveEmptyEntries)[0];
           /*
            * <addr1>916 NORTH ST APT 2</addr1>
                <addr2>Fremont</addr2>
            <state>OH</state>
            <zip>43420</zip>
            <country>United States</country>
            */

            return news;
        }

        public string getAmateurCity(string s)
        {
            string news = "";

            if (s.ToUpper().IndexOf("<ADDR2>") <= 0)
            {
                return "";
            }

            news = s.Substring(s.ToUpper().IndexOf("<ADDR2>") + 7);
            news = news.ToUpper().Split(new string[] { "</ADDR2>" }, StringSplitOptions.RemoveEmptyEntries)[0];
            /*
             * <addr1>916 NORTH ST APT 2</addr1>
                 <addr2>Fremont</addr2>
             <state>OH</state>
             <zip>43420</zip>
             <country>United States</country>
             */

            return news;
        }
        public string getAmateurState(string s)
        {
            string news = "";

            if (s.ToUpper().IndexOf("<STATE>") <=0 )
            {
                return "";
            }

            news = s.Substring(s.ToUpper().IndexOf("<STATE>") + 7);
            news = news.ToUpper().Split(new string[] { "</STATE>" }, StringSplitOptions.RemoveEmptyEntries)[0];
            /*
             * <addr1>916 NORTH ST APT 2</addr1>
                 <addr2>Fremont</addr2>
             <state>OH</state>
             <zip>43420</zip>
             <country>United States</country>
             */

            return news;
        }

        public string getAmateurZip(string s)
        {
            string news = "";
            if (s.ToUpper().IndexOf("<ZIP>") <= 0)
            {
                return "";
            }
            news = s.Substring(s.ToUpper().IndexOf("<ZIP>") + 5);
            news = news.ToUpper().Split(new string[] { "</ZIP>" }, StringSplitOptions.RemoveEmptyEntries)[0];
            /*
             * <addr1>916 NORTH ST APT 2</addr1>
                 <addr2>Fremont</addr2>
             <state>OH</state>
             <zip>43420</zip>
             <country>United States</country>
             */

            return news;
        }
        public string getAmateurCountry(string s)
        {
            string news = "";

            if (s.ToUpper().IndexOf("<COUNTRY>") <= 0)
            {
                return "";
            }

            news = s.Substring(s.ToUpper().IndexOf("<COUNTRY>") + 9);
            news = news.ToUpper().Split(new string[] { "</COUNTRY>" }, StringSplitOptions.RemoveEmptyEntries)[0];
            /*
             * <addr1>916 NORTH ST APT 2</addr1>
                 <addr2>Fremont</addr2>
             <state>OH</state>
             <zip>43420</zip>
             <country>United States</country>
             */

            return news;
        }



        public string getName(string s)
        {
            string news = "";
            if (s.IndexOf("<NAME>") <= 0)
            {
                return "";
            }
            news = s.Substring(s.ToUpper().IndexOf("<NAME>") + 6);
            news = news.ToUpper().Split(new string[] { "</NAME>" }, StringSplitOptions.RemoveEmptyEntries)[0];
            //       < fname > Raymond G </ fname >
            //< name > Grob, Jr </ name >

            return news;
        }

        public string getKey(string s)
        {
            if (s.IndexOf("<Key>") < 0)
            {
                return "";
            }
            string news = s.Substring(s.ToUpper().IndexOf("<KEY>") + 5);
            news = news.Split( new String[] { "</Key>" } , StringSplitOptions.RemoveEmptyEntries)[0];

            return news;
        
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logopath.Text = Settings.Default.ImageName;
            textBox1.Text = Settings.Default.QRZ_User;
            textBox2.Text = Settings.Default.QRZ_Password;

            int idx = 0;
            int sidx = 0;
            foreach (string s in PrinterSettings.InstalledPrinters)
            {
                idx = comboBox1.Items.Add(s);
                if (new PrinterSettings().PrinterName.Equals(s))
                {
                    sidx = idx;
                }
            }

            if (Settings.Default.PrinterName.Trim().Length == 0)
            {
                sPrinter.Text = new PrinterSettings().PrinterName;
                comboBox1.Text = sPrinter.Text;
                comboBox1.SelectedItem = sidx;
            }
            else
            {
                sPrinter.Text = Settings.Default.PrinterName;
            }

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                listBox1.Items.Add(textBox3.Text);
                textBox3.Text = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string calls = Clipboard.GetText();

            foreach(string s in calls.Split(new string[] { "\r\n"},StringSplitOptions.None))
            {
                listBox1.Items.Add(s);

            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox3.Text);
            textBox3.Text = "";
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            textBox3.Text = listBox1.SelectedItem.ToString();
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sPrinter.Text = comboBox1.SelectedItem.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ADIF_Label_Printer af = new ADIF_Label_Printer();
            af.PrinterName = sPrinter.Text;
            af.LogoPath = logopath.Text;
            af.username = textBox1.Text;
            af.password = textBox2.Text; 

            af.ShowDialog();
            af.Close();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            HRDInterface.test(); 

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Settings.Default.QRZ_Password = textBox2.Text;
            Settings.Default.QRZ_User = textBox1.Text;
            Settings.Default.Save(); 

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Settings.Default.CallSign = textBox1.Text;
            Settings.Default.PrinterName = sPrinter.Text;
            Settings.Default.ImageName = logopath.Text;
            Settings.Default.Save();


            WebClient client = new WebClient();

            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead("http://xmldata.qrz.com/xml/current/?username=" + textBox1.Text + ";password=" + textBox2.Text + ";agent=q5.0");
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();

            data.Close();
            reader.Close();

            string key = getKey(s);
            label2.Text = "Key: " + key;

            if (key.ToUpper().Contains("<ERROR>"))
            {
                MessageBox.Show("An error occurred logging in.  Check password and/or internet connection");
                return;
            }

            System.Collections.ArrayList addresses = new System.Collections.ArrayList();
            foreach (string t in listBox1.Items)
            {

                if (t.Trim().Length == 0)
                {
                    continue;
                }
                string a = getAddress(t, key);
                if (a.Contains("<ERROR>"))
                {
                    listBox2.Items.Add(t);
                }
                else
                {
                    addresses.Add(a);
                    listBox3.Items.Add(t);
                }

            }
            listBox1.Items.Clear();

            QRZPrinter.PrinterObejctSmall p = new PrinterObejctSmall();
            foreach (string amateur in addresses)
            {
                //System.Windows.Forms.MessageBox.Show(amateur);
                if (amateur.Length > 0)
                {
                    p.text = amateur;
                    p.photopath = logopath.Text;
                    p.PrinterName = sPrinter.Text;
                    p.Printing();
                }
            }

        }
    }
}
