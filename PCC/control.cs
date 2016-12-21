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
    public  class control
    {

//数据库字段表：
//1：小车原位置（货架号）
//2：小车目标位置（货架号）
//3：小车当前位置（绝对值）
//4：小车状态（0停止 1运动）
//5：故障状态 （0无故障 1有故障）
//6：载货台状态（空 货号字符串）
//7：货叉状态 （0原  1左侧出 2 右侧出 3 左侧收 4 右侧收）
//8：当前时间

        public string xmlPath = Environment.CurrentDirectory +  ConfigurationManager.AppSettings["xmlfile"].ToString();
        //---------------------------------------------------
        #region  基础数据的设置
        //type 1 uint,2 float
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


         

        //读取基础数据
        public DataTable getXmlData()
        {
            DataTable dt = new DataTable();
            DataColumn dc = null;
            dc = dt.Columns.Add("name");
            dc = dt.Columns.Add("value");
            dc = dt.Columns.Add("datatype");


            string filepath = xmlPath;
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            XmlNode xn = rootElem.FirstChild.FirstChild;
            foreach (XmlNode xx in xn.ChildNodes)
            {
                foreach (XmlNode child in xx.ChildNodes)
                {
                    DataRow newRow;
                    newRow = dt.NewRow();
                    newRow["name"] = child.Attributes["name"].Value.ToString();
                    newRow["value"] = child.Attributes["value"].Value.ToString();
                    newRow["datatype"] = child.Attributes["datatype"].Value.ToString();
                    dt.Rows.Add(newRow);
                }
            }
            return dt;

        }
        //设置基础数据
        public int updateBasicData(int handel)
        {
            try
            {
                DataTable dt = getXmlData();
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
        #endregion

        //---------------------------------------------------
        #region 读取最新一条数据
        public SqlConnection cooect = null;
        public DataRow getFirstNewData()
        {
            //string sql = "select top 1 * from " + ConfigurationManager.AppSettings["pcc_table"].ToString() + " order by id desc";
            string sql = "select top 1 * from " + ConfigurationManager.AppSettings["pcc_table"].ToString() + " order by CurrentTime  desc";
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
                return ds.Tables[0].Rows[0];
            }
            return null;
        }

        public int m = 0;
        public DataRow getFirstNewData2()
        {
            m++;
            //string sql = "select top " + m.ToString() + " * from " + ConfigurationManager.AppSettings["pcc_table"].ToString() + " order by CurrentTime desc";
            string sql = " SELECT TOP 1 *  FROM " + ConfigurationManager.AppSettings["pcc_table"].ToString() + "  WHERE (rid NOT IN (SELECT TOP "+(m-1).ToString()+" rid FROM " + ConfigurationManager.AppSettings["pcc_table"].ToString() + " order by CurrentTime asc )) order by CurrentTime asc ";

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
                return ds.Tables[0].Rows[ds.Tables[0].Rows.Count -1];
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

        //---------------------------------------------------
        #region 及时数据的处理
        public float proportion = 0.0f;
        public float singlewidth = 0.0f;
        //读取xml的比例数值
        public void getDebugData()
        {
            string filepath = xmlPath;
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            XmlNode xn = rootElem.FirstChild.ChildNodes[rootElem.FirstChild.ChildNodes.Count - 1];
            
            proportion = float.Parse(xn.ChildNodes[0].Attributes["value"].Value.ToString());
            singlewidth = float.Parse(xn.ChildNodes[1].Attributes["value"].Value.ToString());
        }

        //获取要设置的及时数据的类型
        public DataTable TimelyTable;
        public DataTable getTimelydataType()
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
            XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            XmlNode xn = rootElem.FirstChild.ChildNodes[1];
            foreach (XmlNode child in xn.ChildNodes)
            {
                DataRow dr = dt.NewRow();
                dr[0] = child.Attributes["name"].Value.ToString();
                dr[1] = child.Attributes["datatype"].Value.ToString();
                dr[2] = child.Attributes["sort"].Value.ToString();
                dr[3] = "";
                dr[4] = child.Attributes["key"].Value.ToString();
                dt.Rows.Add(dr);
            }
            return dt;
        }


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
        public float pos_1 = 1.1f;
        public float pos_58 = 72.85f;
        public int point_1 = 25231;
        public int point_58 = 7708439;
        public int totalNum = 58;
        //调试 比例 ，变成模型数据
        public DataTable getModelData()
        {
            //获取第一条要处理的数据从数据库
            DataRow dr_first = getFirstNewData2();
            if (comparedatarow(dr_first))
            {
                return null;
            }
            //获取配置的及时数据表
            if( TimelyTable == null )
            {
                TimelyTable = getTimelydataType();
            }
            DataTable dt_timely = TimelyTable;
            //数据库取得的数据复制给配置表
            for (int i = 0; i < dr_first.ItemArray.Length; i++)
            {
                foreach (DataRow dr in dt_timely.Rows)
                {
                    if (dr["sort"].ToString() == i.ToString())
                    {
                        dr["value"] = dr_first[i].ToString();
                    }
                }
            }

            //获取比例
            if (proportion == 0.0f || singlewidth  == 0.0f)
            {
                getDebugData();
            }
            //数据比例调试为本地模型的数据
            //20161006 wge test
            if(1 == 1)
            {
                foreach (DataRow dr in dt_timely.Rows)
                {
                    if (dr["key"].ToString() == "car_tgt")
                    {
                        dr["value"] = getNumPos(int.Parse(dr["value"].ToString()));
                    }
                    else if (dr["key"].ToString() == "car_current")
                    {
                        dr["value"] = getPointPos(int.Parse(dr["value"].ToString()));
                    }
                }
            }
            else
            {
                 foreach (DataRow dr in dt_timely.Rows)
                {
                    if (dr["key"].ToString() == "car_tgt")
                    {
                        dr["value"] = (float.Parse(dr["value"].ToString()) * singlewidth ).ToString();
                    }
                    else if (dr["key"].ToString() == "car_current")
                    {
                        dr["value"] = (float.Parse(dr["value"].ToString()) * proportion).ToString();
                    }
                }
            }
           
            return dt_timely;
        }


        //设置模型数据
        public void setModelData(  int handel)
        {
            DataTable dt_type = getModelData();
            if (dt_type == null)
            {
                return;
            }
            for (int i = 0; i < dt_type.Rows.Count; i++)
            {
                updateValue(dt_type.Rows[i]["name"].ToString(), dt_type.Rows[i]["value"].ToString(), int.Parse(dt_type.Rows[i]["datatype"].ToString()), handel);
            }
        }

        public void setModelData(int handel, DataTable dt_type)
        {
            if (dt_type == null)
            {
                return;
            }
            for (int i = 0; i < dt_type.Rows.Count; i++)
            {
                updateValue(dt_type.Rows[i]["name"].ToString(), dt_type.Rows[i]["value"].ToString(), int.Parse(dt_type.Rows[i]["datatype"].ToString()), handel);
            }
        }

        //比对datarow数据一不一样
        public DataRow lastDr;
        public bool comparedatarow(DataRow drnow)
        {
            if (lastDr == null || drnow == null )
            {
                lastDr = drnow;
                return true;
            }
            if (int.Parse(lastDr[2].ToString()) - int.Parse(drnow[2].ToString()) > 3 || int.Parse(lastDr[2].ToString()) - int.Parse(drnow[2].ToString()) < -3)
            {

            }
            else
            {
                return true;
            }
            for (int i = 0; i < drnow.ItemArray.Length; i++)
            {
                if (drnow[i].ToString() != lastDr[i].ToString())
                {
                    lastDr = drnow;
                    return false;
                }
            }
            return true;
        }
        #endregion

        //---------------------------------------------------
        #region 数据处理
        public DataTable lastData = null;
        public DataTable nowData = null;
        public void controlData( int handle)
        {
            getNowInfo(handle);
            nowData = getModelData();
            if (nowData == null)
                return;
            //前一条记录空
             if (lastData == null )
            {
                //快速过去
                lastData = nowData.Copy();
                lastData.Rows[0]["value"] = lastData.Rows[1]["value"];
                setCarInfo(1000f, 1000f, 1000f, 1);
                setModelData(1, lastData);
                
                Thread.Sleep(100);
                while (carReched())
                 {
                //正常行驶
                setCarInfo(0.3f,0.3f,1.85f,1);
                setModelData(1, nowData);
                break;
                 }
                return;
            }


             
            //比较当前速度
            //速度起来了
            if ( System.Math.Abs( now_spd ) >= 1.85f)
            {
                //比较位置
                float now_pos2 = 0;
                foreach(DataRow dr in nowData.Rows)
                {
                    if (dr["name"].ToString() == "car01_S_CURRENT")
                    {
                        now_pos2 = float.Parse(dr["value"].ToString());
                    }
                }
                if (System.Math.Abs(now_pos - now_pos2) > 1)
                {
                    lastData = nowData.Copy();
                    //快速过去
                    lastData.Rows[0]["value"] = lastData.Rows[1]["value"];
                    setCarInfo(1000f, 1000f, 1000f, 1);
                    setModelData(1, lastData);
                    Thread.Sleep(100);
                    //正常行驶
                    setCarInfo(1000f, 1000f, 1.85f, 1);
                    setModelData(1, nowData);

                    Thread.Sleep(100);
                    setCarInfo(0.3f, 0.3f, 1.85f, 1);
                }
            }

            setModelData(1, nowData);
        }

        //获取当前位置
        public float now_pos = 0;
        //获取当前速度
        public float now_spd = 0;

        public void getNowInfo(int handle)
        {  
            int index = GetIdex.getDicInputIndex("car01_ACT_SPD");
            ComTCPLib.GetInputAsREAL32(handle,index,out now_spd);
            index = GetIdex.getDicInputIndex("car01_AXIS");
            ComTCPLib.GetInputAsREAL32(handle, index, out now_pos);
        }
        public void setCarInfo(float add,float dec,float spd,int handle)
        {
            int index = 0;
            index = GetIdex.getDicOutputIndex("car01_S_ACC");
            ComTCPLib.SetOutputAsREAL32(handle, index, add);
            index = GetIdex.getDicOutputIndex("car01_S_DEC");
            ComTCPLib.SetOutputAsREAL32(handle, index, dec);
            index = GetIdex.getDicOutputIndex("car01_S_SPD");
            ComTCPLib.SetOutputAsREAL32(handle, index, spd);
        }


        public bool carReched()
        {
            bool flag = false;
            int index = GetIdex.getDicInputIndex("car01_RCHD");
            ComTCPLib.GetInputAsBOOL(1, index, out flag);
            return flag;
        }
        #endregion



       
    }
}
