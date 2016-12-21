using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

using System.Threading;

namespace PCC
{
    public class DBtest
    {
        public int m = 0;
        public SqlConnection cooect = null;
        public void  testdata()
        {
               m++;
               DateTime nn = DateTime.Now;
               string sql1 = "delete from sim_rgvinfo_current ";
               string sql2 = "insert into sim_rgvinfo_current ([SourceLocationCode],[DestLocationCode] ,[CurrentLocation] ,[Status] ,[Error] ,[LoaderStatus] ,[ForkStatus] ,[CurrentTime],[RGVCode])select top 1 [SourceLocationCode] ,[DestLocationCode] ,[CurrentLocation] ,[Status] ,[Error] ,[LoaderStatus] ,[ForkStatus],[CurrentTime],[RGVCode] from sim_rgvinfo_his  WHERE (rid NOT IN (SELECT TOP " + (m - 1).ToString() + " rid FROM sim_rgvinfo_his order by CurrentTime asc )) order by CurrentTime asc ";

               if (cooect == null)
               {
                   cooect = new SqlConnection(ConfigurationManager.AppSettings["conn"].ToString());
                   cooect.Open();
               }
               else if (cooect.State == ConnectionState.Closed)
               {
                   cooect.Open();
               }
               SqlCommand cmd1 = new SqlCommand(sql1.ToString(), cooect);
               cmd1.ExecuteNonQuery();
                SqlCommand cmd2 = new SqlCommand(sql2.ToString(), cooect);
               cmd2.ExecuteNonQuery( );
               DateTime nn2 = DateTime.Now;
               TimeSpan ts = nn2 - nn;		
               Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!************!!!!!!!!!!"+ts.ToString());

         }


        public DataTable dt = null;
        public  DBtest()
        {
            string sql = "select * from sim_rgvinfo_his order by CurrentTime asc ";
            if (cooect == null)
            {
                cooect = new SqlConnection(ConfigurationManager.AppSettings["conn"].ToString());
                cooect.Open();
            }
            else if (cooect.State == ConnectionState.Closed)
            {
                cooect.Open();
            }
            SqlCommand cmd = new SqlCommand(sql.ToString(), cooect);
            DataSet ds = new DataSet();
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(ds);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                dt = ds.Tables[0];
            }
        }


        public void testdata2()
        {
            m++;
            DateTime nn = DateTime.Now;

            string sql = "UPDATE [sim_rgvinfo_current] SET [SourceLocationCode] =" + dt.Rows[m][0] + " ,[DestLocationCode] =" + dt.Rows[m][1] + " ,[CurrentLocation] = " + dt.Rows[m][2] + " ,[Status] = " + dt.Rows[m][3] + " ,[Error] = " + dt.Rows[m][4] + " ,[LoaderStatus] =" + dt.Rows[m][5] + " ,[ForkStatus] =" + dt.Rows[m][6] + ",[CurrentTime] ='" + dt.Rows[m][7] + "' ,[RGVCode] = " + dt.Rows[m][9] + "";

            if (cooect == null)
            {
                cooect = new SqlConnection(ConfigurationManager.AppSettings["conn"].ToString());
                cooect.Open();
            }
            else if (cooect.State == ConnectionState.Closed)
            {
                cooect.Open();
            }
            SqlCommand cmd1 = new SqlCommand(sql.ToString(), cooect);
            cmd1.ExecuteNonQuery();
            DateTime nn2 = DateTime.Now;
            TimeSpan ts = nn2 - nn;
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!************!!!!!!!!!!" + ts.ToString());

        }
           
    }
}
