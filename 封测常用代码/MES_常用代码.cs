using System;
using System.Collections.Generic;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MesCore
{
    #region 数据模型

    /// <summary>
    /// 工单模型（生产工单）
    /// </summary>
    public class WorkOrder
    {
        public string WorkOrderNo { get; set; }      // 工单编号
        public string ProductCode { get; set; }      // 产品编码
        public string ProductName { get; set; }      // 产品名称
        public decimal PlanQty { get; set; }         // 计划数量
        public decimal FinishQty { get; set; }       // 完成数量
        public decimal ScrapQty { get; set; }        // 报废数量
        public string Status { get; set; }           // 状态: Created/Running/Completed/Cancelled
        public string RouteCode { get; set; }        // 工艺路线编码
        public string LineCode { get; set; }         // 产线编码
        public DateTime PlanStartDate { get; set; }  // 计划开始日期
        public DateTime PlanEndDate { get; set; }    // 计划结束日期
        public string CreateBy { get; set; }         // 创建人
        public DateTime CreateTime { get; set; }     // 创建时间
    }

    /// <summary>
    /// 工序模型
    /// </summary>
    public class Operation
    {
        public string OpId { get; set; }             // 工序ID
        public string WorkOrderNo { get; set; }      // 工单编号
        public string OpCode { get; set; }           // 工序编码
        public string OpName { get; set; }           // 工序名称
        public int SeqNo { get; set; }               // 工序顺序号
        public string StationCode { get; set; }      // 工站编码
        public string Status { get; set; }           // 状态
        public decimal InputQty { get; set; }        // 投入数量
        public decimal OutputQty { get; set; }       // 产出数量
        public decimal PassQty { get; set; }         // 良品数量
        public decimal FailQty { get; set; }         // 不良数量
        public DateTime StartTime { get; set; }      // 开始时间
        public DateTime EndTime { get; set; }        // 结束时间
    }

    /// <summary>
    /// 物料追溯模型
    /// </summary>
    public class MaterialTrace
    {
        public string TraceId { get; set; }          // 追溯ID
        public string LotNo { get; set; }            // 批次号
        public string MaterialCode { get; set; }     // 物料编码
        public string MaterialName { get; set; }     // 物料名称
        public string SupplierCode { get; set; }     // 供应商编码
        public decimal Qty { get; set; }             // 数量
        public string Unit { get; set; }             // 单位
        public string WorkOrderNo { get; set; }      // 关联工单
        public string OpCode { get; set; }           // 关联工序
        public DateTime InTime { get; set; }         // 入库时间
        public string SourceLotNo { get; set; }      // 来源批次号
    }

    /// <summary>
    /// 设备数据采集模型
    /// </summary>
    public class EquipmentData
    {
        public string EqId { get; set; }             // 设备ID
        public string EqCode { get; set; }           // 设备编码
        public string EqName { get; set; }           // 设备名称
        public string Status { get; set; }           // 设备状态: Idle/Running/Down/PM
        public string CurrentLotNo { get; set; }     // 当前加工批次
        public string CurrentOpCode { get; set; }    // 当前工序
        public decimal OutputCount { get; set; }     // 产出计数
        public decimal YieldCount { get; set; }      // 良品计数
        public DateTime CollectTime { get; set; }    // 采集时间
        public Dictionary<string, object> Params { get; set; }  // 工艺参数(温度/压力/电压等)
    }

    /// <summary>
    /// 品质检验模型（SPC）
    /// </summary>
    public class QualityRecord
    {
        public string QcId { get; set; }             // 检验记录ID
        public string LotNo { get; set; }            // 批次号
        public string WorkOrderNo { get; set; }      // 工单编号
        public string OpCode { get; set; }           // 工序编码
        public string ItemCode { get; set; }         // 检验项目编码
        public string ItemName { get; set; }         // 检验项目名称
        public decimal StdValue { get; set; }        // 标准值
        public decimal Usl { get; set; }             // 规格上限(USL)
        public decimal Lsl { get; set; }             // 规格下限(LSL)
        public decimal MeasValue { get; set; }       // 实测值
        public string Result { get; set; }           // 判定结果: Pass/Fail
        public string Inspector { get; set; }        // 检验员
        public DateTime InspectTime { get; set; }    // 检验时间
    }

    #endregion

    #region Oracle数据库帮助类

    /// <summary>
    /// Oracle数据库操作帮助类
    /// MES系统核心数据访问层
    /// </summary>
    public class OracleDbHelper : IDisposable
    {
        private readonly string _connStr;
        private OracleConnection _conn;

        public OracleDbHelper(string connectionString)
        {
            _connStr = connectionString;
            _conn = new OracleConnection(_connStr);
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void OpenConnection()
        {
            if (_conn.State == ConnectionState.Closed)
            {
                _conn.Open();
            }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConnection()
        {
            if (_conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }

        /// <summary>
        /// 查询工单列表
        /// </summary>
        public List<WorkOrder> GetWorkOrders(string status = null, string lineCode = null)
        {
            var list = new List<WorkOrder>();
            string sql = @"
                SELECT WORK_ORDER_NO, PRODUCT_CODE, PRODUCT_NAME, PLAN_QTY, FINISH_QTY,
                       SCRAP_QTY, STATUS, ROUTE_CODE, LINE_CODE,
                       PLAN_START_DATE, PLAN_END_DATE, CREATE_BY, CREATE_TIME
                FROM MES_WORK_ORDER
                WHERE 1=1";

            if (!string.IsNullOrEmpty(status))
                sql += " AND STATUS = :STATUS";
            if (!string.IsNullOrEmpty(lineCode))
                sql += " AND LINE_CODE = :LINE_CODE";
            sql += " ORDER BY CREATE_TIME DESC";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                if (!string.IsNullOrEmpty(status))
                    cmd.Parameters.Add(":STATUS", status);
                if (!string.IsNullOrEmpty(lineCode))
                    cmd.Parameters.Add(":LINE_CODE", lineCode);

                OpenConnection();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new WorkOrder
                        {
                            WorkOrderNo = reader["WORK_ORDER_NO"].ToString(),
                            ProductCode = reader["PRODUCT_CODE"].ToString(),
                            ProductName = reader["PRODUCT_NAME"].ToString(),
                            PlanQty = Convert.ToDecimal(reader["PLAN_QTY"]),
                            FinishQty = Convert.ToDecimal(reader["FINISH_QTY"]),
                            ScrapQty = Convert.ToDecimal(reader["SCRAP_QTY"]),
                            Status = reader["STATUS"].ToString(),
                            RouteCode = reader["ROUTE_CODE"].ToString(),
                            LineCode = reader["LINE_CODE"].ToString(),
                            PlanStartDate = Convert.ToDateTime(reader["PLAN_START_DATE"]),
                            PlanEndDate = Convert.ToDateTime(reader["PLAN_END_DATE"]),
                            CreateBy = reader["CREATE_BY"].ToString(),
                            CreateTime = Convert.ToDateTime(reader["CREATE_TIME"])
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 创建工单
        /// </summary>
        public bool CreateWorkOrder(WorkOrder wo)
        {
            string sql = @"
                INSERT INTO MES_WORK_ORDER
                    (WORK_ORDER_NO, PRODUCT_CODE, PRODUCT_NAME, PLAN_QTY, FINISH_QTY,
                     SCRAP_QTY, STATUS, ROUTE_CODE, LINE_CODE,
                     PLAN_START_DATE, PLAN_END_DATE, CREATE_BY, CREATE_TIME)
                VALUES
                    (:WORK_ORDER_NO, :PRODUCT_CODE, :PRODUCT_NAME, :PLAN_QTY, 0,
                     0, 'Created', :ROUTE_CODE, :LINE_CODE,
                     :PLAN_START_DATE, :PLAN_END_DATE, :CREATE_BY, SYSDATE)";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":WORK_ORDER_NO", wo.WorkOrderNo);
                cmd.Parameters.Add(":PRODUCT_CODE", wo.ProductCode);
                cmd.Parameters.Add(":PRODUCT_NAME", wo.ProductName);
                cmd.Parameters.Add(":PLAN_QTY", wo.PlanQty);
                cmd.Parameters.Add(":ROUTE_CODE", wo.RouteCode);
                cmd.Parameters.Add(":LINE_CODE", wo.LineCode);
                cmd.Parameters.Add(":PLAN_START_DATE", wo.PlanStartDate);
                cmd.Parameters.Add(":PLAN_END_DATE", wo.PlanEndDate);
                cmd.Parameters.Add(":CREATE_BY", wo.CreateBy);

                OpenConnection();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 工单开工（更新状态为Running）
        /// </summary>
        public bool StartWorkOrder(string workOrderNo)
        {
            string sql = @"
                UPDATE MES_WORK_ORDER
                SET STATUS = 'Running',
                    START_TIME = SYSDATE
                WHERE WORK_ORDER_NO = :WORK_ORDER_NO
                  AND STATUS = 'Created'";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":WORK_ORDER_NO", workOrderNo);
                OpenConnection();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 记录工序报工
        /// </summary>
        public bool ReportOperation(Operation op)
        {
            string sql = @"
                INSERT INTO MES_OPERATION_REPORT
                    (REPORT_ID, WORK_ORDER_NO, OP_CODE, OP_NAME, SEQ_NO,
                     STATION_CODE, INPUT_QTY, OUTPUT_QTY, PASS_QTY, FAIL_QTY,
                     START_TIME, END_TIME, REPORT_BY, REPORT_TIME)
                VALUES
                    (MES_SEQ_REPORT_ID.NEXTVAL, :WORK_ORDER_NO, :OP_CODE, :OP_NAME, :SEQ_NO,
                     :STATION_CODE, :INPUT_QTY, :OUTPUT_QTY, :PASS_QTY, :FAIL_QTY,
                     :START_TIME, :END_TIME, :REPORT_BY, SYSDATE)";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":WORK_ORDER_NO", op.WorkOrderNo);
                cmd.Parameters.Add(":OP_CODE", op.OpCode);
                cmd.Parameters.Add(":OP_NAME", op.OpName);
                cmd.Parameters.Add(":SEQ_NO", op.SeqNo);
                cmd.Parameters.Add(":STATION_CODE", op.StationCode);
                cmd.Parameters.Add(":INPUT_QTY", op.InputQty);
                cmd.Parameters.Add(":OUTPUT_QTY", op.OutputQty);
                cmd.Parameters.Add(":PASS_QTY", op.PassQty);
                cmd.Parameters.Add(":FAIL_QTY", op.FailQty);
                cmd.Parameters.Add(":START_TIME", op.StartTime);
                cmd.Parameters.Add(":END_TIME", op.EndTime);
                cmd.Parameters.Add(":REPORT_BY", "MES_SYSTEM");

                OpenConnection();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 物料追溯查询（通过批次号追溯全流程）
        /// </summary>
        public DataTable TraceMaterialByLot(string lotNo)
        {
            string sql = @"
                SELECT T.TRACE_ID, T.LOT_NO, T.MATERIAL_CODE, T.MATERIAL_NAME,
                       T.SUPPLIER_CODE, T.QTY, T.UNIT, T.WORK_ORDER_NO,
                       T.OP_CODE, T.IN_TIME, T.SOURCE_LOT_NO
                FROM MES_MATERIAL_TRACE T
                WHERE T.LOT_NO = :LOT_NO
                ORDER BY T.IN_TIME ASC";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":LOT_NO", lotNo);
                OpenConnection();

                var dt = new DataTable();
                using (var adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
                return dt;
            }
        }

        /// <summary>
        /// 记录物料投料
        /// </summary>
        public bool RecordMaterialTrace(MaterialTrace trace)
        {
            string sql = @"
                INSERT INTO MES_MATERIAL_TRACE
                    (TRACE_ID, LOT_NO, MATERIAL_CODE, MATERIAL_NAME,
                     SUPPLIER_CODE, QTY, UNIT, WORK_ORDER_NO,
                     OP_CODE, IN_TIME, SOURCE_LOT_NO)
                VALUES
                    (MES_SEQ_TRACE_ID.NEXTVAL, :LOT_NO, :MATERIAL_CODE, :MATERIAL_NAME,
                     :SUPPLIER_CODE, :QTY, :UNIT, :WORK_ORDER_NO,
                     :OP_CODE, SYSDATE, :SOURCE_LOT_NO)";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":LOT_NO", trace.LotNo);
                cmd.Parameters.Add(":MATERIAL_CODE", trace.MaterialCode);
                cmd.Parameters.Add(":MATERIAL_NAME", trace.MaterialName);
                cmd.Parameters.Add(":SUPPLIER_CODE", trace.SupplierCode);
                cmd.Parameters.Add(":QTY", trace.Qty);
                cmd.Parameters.Add(":UNIT", trace.Unit);
                cmd.Parameters.Add(":WORK_ORDER_NO", trace.WorkOrderNo);
                cmd.Parameters.Add(":OP_CODE", trace.OpCode);
                cmd.Parameters.Add(":SOURCE_LOT_NO", trace.SourceLotNo ?? string.Empty);

                OpenConnection();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 保存设备采集数据
        /// </summary>
        public bool SaveEquipmentData(EquipmentData eqData)
        {
            string sql = @"
                INSERT INTO MES_EQUIPMENT_DATA
                    (DATA_ID, EQ_CODE, EQ_NAME, STATUS, CURRENT_LOT_NO,
                     CURRENT_OP_CODE, OUTPUT_COUNT, YIELD_COUNT, COLLECT_TIME)
                VALUES
                    (MES_SEQ_EQ_DATA_ID.NEXTVAL, :EQ_CODE, :EQ_NAME, :STATUS,
                     :CURRENT_LOT_NO, :CURRENT_OP_CODE, :OUTPUT_COUNT,
                     :YIELD_COUNT, SYSDATE)";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":EQ_CODE", eqData.EqCode);
                cmd.Parameters.Add(":EQ_NAME", eqData.EqName);
                cmd.Parameters.Add(":STATUS", eqData.Status);
                cmd.Parameters.Add(":CURRENT_LOT_NO", eqData.CurrentLotNo ?? string.Empty);
                cmd.Parameters.Add(":CURRENT_OP_CODE", eqData.CurrentOpCode ?? string.Empty);
                cmd.Parameters.Add(":OUTPUT_COUNT", eqData.OutputCount);
                cmd.Parameters.Add(":YIELD_COUNT", eqData.YieldCount);

                OpenConnection();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 保存品质检验记录
        /// </summary>
        public bool SaveQualityRecord(QualityRecord qr)
        {
            string sql = @"
                INSERT INTO MES_QUALITY_RECORD
                    (QC_ID, LOT_NO, WORK_ORDER_NO, OP_CODE, ITEM_CODE, ITEM_NAME,
                     STD_VALUE, USL, LSL, MEAS_VALUE, RESULT,
                     INSPECTOR, INSPECT_TIME)
                VALUES
                    (MES_SEQ_QC_ID.NEXTVAL, :LOT_NO, :WORK_ORDER_NO, :OP_CODE,
                     :ITEM_CODE, :ITEM_NAME, :STD_VALUE, :USL, :LSL,
                     :MEAS_VALUE, :RESULT, :INSPECTOR, SYSDATE)";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":LOT_NO", qr.LotNo);
                cmd.Parameters.Add(":WORK_ORDER_NO", qr.WorkOrderNo);
                cmd.Parameters.Add(":OP_CODE", qr.OpCode);
                cmd.Parameters.Add(":ITEM_CODE", qr.ItemCode);
                cmd.Parameters.Add(":ITEM_NAME", qr.ItemName);
                cmd.Parameters.Add(":STD_VALUE", qr.StdValue);
                cmd.Parameters.Add(":USL", qr.Usl);
                cmd.Parameters.Add(":LSL", qr.Lsl);
                cmd.Parameters.Add(":MEAS_VALUE", qr.MeasValue);
                cmd.Parameters.Add(":RESULT", qr.Result);
                cmd.Parameters.Add(":INSPECTOR", qr.Inspector);

                OpenConnection();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// 调用Oracle存储过程（通用方法）
        /// </summary>
        public DataSet ExecuteProcedure(string procName, OracleParameter[] parameters)
        {
            using (var cmd = new OracleCommand(procName, _conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                OpenConnection();
                var ds = new DataSet();
                using (var adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(ds);
                }
                return ds;
            }
        }

        /// <summary>
        /// 获取产线实时良率统计
        /// </summary>
        public DataTable GetYieldStatistics(string lineCode, DateTime startDate, DateTime endDate)
        {
            string sql = @"
                SELECT O.WORK_ORDER_NO, O.OP_CODE, O.OP_NAME,
                       SUM(O.OUTPUT_QTY) AS TOTAL_OUTPUT,
                       SUM(O.PASS_QTY) AS TOTAL_PASS,
                       SUM(O.FAIL_QTY) AS TOTAL_FAIL,
                       ROUND(SUM(O.PASS_QTY) / NULLIF(SUM(O.OUTPUT_QTY), 0) * 100, 2) AS YIELD_RATE
                FROM MES_OPERATION_REPORT O
                WHERE O.LINE_CODE = :LINE_CODE
                  AND O.REPORT_TIME BETWEEN :START_DATE AND :END_DATE
                GROUP BY O.WORK_ORDER_NO, O.OP_CODE, O.OP_NAME
                ORDER BY O.OP_CODE";

            using (var cmd = new OracleCommand(sql, _conn))
            {
                cmd.Parameters.Add(":LINE_CODE", lineCode);
                cmd.Parameters.Add(":START_DATE", startDate);
                cmd.Parameters.Add(":END_DATE", endDate);

                OpenConnection();
                var dt = new DataTable();
                using (var adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
                return dt;
            }
        }

        public void Dispose()
        {
            CloseConnection();
            _conn?.Dispose();
        }
    }

    #endregion

    #region ERP接口对接

    /// <summary>
    /// ERP接口服务（MES与ERP数据同步）
    /// </summary>
    public class ErpInterfaceService
    {
        private readonly OracleDbHelper _db;

        public ErpInterfaceService(OracleDbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// 从ERP同步工单到MES
        /// </summary>
        public int SyncWorkOrdersFromErp()
        {
            // 调用存储过程同步ERP工单
            var param = new OracleParameter[]
            {
                new OracleParameter("v_count", OracleDbType.Int32, ParameterDirection.Output)
            };

            _db.ExecuteProcedure("MES_PKG_SYNC.SYNC_WORK_ORDER_FROM_ERP", param);
            return Convert.ToInt32(param[0].Value);
        }

        /// <summary>
        /// 从MES回传工单完工数据到ERP
        /// </summary>
        public bool ReportCompletionToErp(string workOrderNo, decimal finishQty, decimal scrapQty)
        {
            var param = new OracleParameter[]
            {
                new OracleParameter("v_wo_no", workOrderNo),
                new OracleParameter("v_finish_qty", finishQty),
                new OracleParameter("v_scrap_qty", scrapQty),
                new OracleParameter("v_result", OracleDbType.Varchar2, 200).Direction = ParameterDirection.Output
            };

            _db.ExecuteProcedure("MES_PKG_SYNC.REPORT_WO_COMPLETION", param);
            return param[2].Value.ToString() == "SUCCESS";
        }

        /// <summary>
        /// 从ERP同步物料主数据
        /// </summary>
        public int SyncMaterialMaster()
        {
            var param = new OracleParameter[]
            {
                new OracleParameter("v_count", OracleDbType.Int32, ParameterDirection.Output)
            };

            _db.ExecuteProcedure("MES_PKG_SYNC.SYNC_MATERIAL_MASTER", param);
            return Convert.ToInt32(param[0].Value);
        }
    }

    #endregion

    #region SPC统计分析

    /// <summary>
    /// SPC统计过程控制服务
    /// 半导体行业品质管控核心模块
    /// </summary>
    public class SpcService
    {
        private readonly OracleDbHelper _db;

        public SpcService(OracleDbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// 计算Cpk（过程能力指数）
        /// Cpk = Min[(USL-X̄)/(3σ), (X̄-LSL)/(3σ)]
        /// </summary>
        public decimal CalculateCpk(List<decimal> values, decimal usl, decimal lsl)
        {
            if (values == null || values.Count < 2)
                return 0;

            double mean = 0;
            foreach (var v in values) mean += (double)v;
            mean /= values.Count;

            double sumSq = 0;
            foreach (var v in values) sumSq += Math.Pow((double)v - mean, 2);
            double stdDev = Math.Sqrt(sumSq / (values.Count - 1));

            if (stdDev == 0) return 0;

            double cpu = ((double)usl - mean) / (3 * stdDev);
            double cpl = (mean - (double)lsl) / (3 * stdDev);

            return (decimal)Math.Min(cpu, cpl);
        }

        /// <summary>
        /// 获取SPC分析数据
        /// </summary>
        public DataTable GetSpcData(string itemCode, DateTime startTime, DateTime endTime)
        {
            string sql = @"
                SELECT QC_ID, LOT_NO, MEAS_VALUE, RESULT, INSPECT_TIME
                FROM MES_QUALITY_RECORD
                WHERE ITEM_CODE = :ITEM_CODE
                  AND INSPECT_TIME BETWEEN :START_TIME AND :END_TIME
                ORDER BY INSPECT_TIME ASC";

            using (var cmd = new OracleCommand(sql, _db.GetConnection()))
            {
                cmd.Parameters.Add(":ITEM_CODE", itemCode);
                cmd.Parameters.Add(":START_TIME", startTime);
                cmd.Parameters.Add(":END_TIME", endTime);

                var dt = new DataTable();
                using (var adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
                return dt;
            }
        }

        /// <summary>
        /// 判定SPC规则（Western Electric Rules简化版）
        /// </summary>
        public List<string> CheckSpcRules(List<decimal> values, decimal usl, decimal lsl, decimal mean)
        {
            var alerts = new List<string>();

            if (values.Count < 2) return alerts;

            // 规则1: 超出控制限
            if (values[values.Count - 1] > usl || values[values.Count - 1] < lsl)
                alerts.Add("超出规格上下限");

            // 规则2: 连续7点在中心线同侧
            if (values.Count >= 7)
            {
                bool aboveMean = true;
                bool allSame = true;
                for (int i = values.Count - 7; i < values.Count; i++)
                {
                    if (i == values.Count - 7)
                        aboveMean = values[i] > mean;
                    else if ((values[i] > mean) != aboveMean)
                    {
                        allSame = false;
                        break;
                    }
                }
                if (allSame) alerts.Add("连续7点在中心线同侧");
            }

            return alerts;
        }
    }

    #endregion

    #region 设备通信与数据采集

    /// <summary>
    /// 设备数据采集服务
    /// 负责从产线设备采集实时数据
    /// </summary>
    public class EquipmentCollectService
    {
        private readonly OracleDbHelper _db;

        public EquipmentCollectService(OracleDbHelper db)
        {
            _db = db;
        }

        /// <summary>
        /// 解析设备上传的工艺参数数据
        /// 常见格式: EQ001|Running|LOT20260401|OP10|150|148|0.85
        /// </summary>
        public EquipmentData ParseEquipmentMessage(string message)
        {
            var parts = message.Split('|');
            if (parts.Length < 6) return null;

            return new EquipmentData
            {
                EqCode = parts[0],
                Status = parts[1],
                CurrentLotNo = parts[2],
                CurrentOpCode = parts[3],
                OutputCount = decimal.Parse(parts[4]),
                YieldCount = decimal.Parse(parts[5]),
                CollectTime = DateTime.Now,
                Params = new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// 批量保存设备采集数据
        /// </summary>
        public int BatchSaveEquipmentData(List<EquipmentData> dataList)
        {
            int count = 0;
            foreach (var data in dataList)
            {
                if (_db.SaveEquipmentData(data))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 获取设备OEE（设备综合效率）
        /// OEE = 可用率 × 表现率 × 良品率
        /// </summary>
        public decimal CalculateOEE(string eqCode, DateTime startTime, DateTime endTime)
        {
            string sql = @"
                SELECT
                    ROUND(AVG(NVL(AVAILABILITY_RATE, 0)) * 100, 2) AS AVAILABILITY,
                    ROUND(AVG(NVL(PERFORMANCE_RATE, 0)) * 100, 2) AS PERFORMANCE,
                    ROUND(AVG(NVL(QUALITY_RATE, 0)) * 100, 2) AS QUALITY
                FROM MES_EQUIPMENT_OEE
                WHERE EQ_CODE = :EQ_CODE
                  AND COLLECT_TIME BETWEEN :START_TIME AND :END_TIME";

            using (var cmd = new OracleCommand(sql, _db.GetConnection()))
            {
                cmd.Parameters.Add(":EQ_CODE", eqCode);
                cmd.Parameters.Add(":START_TIME", startTime);
                cmd.Parameters.Add(":END_TIME", endTime);

                _db.OpenConnection();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        decimal availability = Convert.ToDecimal(reader["AVAILABILITY"]);
                        decimal performance = Convert.ToDecimal(reader["PERFORMANCE"]);
                        decimal quality = Convert.ToDecimal(reader["QUALITY"]);
                        return (availability * performance * quality) / 10000;
                    }
                }
            }
            return 0;
        }
    }

    #endregion
}
