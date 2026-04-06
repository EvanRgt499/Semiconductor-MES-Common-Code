using System;
using System.Data;
using System.Configuration;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace MES.Common
{
    public class OracleHelper
    {
        // 从配置文件读取连接字符串
        private static string connStr = ConfigurationManager.ConnectionStrings["MESOracle"].ConnectionString ?? "Data Source=mesdb;User Id=mes;Password=mes123;";

        /// <summary>
        /// 执行查询并返回DataTable
        /// </summary>
        public static DataTable Query(string sql, params OracleParameter[] param)
        {
            DataTable dt = new DataTable();
            try
            {
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
            catch (Exception ex)
            {
                LogError("Query", sql, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行非查询操作并返回影响行数
        /// </summary>
        public static int Execute(string sql, params OracleParameter[] param)
        {
            try
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
            catch (Exception ex)
            {
                LogError("Execute", sql, ex);
                throw;
            }
        }

        /// <summary>
        /// 异步执行查询并返回DataTable
        /// </summary>
        public static async Task<DataTable> QueryAsync(string sql, params OracleParameter[] param)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    await conn.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        if (param != null)
                            cmd.Parameters.AddRange(param);

                        using (OracleDataAdapter sda = new OracleDataAdapter(cmd))
                        {
                            await Task.Run(() => sda.Fill(dt));
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                LogError("QueryAsync", sql, ex);
                throw;
            }
        }

        /// <summary>
        /// 异步执行非查询操作并返回影响行数
        /// </summary>
        public static async Task<int> ExecuteAsync(string sql, params OracleParameter[] param)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    await conn.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        if (param != null)
                            cmd.Parameters.AddRange(param);

                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ExecuteAsync", sql, ex);
                throw;
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private static void LogError(string method, string sql, Exception ex)
        {
            // 实际项目中可使用日志框架如Log4net、NLog等
            string logMessage = $"[{DateTime.Now}] OracleHelper.{method} 错误: {ex.Message}\nSQL: {sql}\n堆栈: {ex.StackTrace}";
            System.Diagnostics.Debug.WriteLine(logMessage);
            // 可根据需要写入日志文件
        }
    }
}