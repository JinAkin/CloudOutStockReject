using System.Data.SqlClient;

namespace CloudOutStockReject
{
    //更新K3-CLOUD对应的单据记录
    public class Generate
    {
        SqlList sqlList=new SqlList();

        /// <summary>
        /// 更新K3-CLOUD对应单据
        /// </summary>
        /// <param name="orderno"></param>
        public void UpdateK3Record(string orderno)
        {
            var sqlscript = sqlList.GetUpdate(orderno);
            Generdt(sqlscript);
        }

        /// <summary>
        /// 按照指定的SQL语句执行记录
        /// </summary>
        private void Generdt(string sqlscript)
        {
            using (var sql = GetCloudConn())
            {
                sql.Open();
                var sqlCommand = new SqlCommand(sqlscript, sql);
                sqlCommand.ExecuteNonQuery();
                sql.Close();
            }
        }

        /// <summary>
        /// 获取K3-Cloud连接
        /// </summary>
        /// <returns></returns>
        public SqlConnection GetCloudConn()
        {
            var sqlcon = new SqlConnection(GetConnectionString());
            return sqlcon;
        }


        /// <summary>
        /// 连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetConnectionString()
        {
            var strcon = @"Data Source='192.168.1.228';Initial Catalog='AIS20181204095717';Persist Security Info=True;User ID='sa'; Password='kingdee';
                       Pooling=true;Max Pool Size=40000;Min Pool Size=0";
            return strcon;
        }
    }
}
