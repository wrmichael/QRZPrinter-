using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.X;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using MySql.Data.Types;
using MySql.Data.EntityFramework.Properties;
using MySql.Data.MySqlClient.Authentication;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace QRZPrinter
{
    class HRDInterface
    {
        
        public static   string sql_getbycall = "SELECT COL_TX_PWR, COL_NAME, COL_QTH, COL_CALL,COL_FREQ, COL_BAND, COL_MODE, COL_TIME_ON,COL_RST_SENT, COL_RST_RCVD, COL_QSL_RCVD, COL_QSL_SENT, COL_COMMENT FROM HRD.TABLE_HRD_CONTACTS_V01";
        public static string connstring = @"server=%S%;userid=%U%;password=%P%;database=HRD";
        public static MySqlConnection conn;
        public static string u;
        public static string p;
        public static string s;

        //filters
        public static string callsign; 

        public static System.Collections.ArrayList test()
        {
            System.Collections.ArrayList qsos = new System.Collections.ArrayList(); 
                       
            try
            {
                connstring = connstring.Replace("%U%", u);
                connstring = connstring.Replace("%P%", p);
                connstring = connstring.Replace("%S%", s);

                conn = new MySqlConnection(connstring);
                
                //conn.ConnectionTimeout = 360;
                conn.Open();

                string query = sql_getbycall;

                if (callsign.Trim().Length > 0)
                {
                    query = query + " where COL_CALL = '" + callsign + "'";
                }
                else
                {
                    //query = query + " LIMIT 5000";
                }
                MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                DataSet ds = new DataSet();
                da.Fill(ds, "HRD");
                DataTable dt = ds.Tables["HRD"];

                foreach (DataRow row in dt.Rows)
                {
                    QSOData q = new QSOData();
                    
                    q.callsign = row["COL_CALL"].ToString();
                    q.freq = row["COL_FREQ"].ToString();
                    q.mode = row["COL_MODE"].ToString();
                    q.rcvd = row["COL_RST_RCVD"].ToString();
                    q.sent = row["COL_RST_SENT"].ToString();
                    q.comment = row["COL_COMMENT"].ToString();
                    q.date = row["COL_TIME_ON"].ToString();
                    q.QSL_SENT = row["COL_QSL_SENT"].ToString();
                    q.name = row["COL_NAME"].ToString();
                    q.qth = row["COL_QTH"].ToString();
                    q.power = row["COL_TX_PWR"].ToString();
                    q.band = row["COL_BAND"].ToString();


                    //foreach (DataColumn col in dt.Columns)
                    //{
                    //    Console.Write(row[col] + "\t");
                    //}

                    qsos.Add(q);
                    //Console.Write("\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return qsos;
        }

    }
}
