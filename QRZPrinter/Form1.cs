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


                WebClient client = new WebClient();

                // Add a user agent header in case the
                // requested URI contains a query.

                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                Stream data = client.OpenRead("http://xmldata.qrz.com/xml/current/?username=" + textBox1.Text + ";password=" + textBox2.Text + ";agent=q5.0");
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                //Console.WriteLine(s);
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
                    p.Printing();
                }
            }
            


    }
        public string getAddress(string t, string k)
        {
            string newt = "";
            string url = "http://xmldata.qrz.com/xml/current/?s=" + k + ";callsign=" +t ;
            //http://xmldata.qrz.com/xml/current/?s=f894c4bd29f3923f3bacf02c532d7bd9;callsign=aa7bq

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

            newt = t.ToUpper() + "\r\n" + getfName(s) + " " + getName(s) + "\r\n" ;
            newt = newt + getAmateurAddress(s) + "\r\n";
            newt = newt + getAmateurCity(s) + " " + getAmateurState(s) + " " + getAmateurZip(s) + "\r\n";
            newt = newt + getAmateurCountry(s);
            
            return newt;
               
        }

        public string getfName(string s)
        {
            string news = "";

            news = s.Substring(s.ToUpper().IndexOf("<FNAME>")+7);
            news = news.ToUpper().Split(new string[] { "</FNAME>"},StringSplitOptions.RemoveEmptyEntries)[0];
     //       < fname > Raymond G </ fname >
   //< name > Grob, Jr </ name >

            return news;
        }


        public string getAmateurAddress(string s)
        {
            string news = "";

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
    }
}
