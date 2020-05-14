﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace QRZPrinter
{
    class GlobalClass
    {

        public string key;
        public string username;
        public string password;
        public System.Collections.ArrayList callsigns;
        public System.Collections.ArrayList addresses;
        public System.Collections.ArrayList qsos; 

        //public string[] callsigns; 
        public string PrinterName;
        public string LogoPath;


        public void printQSO()
        {
            QRZPrinter.PrinterObejct p = new PrinterObejct();

            foreach (QSOData qd in qsos)
            {

                p.qso = qd;
                //p.photopath = LogoPath;
                p.PrinterName = PrinterName;
                p.PrintQSO();

            }
        
        }

        public void printLabelsFromQRZ()
        {

            WebClient client = new WebClient();

            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead("http://xmldata.qrz.com/xml/current/?username=" + this.username + ";password=" + this.password + ";agent=q5.0");
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();

            data.Close();
            reader.Close();

            key = getKey(s);
            
            if (key.ToUpper().Contains("<ERROR>"))
            {
                MessageBox.Show("An error occurred logging in.  Check password and/or internet connection");
                return;
            }

            addresses = new System.Collections.ArrayList();
            foreach (string t in this.callsigns)
            {

                if (t.Trim().Length == 0)
                {
                    continue;
                }
                string a = getAddress(t, key);
                if (a.Contains("<ERROR>"))
                {
                    //callsigns.Items.Add(t);
                }
                else
                {
                    addresses.Add(a);
                    //listBox3.Items.Add(t);
                }

            }

            QRZPrinter.PrinterObejct p = new PrinterObejct();
            foreach (string amateur in addresses)
            {
                //System.Windows.Forms.MessageBox.Show(amateur);
                if (amateur.Length > 0)
                {
                    p.text = amateur;
                    p.photopath = LogoPath;
                    p.PrinterName = PrinterName;
                    p.Printing();
                }
            }

        }

        public string getAddress(string t, string k)
        {
            string newt = "";
            string url = "http://xmldata.qrz.com/xml/current/?s=" + k + ";callsign=" + t;

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

            newt = t.ToUpper() + "\r\n" + getfName(s) + " " + getName(s) + "\r\n";
            newt = newt + getAmateurAddress(s) + "\r\n";
            newt = newt + getAmateurCity(s) + " " + getAmateurState(s) + " " + getAmateurZip(s) + "\r\n";
            newt = newt + getAmateurCountry(s);

            return newt;

        }

        public string getfName(string s)
        {
            string news = "";

            news = s.Substring(s.ToUpper().IndexOf("<FNAME>") + 7);
            news = news.ToUpper().Split(new string[] { "</FNAME>" }, StringSplitOptions.RemoveEmptyEntries)[0];
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
            news = news.Split(new String[] { "</Key>" }, StringSplitOptions.RemoveEmptyEntries)[0];

            return news;

        }


    }
}
