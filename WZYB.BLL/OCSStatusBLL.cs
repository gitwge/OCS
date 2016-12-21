using System;
using System.Data;
using System.Collections.Generic;
using WZYB.Model;
using WZYB.DAL;
namespace WZYB.BLL
{
	/// <summary>
	/// 业务逻辑类OCSStatusBLL 的摘要说明。
	/// </summary>
	public class OCSStatusBLL
	{
		#region  成员方法
		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public static bool Exists(int carId)
		{
			return OCSStatusDAL.Exists(carId);
		}

		/// <summary>
		/// 是否存在该记录
		/// </summary>
		public static bool ExistsBy(string strWhere)
		{
			return OCSStatusDAL.ExistsBy(strWhere);
		}

		/// <summary>
		/// 增加一条数据
		/// </summary>
		public static bool Add(OCSStatus model)
		{
			return OCSStatusDAL.Add(model);
		}

		/// <summary>
		/// 更新一条数据
		/// </summary>
		public static bool Update(OCSStatus model)
		{
			return OCSStatusDAL.Update(model);
		}

		/// <summary>
		/// 删除一条数据
		/// </summary>
		public static bool Delete(int carId)
		{
			return OCSStatusDAL.Delete(carId);
		}

		public static int DeleteBy(string strWhere)
		{
			return OCSStatusDAL.DeleteBy(strWhere);
		}

		/// <summary>
		/// 得到一个对象实体
		/// </summary>
		public static OCSStatus GetModel(int carId)
		{
			return GetModelBy("carId="+carId+" ");
		}
		/// <summary>
		/// 得到一个对象实体
		/// </summary>
		public static OCSStatus GetModelBy(string strWhere)
		{
			DataSet ds = GetList(1,strWhere,"");
			if (ds.Tables[0].Rows.Count > 0)
				return DataTableToList(ds.Tables[0])[0];
			else
				return null;
		}
		/// <summary>
		/// 得到一个对象实体
		/// </summary>
		public static OCSStatus GetModelBy(string strWhere,string filedOrder)
		{
			DataSet ds = GetList(1,strWhere,filedOrder);
			if (ds.Tables[0].Rows.Count > 0)
				return DataTableToList(ds.Tables[0])[0];
			else
				return null;
		}

		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static DataSet GetList(string strWhere)
		{
			return GetList(0, strWhere, "");
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static DataSet GetList(int Top,string strWhere,string filedOrder)
		{
			return OCSStatusDAL.GetList(Top,strWhere,filedOrder);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static DataSet GetList(string strWhere,string filedOrder)
		{
			return GetList(0,strWhere,filedOrder);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static List<OCSStatus> GetModelList(string strWhere)
		{
			DataSet ds = GetList(strWhere);
			return DataTableToList(ds.Tables[0]);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static List<OCSStatus> GetModelList(int Top,string strWhere,string filedOrder)
		{
			DataSet ds = GetList(Top,strWhere,filedOrder);
			return DataTableToList(ds.Tables[0]);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static List<OCSStatus> GetModelList(string strWhere,string filedOrder)
		{
			DataSet ds = GetList(strWhere,filedOrder);
			return DataTableToList(ds.Tables[0]);
		}
		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static List<OCSStatus> DataTableToList(DataTable dt)
		{
			List<OCSStatus> modelList = new List<OCSStatus>();
			int rowsCount = dt.Rows.Count;
			if (rowsCount > 0)
			{
				OCSStatus model;
				for (int n = 0; n < rowsCount; n++)
				{
					model = new OCSStatus();
					if(dt.Rows[n]["carId"].ToString()!="")
					{
						model.carId=int.Parse(dt.Rows[n]["carId"].ToString());
					}
					if(dt.Rows[n]["line"].ToString()!="")
					{
						model.line=int.Parse(dt.Rows[n]["line"].ToString());
					}
					if(dt.Rows[n]["direction"].ToString()!="")
					{
						model.direction=int.Parse(dt.Rows[n]["direction"].ToString());
					}
					if(dt.Rows[n]["sequence"].ToString()!="")
					{
						model.sequence=int.Parse(dt.Rows[n]["sequence"].ToString());
					}
					if(dt.Rows[n]["backLine"].ToString()!="")
					{
						model.backLine=int.Parse(dt.Rows[n]["backLine"].ToString());
					}
					if(dt.Rows[n]["position"].ToString()!="")
					{
						model.position=decimal.Parse(dt.Rows[n]["position"].ToString());
					}
					modelList.Add(model);
				}
			}
			return modelList;
		}

		/// <summary>
		/// 获得数据列表
		/// </summary>
		public static DataSet GetAllList()
		{
			return GetList("");
		}

		#endregion  成员方法

        #region 自定义方法
        
        public static int getCountByLine(int line)
        {
            return OCSStatusDAL.getCountByLine(line);
        }

        #endregion
    }
}

