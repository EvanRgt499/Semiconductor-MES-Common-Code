using System;
using System.Data;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using MES.Common; 
using OfficeOpenXml;

namespace MES.Web
{
    public partial class WorkOrderManage : System.Web.UI.Page
    {
        private int pageSize = 20;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState["CurrentPage"] = 1;
                BindWorkOrderList("");
            }
        }

        // 绑定工单列表
        private void BindWorkOrderList(string workOrderNo)
        {
            try
            {
                int currentPage = Convert.ToInt32(ViewState["CurrentPage"]);
                int startRow = (currentPage - 1) * pageSize + 1;
                int endRow = currentPage * pageSize;

                // 构建查询条件
                string whereClause = " WHERE 1=1";
                if (!string.IsNullOrEmpty(workOrderNo))
                {
                    whereClause += " AND work_order_no LIKE :work_order_no";
                }

                // 计算总数
                string countSql = "SELECT COUNT(*) FROM mes_work_order" + whereClause;
                OracleParameter[] countParam = GetParameters(workOrderNo);
                DataTable countDt = OracleHelper.Query(countSql, countParam);
                int totalRows = Convert.ToInt32(countDt.Rows[0][0]);

                // 计算总页数
                int totalPages = (totalRows + pageSize - 1) / pageSize;
                lblPageInfo.Text = $"第 {currentPage} 页，共 {totalPages} 页，总记录数：{totalRows}";

                // 构建分页查询
                string sql = $@"
                    SELECT * FROM (
                        SELECT ROW_NUMBER() OVER (ORDER BY create_time DESC) AS rownum,
                               work_order_no, part_no, qty_plan, qty_complete,
                               status, line_code, create_time
                        FROM mes_work_order
                        {whereClause}
                    ) WHERE rownum BETWEEN :start_row AND :end_row";

                OracleParameter[] param = GetParameters(workOrderNo);
                Array.Resize(ref param, param.Length + 2);
                param[param.Length - 2] = new OracleParameter(":start_row", startRow);
                param[param.Length - 1] = new OracleParameter(":end_row", endRow);

                DataTable dt = OracleHelper.Query(sql, param);
                gvWorkOrder.DataSource = dt;
                gvWorkOrder.DataBind();

                // 控制分页按钮状态
                btnFirst.Enabled = currentPage > 1;
                btnPrev.Enabled = currentPage > 1;
                btnNext.Enabled = currentPage < totalPages;
                btnLast.Enabled = currentPage < totalPages;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "查询失败：" + ex.Message;
                lblMessage.CssClass = "error";
            }
        }

        private OracleParameter[] GetParameters(string workOrderNo)
        {
            var parameters = new System.Collections.Generic.List<OracleParameter>();
            if (!string.IsNullOrEmpty(workOrderNo))
            {
                parameters.Add(new OracleParameter(":work_order_no", "%" + workOrderNo + "%"));
            }
            return parameters.ToArray();
        }

        // 查询按钮点击
        protected void btnQuery_Click(object sender, EventArgs e)
        {
            ViewState["CurrentPage"] = 1;
            string woNo = txtWorkOrderNo.Text.Trim();
            BindWorkOrderList(woNo);
        }

        // 导出Excel
        protected void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                // 重新查询数据（不分页）
                string sql = @"
                    SELECT 
                        work_order_no AS 工单号, 
                        part_no AS 物料号, 
                        qty_plan AS 计划数量, 
                        qty_complete AS 已完成数量, 
                        status AS 状态, 
                        line_code AS 产线, 
                        create_time AS 创建时间
                    FROM mes_work_order
                    ORDER BY create_time DESC";

                DataTable dt = OracleHelper.Query(sql);

                if (dt.Rows.Count == 0)
                {
                    lblMessage.Text = "无数据可导出";
                    lblMessage.CssClass = "info";
                    return;
                }

                // 使用EPPlus导出
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("工单列表");

                    // 设置表头
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = dt.Columns[i].ColumnName;
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                        worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // 填充数据
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            var cellValue = dt.Rows[i][j];
                            if (cellValue != DBNull.Value)
                            {
                                if (cellValue is DateTime dateValue)
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = dateValue;
                                    worksheet.Cells[i + 2, j + 1].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                                }
                                else
                                {
                                    worksheet.Cells[i + 2, j + 1].Value = cellValue;
                                }
                            }
                        }
                    }

                    // 自动调整列宽
                    worksheet.Cells.AutoFitColumns();

                    // 输出到响应
                    System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                    response.Clear();
                    response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    response.AddHeader("content-disposition", "attachment; filename=工单列表_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
                    response.BinaryWrite(package.GetAsByteArray());
                    response.End();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "导出失败：" + ex.Message;
                lblMessage.CssClass = "error";
            }
        }

        // 分页按钮点击事件
        protected void btnFirst_Click(object sender, EventArgs e)
        {
            ViewState["CurrentPage"] = 1;
            BindWorkOrderList(txtWorkOrderNo.Text.Trim());
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            int currentPage = Convert.ToInt32(ViewState["CurrentPage"]);
            if (currentPage > 1)
            {
                ViewState["CurrentPage"] = currentPage - 1;
                BindWorkOrderList(txtWorkOrderNo.Text.Trim());
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            int currentPage = Convert.ToInt32(ViewState["CurrentPage"]);
            ViewState["CurrentPage"] = currentPage + 1;
            BindWorkOrderList(txtWorkOrderNo.Text.Trim());
        }

        protected void btnLast_Click(object sender, EventArgs e)
        {
            // 计算总页数
            string workOrderNo = txtWorkOrderNo.Text.Trim();
            string whereClause = " WHERE 1=1";
            if (!string.IsNullOrEmpty(workOrderNo))
            {
                whereClause += " AND work_order_no LIKE :work_order_no";
            }

            string countSql = "SELECT COUNT(*) FROM mes_work_order" + whereClause;
            OracleParameter[] countParam = GetParameters(workOrderNo);
            DataTable countDt = OracleHelper.Query(countSql, countParam);
            int totalRows = Convert.ToInt32(countDt.Rows[0][0]);
            int totalPages = (totalRows + pageSize - 1) / pageSize;

            ViewState["CurrentPage"] = totalPages;
            BindWorkOrderList(workOrderNo);
        }

        // 页码输入框事件
        protected void txtPage_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtPage.Text, out int page) && page > 0)
            {
                ViewState["CurrentPage"] = page;
                BindWorkOrderList(txtWorkOrderNo.Text.Trim());
            }
        }
    }
}