// 半导体封装MES系统 - 数据库操作帮助类
// 作者：[孟斯辰]
// 日期：2026-04-06
// 说明：封装Oracle数据库操作，提供同步和异步方法

using System;
using System.Data;
using System.Configuration;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace MES.Common
{
    /// <summary>
    /// Oracle数据库操作帮助类
    /// 用于半导体封装MES系统的数据库访问
    /// </summary>
    public class OracleHelper
    {
        // 从配置文件读取连接字符串
        // 连接字符串格式：Data Source=mesdb;User Id=mes;Password=mes123;
        private static string connStr = ConfigurationManager.ConnectionStrings["MESOracle"].ConnectionString ?? "Data Source=mesdb;User Id=mes;Password=mes123;";

        /// <summary>
        /// 执行查询并返回DataTable
        /// 用于查询工单、生产记录等数据
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="param">SQL参数数组</param>
        /// <returns>查询结果DataTable</returns>
        public static DataTable Query(string sql, params OracleParameter[] param)
        {
            DataTable dt = new DataTable();
            try
            {
                // 创建数据库连接
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    // 创建命令对象
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        // 添加参数
                        if (param != null)
                            cmd.Parameters.AddRange(param);

                        // 执行查询
                        OracleDataAdapter sda = new OracleDataAdapter(cmd);
                        sda.Fill(dt);
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                // 记录错误日志
                LogError("Query", sql, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行非查询操作并返回影响行数
        /// 用于更新、删除、插入操作
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">SQL参数数组</param>
        /// <returns>影响的行数</returns>
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
        /// 用于大数据量查询，避免UI卡顿
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="param">SQL参数数组</param>
        /// <returns>查询结果DataTable</returns>
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
        /// 用于异步更新操作
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="param">SQL参数数组</param>
        /// <returns>影响的行数</returns>
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
        /// 实际项目中可使用Log4net、NLog等日志框架
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="sql">执行的SQL语句</param>
        /// <param name="ex">异常对象</param>
        private static void LogError(string method, string sql, Exception ex)
        {
            string logMessage = $"[{DateTime.Now}] OracleHelper.{method} 错误: {ex.Message}\nSQL: {sql}\n堆栈: {ex.StackTrace}";
            System.Diagnostics.Debug.WriteLine(logMessage);
            // 可根据需要写入日志文件
            // 例如：System.IO.File.AppendAllText("oracle_error.log", logMessage + "\n");
        }
    }

    // 历史记录：
    // 2026-04-06：初始版本
    // 后续计划：添加事务支持、批量操作等功能
}