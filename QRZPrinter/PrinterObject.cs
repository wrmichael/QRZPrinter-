﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Security.Policy;

namespace QRZPrinter
{

    public class PrinterObejct
    {

        public QSOData qso;

        public string qsoline1;
        public string qsoline2;
        public string qsoline3;
        public string qsoline4;


        private Font printFont;
        //private StreamReader streamToPrint;
        //static string filePath;
        public string text = "";
     public string photopath  = "";
        public string PrinterName = "";
       

        // The PrintPage event is raised for each page to be printed.
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;

            Bitmap bm = new Bitmap(60, 60);
            Image logoImage = Image.FromHbitmap(bm.GetHbitmap());

            if (System.IO.File.Exists(photopath))
            {
                logoImage = Image.FromFile(photopath);
            }


            //String line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);
            //logo image if it exists..  // if not maybe we do something else 

            if (System.IO.File.Exists(photopath))
            {

                ev.Graphics.DrawImage(logoImage, new Point(0, 0));
            }

            // Iterate over the file, printing each line.
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" },
    StringSplitOptions.None);
            for (int idx = 0; idx < lines.Length; idx++)
            {

                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
               
                ev.Graphics.DrawString(lines[idx], printFont, Brushes.Black,
                   75, 0 + (count * 15), new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            //if (line != null)
            //    ev.HasMorePages = true;
            //else
            //    ev.HasMorePages = false;
        }



        private void qso_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;


            printFont.GetHeight(ev.Graphics);

            ev.Graphics.DrawString("CALL SIGN: " + qso.callsign, printFont, Brushes.Black, 0, 0, new StringFormat());
            ev.Graphics.DrawString("QSO DATE: " + qso.date, printFont, Brushes.Black, 0, 25, new StringFormat());
            ev.Graphics.DrawString("UTC: " + qso.time, printFont, Brushes.Black, 200, 25, new StringFormat());

            ev.Graphics.DrawString("RST SENT: " + qso.sent, printFont, Brushes.Black, 0, 50, new StringFormat());
            ev.Graphics.DrawString("RST RCVD: " + qso.rcvd, printFont, Brushes.Black, 200, 50, new StringFormat());
            
            ev.Graphics.DrawString("FREQUECY: " + qso.freq, printFont, Brushes.Black, 0, 75, new StringFormat());
            ev.Graphics.DrawString("MODE: " + qso.mode, printFont, Brushes.Black, 200, 75, new StringFormat());
        }


        public void PrintingQSOTemplate(object sender, PrintPageEventArgs ev)
        {
            

            ev.Graphics.DrawString(qsoline1, printFont, Brushes.Black, 0,0, new StringFormat());
            
        }


        public void PrintQSOTemplate()
        {
            try
            {
                //streamToPrint = new StreamReader(filePath);
                try
                {
                    printFont = new Font("Arial", 10);
                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = this.PrinterName;
                    pd.DefaultPageSettings.Landscape = true;

                    pd.PrintPage += new PrintPageEventHandler(PrintingQSOTemplate);
                    // Print the document.
                    pd.Print();
                }
                finally
                {
                    //streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        public void PrintQSO()
        {
            try
            {
                //streamToPrint = new StreamReader(filePath);
                try
                {
                    printFont = new Font("Arial", 10);
                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = this.PrinterName;
                    pd.DefaultPageSettings.Landscape = true;

                    pd.PrintPage += new PrintPageEventHandler(qso_PrintPage);
                    // Print the document.
                    pd.Print();
                }
                finally
                {
                    //streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        // Print the file.
        public void Printing()
        {
            try
            {
                //streamToPrint = new StreamReader(filePath);
                try
                {
                    printFont = new Font("Arial", 10);
                    PrintDocument pd = new PrintDocument( );
                    pd.PrinterSettings.PrinterName = this.PrinterName;
                    pd.DefaultPageSettings.Landscape = true;
                    
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    // Print the document.
                    pd.Print();
                }
                finally
                {
                    //streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // This is the main entry point for the application.
        //public static void Main(string[] args)
        //{
        //    string sampleName = Environment.GetCommandLineArgs()[0];
        //    if (args.Length != 1)
        //    {
        //        Console.WriteLine("Usage: " + sampleName + " <file path>");
        //        return;
        //    }
        //    filePath = args[0];
        //    new PrintingExample();
        //}
    }
}
