using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MES.Common
{
    public class OracleHelper
    {
        // 实际项目从配置文件读取
        private static string connStr = "Data Source=mesdb;User Id=mes;Password=mes123;";

        public static DataTable Query(string sql, params OracleParameter[] param)
        {
            DataTable dt = new DataTable();
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    if (param != null)
                        cmd.Parameters.AddRange(param);

                    OracleDataAdapter sda = new OracleDataAdapter(cmd);
                    sda.Fill(dt);
                }
            }
            return dt;
        }

        public static int Execute(string sql, params OracleParameter[] param)
        {
            int rows = 0;
            using (OracleConnection conn = new OracleConnection(connStr))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand(sql, conn))
                {
                    if (param != null)
                        cmd.Parameters.AddRange(param);

                    rows = cmd.ExecuteNonQuery();
                }
            }
            return rows;
        }
    }
}