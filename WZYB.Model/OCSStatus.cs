using System;
namespace WZYB.Model
{
	/// <summary>
	/// 实体类OCSStatus
	/// </summary>
	[Serializable]
    public class OCSStatus : IComparable
	{
		#region Model
		private int _carid;
		private int _line;
		private int _direction;
		private int _sequence;
		private int _backline;
		private decimal _position;
		/// <summary>
		/// 车辆ID
		/// </summary>
		public int carId
		{
			set{ _carid=value;}
			get{return _carid;}
		}
		/// <summary>
		/// 所在驱动段
		/// </summary>
		public int line
		{
			set{ _line=value;}
			get{return _line;}
		}
		/// <summary>
		/// 方向0停止　1向前　2向后
		/// </summary>
		public int direction
		{
			set{ _direction=value;}
			get{return _direction;}
		}
		/// <summary>
		/// 所在驱动段顺序
		/// </summary>
		public int sequence
		{
			set{ _sequence=value;}
			get{return _sequence;}
		}
		/// <summary>
		/// 上一条驱动段
		/// </summary>
		public int backLine
		{
			set{ _backline=value;}
			get{return _backline;}
		}
		/// <summary>
		/// 具体位置
		/// </summary>
		public decimal position
		{
			set{ _position=value;}
			get{return _position;}
		}
		#endregion Model

        #region 实现比较接口的CompareTo方法
        public int CompareTo(object obj)
        {
            int res = 0;
            try
            {
                OCSStatus sObj = (OCSStatus)obj;
                if (this.sequence > sObj.sequence)
                {
                    res = 1;
                }
                else if (this.sequence < sObj.sequence)
                {
                    res = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("比较异常", ex.InnerException);
            }
            return res;
        }
        #endregion

	}
}

