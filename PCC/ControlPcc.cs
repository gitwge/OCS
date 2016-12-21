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
    public class ControlPcc
    {
        public string xmlPath = Environment.CurrentDirectory + ConfigurationManager.AppSettings["xmlfile"].ToString();
        public int debugMode = int.Parse( ConfigurationManager.AppSettings["debugMode"].ToString());
        //--------------------------------------
        #region 小车位置调试信息
        public float pos_1 = 0;
        public float pos_58 = 0;
        public int point_1 = 0;
        public int point_58 = 0;
        public int totalNum = 0;

        //获取小车的调试变量
        public void getCarDebugData()
        {
            string filepath = xmlPath;
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            //XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            //XmlNode xn = rootElem.SelectSingleNode("mechan[0]").SelectSingleNode("config");
            XmlNode xn = doc.SelectSingleNode("DATA/mechan[@name='PCC']/config");
            foreach (XmlNode xx in xn.ChildNodes)
            {
                if (xx.Attributes["name"].Value.ToString() == "pos_1")
                {
                    pos_1 = float.Parse(xx.Attributes["value"].Value.ToString());
                }
                else if (xx.Attributes["name"].Value.ToString() == "pos_58")
                {
                    pos_58 = float.Parse(xx.Attributes["value"].Value.ToString());
                }
                else if (xx.Attributes["name"].Value.ToString() == "point_1")
                {
                    point_1 = int.Parse(xx.Attributes["value"].Value.ToString());
                }
                else if (xx.Attributes["name"].Value.ToString() == "point_58")
                {
                    point_58 = int.Parse(xx.Attributes["value"].Value.ToString());
                }
                else if (xx.Attributes["name"].Value.ToString() == "totalNum")
                {
                    totalNum = int.Parse(xx.Attributes["value"].Value.ToString());
                }
            }
        }

        //点位基础数据计算
        public float getPointPos(int point)
        {
            float bc = (pos_58 - pos_1) / (point_58 - point_1);
            float bc_start = 1.1f - point_1 * bc;
            float result = bc_start + point * bc;
            return result;
        }

        //编号基础计算
        
        public float getNumPos(int num)
        {
            float bc = (pos_58 - pos_1) / (totalNum - 1);
            float result = (num - 1) * bc + pos_1;
            return result;
        }
        #endregion

        //---------------------------------------------------
        #region 基本数据处理
        //type 1 uint,2 float,3 flag
        //设置数据
        public void updateValue(string name, string value, int type, int handle)
        {
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            int index = GetIdex.getDicOutputIndex(name);
            if (type == 1)
            {
                ComTCPLib.SetOutputAsUINT(handle, index, uint.Parse(value));
            }
            else if (type == 2)
            {
                ComTCPLib.SetOutputAsREAL32(handle, index, float.Parse(value));
            }
        }

        public object readValue(string name,  int type, int handle)
        {
            double time, timeStep;
            ComTCPLib.UpdateData(handle, out time, out timeStep);
            int index = GetIdex.getDicInputIndex(name);
            if (type == 1)
            {
                int result;
                ComTCPLib.GetInputAsINT(handle, index, out result);
                return result;
            }
            else if (type == 2)
            {
                float result;
                ComTCPLib.GetInputAsREAL32(handle, index, out result);
                return result;
            }
            else if(type == 3)
            {
                bool result;
                ComTCPLib.GetInputAsBOOL(handle, index, out result);
                return result;
            }
            return null ;
        }

       
        //设置基础数据 by table
        public int updateByTable(int handel,DataTable dt)
        {
            try
            {
                if (dt == null || dt.Rows.Count < 1)
                    return -1;
                foreach (DataRow dr in dt.Rows)
                {
                    updateValue(dr["name"].ToString(), dr["value"].ToString(), int.Parse(dr["datatype"].ToString()), handel);
                }
                return 1;
            }
            catch (Exception ex)
            {
                return -2;
            }

        }

        //读取设置数据
        public DataTable getpartsDataByNmae(string name, string rORw)
        {
            DataTable dt = new DataTable();
            DataColumn dc = null;
            dc = dt.Columns.Add("name");
            dc = dt.Columns.Add("value");
            dc = dt.Columns.Add("datatype");
            dc = dt.Columns.Add("key");


            string filepath = xmlPath;
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            //XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            //XmlNode xn = rootElem.SelectSingleNode("mechan[@name='car']");
            //XmlNode xn = rootElem.SelectSingleNode("mechan[0]");

            XmlNode xn = doc.SelectSingleNode("DATA/mechan[@name='PCC']");
            foreach (XmlNode xx in xn.ChildNodes)
            {
                if (xx.Attributes["name"].Value.ToString() == name)
                {
                    foreach (XmlNode child in xx.ChildNodes)
                    {
                        if (child.Attributes["rORw"].Value.ToString() == rORw)
                        {
                            DataRow newRow;
                            newRow = dt.NewRow();
                            newRow["name"] = child.Attributes["name"].Value.ToString();
                            newRow["value"] = child.Attributes["value"].Value.ToString();
                            newRow["datatype"] = child.Attributes["datatype"].Value.ToString();
                            newRow["key"] = child.Attributes["key"].Value.ToString();
                            dt.Rows.Add(newRow);
                        }
                        
                    }
                    break;
                }

            }
            return dt;
        }
        #endregion

        //---------------------------------------------------
        #region 读取数据库数据
        public SqlConnection cooect = null;
       
        public int m = 0;
        public DataRow getFirstNewData()
        {
            m++;
            string sql = "";
            if( debugMode == 1 )
            {
                sql = "select top 1 * from " + ConfigurationManager.AppSettings["pcc_table"].ToString() + " order by CurrentTime  desc";
            }
            else
            {
                sql = " SELECT TOP 1 *  FROM " + ConfigurationManager.AppSettings["pcc_table"].ToString() + "  WHERE (rid NOT IN (SELECT TOP " + (m - 1).ToString() + " rid FROM " + ConfigurationManager.AppSettings["pcc_table"].ToString() + " order by CurrentTime asc )) order by CurrentTime asc ";
            }
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
                return ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1];
            }
            return null;
        }

        public void disConnect()
        {
            if (cooect == null)
            {
                return;
            }
            else if (cooect.State == ConnectionState.Open)
            {
                cooect.Close();
            }
        }

        #endregion

        //------------------------------------
        #region 基本方法处理
        //比对datarow数据一不一样
        public DataRow lastDr;
        public bool comparedatarow(DataRow drnow)
        {
            if( drnow == null)
            {
                return true;
            }
            if (lastDr == null )
            {
                lastDr = drnow;
                return false;
            }
            if ((int.Parse(lastDr[2].ToString()) - int.Parse(drnow[2].ToString()) > 5 || int.Parse(lastDr[2].ToString()) - int.Parse(drnow[2].ToString()) < -5) && (lastDr["CurrentTime"].ToString() != drnow["CurrentTime"].ToString()))
            {
                lastDr = drnow;
                return false;
            }
            else
            {
                //lastDr = drnow;
                return true;
            }
        }
        //type = 1 速度归位，加减速度还是最大
        public DataTable setMaxTable(DataTable dt, int type)
        { 
            foreach( DataRow dr in dt.Rows )
            {
                //if (dr["key"].ToString() == "spd" && type == 1)
                 if ( type == 1)
                {
                    dr["value"] = carSpd.ToString();
                }
                else
                {
                    dr["value"] = "1000";
                }
                
            }
            return dt;
        }

        //当前是否到达
        public bool carReched( DataTable dt ,int handel)
        {
            object flag=null;
            foreach(DataRow dr in dt.Rows)
            {
                if (dr["key"].ToString() == "nowrchd")
                {
                    flag = readValue(dr["name"].ToString(), int.Parse(dr["datatype"].ToString()), handel);
                    break;
                }
            }
            return bool.Parse(flag.ToString());
        }

        //当前速度
        public float carnowspd(DataTable dt, int handel)
        {
            object flag = null;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["key"].ToString() == "nowspd")
                {
                    flag = readValue(dr["name"].ToString(), int.Parse(dr["datatype"].ToString()), handel);
                    break;
                }
            }
            return float.Parse(flag.ToString());
        }
        //当前位置
        public float carnowpos(DataTable dt, int handel)
        {
            object flag = null;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["key"].ToString() == "nowaxis")
                {
                    flag = readValue(dr["name"].ToString(), int.Parse(dr["datatype"].ToString()), handel);
                    break;
                }
            }
            return float.Parse(flag.ToString());
        }



        #endregion

        //------------------------------------
        #region   读取xml的数据配置
        //获取要设置的及时数据的类型
        public DataTable TimelyTableType;
        public void getTimelydataType()
        {
            DataTable dt = new DataTable();
            DataColumn dc = null;
            dc = dt.Columns.Add("name");
            dc = dt.Columns.Add("datatype");
            dc = dt.Columns.Add("sort");
            dc = dt.Columns.Add("value");
            dc = dt.Columns.Add("key");
            string filepath = xmlPath;
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            XmlNode xn = doc.SelectSingleNode("DATA/mechan[@name='PCC']/data");
            foreach (XmlNode xnn in xn.ChildNodes)
            {
                DataRow dr = dt.NewRow();
                dr[0] = xnn.Attributes["name"].Value.ToString();
                dr[1] = xnn.Attributes["datatype"].Value.ToString();
                dr[2] = xnn.Attributes["sort"].Value.ToString();
                dr[3] = "";
                dr[4] = xnn.Attributes["key"].Value.ToString();
                dt.Rows.Add(dr);
                   
            }

            TimelyTableType = dt;
        }


        #endregion



        //---------------------------------------------------
        #region 数据处理逻辑
        public DataTable TimelyTable = null;
        public DataTable getTimelytable()
        {
            DataRow nowrow = null;
            nowrow = getFirstNewData();
            if (comparedatarow(nowrow))
            {
                return null;
            }
            //获取本地变量
            if(pos_1 == 0 || pos_58 == 0 || point_1 == 0 || point_58 == 0 || totalNum == 0)
            {
                getCarDebugData();
            }
            //获取设置表的结构
            if (TimelyTableType == null)
            {
                 getTimelydataType();
            }
            DataTable now = TimelyTableType.Copy();
            //数据库取得的数据复制给配置表
            for (int i = 0; i < nowrow.ItemArray.Length; i++)
            {
                foreach (DataRow dr in now.Rows)
                {
                    if (dr["sort"].ToString() == i.ToString())
                    {
                        dr["value"] = nowrow[i].ToString();
                        break;
                    }
                }
            }
            //设置数据对应的比例
            foreach (DataRow dr in now.Rows)
            {
                //小车目标
                if (dr["key"].ToString() == "car_tgt")
                {
                    dr["value"] = getNumPos(int.Parse(dr["value"].ToString()));
                }
                //小车当前
                else if (dr["key"].ToString() == "car_current")
                {
                    dr["value"] = getPointPos(int.Parse(dr["value"].ToString()));
                }
            }

            //设置
            return now;

        }

        public DataTable CarSpdTable = null;
        public DataTable CarReadTable = null;
        public DataTable CarMaxTable = null;
        public void getCarSpdTable()
        {
            CarSpdTable = getpartsDataByNmae("car", "2");
        }
        public void getCarReadTable()
        {
            CarReadTable = getpartsDataByNmae("car", "1");
        }
        #endregion


        public DataTable lastData = null;
        public DataTable nowData = null;
        public float carSpd = 0;
        public void setModelData(int handel)
        {
            TimelyTable = getTimelytable();
            if (TimelyTable == null)
            {
                return;
            }
            if (CarSpdTable == null)
            {
                getCarSpdTable();
            }
            if(CarReadTable == null)
            {
                getCarReadTable();
            }
            if(CarMaxTable == null)
            {
                CarMaxTable = setMaxTable(CarSpdTable.Copy(),0);
            }
            nowData = TimelyTable.Copy();
            
            float spd_now = System.Math.Abs(carnowspd(CarReadTable, handel));
            if(carSpd == 0)
            {
                foreach (DataRow dr in CarSpdTable.Rows)
                {
                    if (dr["key"].ToString() == "spd")
                    {
                        carSpd = float.Parse(dr["value"].ToString());
                        break;
                    }
                }
            }
           
            //前一条记录空
            if (lastData == null || spd_now > carSpd)
            {
                //快速过去
                lastData = nowData.Copy();
                lastData.Rows[0]["value"] = lastData.Rows[1]["value"];
                updateByTable(1,CarMaxTable);
                updateByTable(1, lastData);
                Thread.Sleep(100);
                
                //正常行驶
                //updateByTable(handel, CarSpdTable);
                //updateByTable(handel, nowData);
                return;
            }

            
            //比较当前速度
            //速度起来了
           
            if ( spd_now == 0)
            {
                updateByTable(handel, CarSpdTable);
            }
            if (spd_now == carSpd)
            {
                //比较位置
                float now_pos2 = 0;
                float now_pos1 = 0;
                foreach (DataRow dr in nowData.Rows)
                {
                    if (dr["key"].ToString() == "car_current")
                    {
                        now_pos1 = float.Parse(dr["value"].ToString());
                        break;
                    }
                }
                now_pos2 = carnowpos(CarReadTable, handel);
                if (System.Math.Abs(now_pos1 - now_pos2) > 1)
                {
                    lastData = nowData.Copy();
                    //快速过去
                    lastData.Rows[0]["value"] = lastData.Rows[1]["value"];
                    updateByTable(1, CarMaxTable);
                    updateByTable(1, lastData);
                    Thread.Sleep(100);
                    //正常行驶
                    
                    updateByTable(handel, setMaxTable(CarSpdTable.Copy(),1));
                    updateByTable(handel, nowData);
                    //Thread.Sleep(100);
                    //updateByTable(handel, CarSpdTable);
                    //updateByTable(handel, nowData);
                    //while(1==1)
                    //{
                    //    if (System.Math.Abs(carnowspd(CarReadTable, handel)) == carSpd)
                    //    {
                    //        updateByTable(handel, CarSpdTable);
                    //        break;
                    //    }
                    //}
                    return;
                }
            }
            updateByTable(handel, CarSpdTable);
            updateByTable(handel, nowData);
        }

        public void setBaseData(int handel)
        {
            try
            {
                //设置基础速度

                getCarSpdTable();
                updateByTable(handel, CarSpdTable);

                //设置基础变量
                getCarDebugData();

                this.m = 0;

                //设置上一条数据
                this.lastDr = null;
                this.lastData = null;

                getPlattable();
                updateByTable(handel, plattable);

                getForktable();
                updateByTable(handel, forktable);
            }catch(Exception ex)
            {
                return;
            }

        }

        public void quicklyGo(DataTable gotoTable, int handel)
        {
            try
            { 
                Console.WriteLine("---006-----");
                Console.WriteLine("---006-----" + gotoTable.Rows[0]["value"].ToString() + "***" + gotoTable.Rows[1]["value"].ToString() + "***");
                DataTable dt = gotoTable.Copy();
                dt.Rows[0]["value"] = dt.Rows[1]["value"];
                //快速过去
                updateByTable(1, dt);
                updateByTable(1, CarMaxTable);
                updateByTable(1, dt);
                Thread.Sleep(100);
                //到了没有
                int i = 0;
                while(i < 100)
                {
                    i++;
                    float nowspd = carnowspd(CarReadTable, handel);
                    float nowpos = carnowpos(CarReadTable, handel);
                    float nowpos2 = float.Parse(dt.Rows[1]["value"].ToString());
                    Console.WriteLine("---006-----" + i.ToString() + "*****" + nowspd.ToString() + "****" + nowpos.ToString() +"************"+ nowpos2.ToString());
                    if (nowpos== nowpos2)
                    {
                        updateByTable(handel, setMaxTable(CarSpdTable.Copy(), 1));
                        updateByTable(handel, gotoTable);
                        Console.WriteLine("---006-----"+"RCHD");
                        break;
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(100);
                //判断速度
                i = 0;
                while (i < 100)
                {
                    i++;
                    Console.WriteLine("---006-----2--" + i.ToString());
                    float nowspd = System.Math.Abs(carnowspd(CarReadTable, handel));
                    if (nowspd >= carSpd)
                    {
                        updateByTable(handel, CarSpdTable);
                        updateByTable(handel, gotoTable);
                        Console.WriteLine("---006----2----" + "RCHD");
                        break;
                    }
                    else if (nowspd == 0.0)
                    {
                        Console.WriteLine("---006----3----" + "RCHD");
                        break;
                    }
                    Thread.Sleep(10);
                }

            }
            catch (Exception ex)
            {
                return;
            }
        }

        public void setModelData2(int handel)
        {
            TimelyTable = getTimelytable();
            
            if (TimelyTable == null)
            {
                return;
            }
            if (CarSpdTable == null)
            {
                getCarSpdTable();
            }
            if (CarReadTable == null)
            {
                getCarReadTable();
            }
            if (CarMaxTable == null)
            {
                CarMaxTable = setMaxTable(CarSpdTable.Copy(), 0);
            }
            nowData = TimelyTable.Copy();

            float spd_now = System.Math.Abs(carnowspd(CarReadTable, handel));
            if (carSpd == 0)
            {
                foreach (DataRow dr in CarSpdTable.Rows)
                {
                    if (dr["key"].ToString() == "spd")
                    {
                        carSpd = float.Parse(dr["value"].ToString());
                        break;
                    }
                }
            }

            //前一条记录空
            if (lastData == null || spd_now > carSpd)
            {
                lastData = nowData.Copy();
                lastData.Rows[0]["value"] = lastData.Rows[1]["value"];
                //快速过去
                quicklyGo(lastData.Copy(), handel);
                return;
            }


            //比较当前速度
            //速度起来了

            if (spd_now == 0)
            {
                updateByTable(handel, CarSpdTable);
            }
            if (spd_now == carSpd)
            {
                //比较位置
                float now_pos2 = 0;
                float now_pos1 = 0;
                foreach (DataRow dr in nowData.Rows)
                {
                    if (dr["key"].ToString() == "car_current")
                    {
                        now_pos1 = float.Parse(dr["value"].ToString());
                        break;
                    }
                }
                now_pos2 = carnowpos(CarReadTable, handel);
                if (System.Math.Abs(now_pos1 - now_pos2) > 1)
                {
                    quicklyGo(nowData.Copy(), handel);
                    return;
                }
            }
            updateByTable(handel, CarSpdTable);
            updateByTable(handel, nowData);

        }

        public bool flag = true;
        public float BC = 0.0f;
        public void setModelData3(int handel)
        {
            
            TimelyTable = getTimelytable();
           
            if (TimelyTable == null)
            {
                return;
            }
            Console.WriteLine("");
            Console.WriteLine("++++++++++++++++"+m.ToString());
            Console.WriteLine("---001-----" + TimelyTable.Rows[0]["value"].ToString() + "***" + TimelyTable.Rows[1]["value"].ToString() + "***");
            if (CarSpdTable == null)
            {
                getCarSpdTable();
            }
            if (CarReadTable == null)
            {
                getCarReadTable();
            }
            if (CarMaxTable == null)
            {
                CarMaxTable = setMaxTable(CarSpdTable.Copy(), 0);
            }
            if(BC == 0.0f)
            {
                 BC = float.Parse(System.Configuration.ConfigurationManager.AppSettings["BC"].ToString());
            }
            nowData = TimelyTable.Copy();

            float spd_now = System.Math.Abs(carnowspd(CarReadTable, handel));
            if (carSpd == 0)
            {
                foreach (DataRow dr in CarSpdTable.Rows)
                {
                    if (dr["key"].ToString() == "spd")
                    {
                        carSpd = float.Parse(dr["value"].ToString());
                        break;
                    }
                }
            }

            //前一条记录空
            if (lastData == null || spd_now > carSpd)
            {
               
                lastData = nowData.Copy();
                lastData.Rows[0]["value"] = lastData.Rows[1]["value"];
                //快速过去
                quicklyGo(lastData.Copy(), handel);
                Console.WriteLine("------002------" + spd_now.ToString() + "****" + carSpd.ToString() + "***" + lastData.Rows[1]["value"].ToString());
                return;
            }

            //if (System.Math.Abs(spd_now) < 0.001)
            //{
            //    lastData = nowData.Copy();
            //    updateByTable(handel, CarSpdTable);
            //    updateByTable(handel, nowData);
            //    return;
            //}
            
            if ( float.Parse( lastData.Rows[0]["value"].ToString()) == float.Parse( nowData.Rows[0]["value"].ToString()))
            {
                Console.WriteLine("------003------");
                float now_pos2 = 0;
                float now_pos1 = 0;
                foreach (DataRow dr in nowData.Rows)
                {
                    if (dr["key"].ToString() == "car_current")
                    {
                        now_pos1 = float.Parse(dr["value"].ToString());
                        break;
                    }
                }
                now_pos2 = carnowpos(CarReadTable, handel);
                Console.WriteLine("------003------" + now_pos1.ToString() + "***" + now_pos2.ToString() + "*****" + spd_now.ToString());
                if ((carSpd == spd_now || spd_now == 0) && (System.Math.Abs(now_pos1 - now_pos2) > BC))
                {
                    quicklyGo(nowData.Copy(), handel);
                    Console.WriteLine("------003------&&&&&&&" + nowData.Rows[0]["value"].ToString() + "***" + nowData.Rows[1]["value"].ToString() + "***");
                }
               
            }
            else
            {
                updateByTable(handel, CarSpdTable);
                updateByTable(handel, nowData);
                Console.WriteLine("------004------" + nowData.Rows[0]["value"].ToString() + "***" + nowData.Rows[1]["value"].ToString() + "***");
            }
            lastData = nowData.Copy();
        }



        //---------------------------------------------------
        #region  载货台的集成
        //左：1，中：2，右：3，上：10，下：20
        public DataTable plattable = null;

        public void getPlattable()
        {
            plattable = getpartsDataByNmae("platform", "2");
            
        }
        public int getPlatState(int state)
        { 
           if(state /10 == 1)
           {
               return 1;
           }
           else if (state / 10 == 2)
           {
               return 2;
           }
           else
           {
               return 0;
           }
        }
        #endregion

        //---------------------------------------------------
        #region 货叉的逻辑
        public DataTable forktable = null;
        public void getForktable()
        {
            forktable = getpartsDataByNmae("fork", "2");

        }

        public int getForkState(int state)
        {
            if (state % 10 == 1)
            {
                //左
                return 1;
            }
            else if (state % 10 == 2)
            {
                //中
                return 3;
            }
            else if (state % 10 == 3)
            {
                //右
                return 2;
            }
            else 
            {
                return 3;
            }
        }
        #endregion

    }
}
