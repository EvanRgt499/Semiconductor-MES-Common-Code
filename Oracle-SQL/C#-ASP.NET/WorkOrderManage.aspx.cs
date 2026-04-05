using System;
using System.Data;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using MES.Common; 

namespace MES.Web
{
    public partial class WorkOrderManage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindWorkOrderList("");
            }
        }

        // 绑定工单列表
        private void BindWorkOrderList(string workOrderNo)
        {
            string sql = @"
                SELECT 
                    work_order_no, 
                    part_no, 
                    qty_plan, 
                    qty_complete, 
                    status, 
                    line_code, 
                    create_time
                FROM mes_work_order";

            // 如果有工单号，加查询条件
            if (!string.IsNullOrEmpty(workOrderNo))
            {
                sql += " WHERE work_order_no = :work_order_no";
            }

            sql += " ORDER BY create_time DESC";

            OracleParameter[] param = null;
            if (!string.IsNullOrEmpty(workOrderNo))
            {
                param = new OracleParameter[] {
                    new OracleParameter(":work_order_no", workOrderNo)
                };
            }

            DataTable dt = OracleHelper.Query(sql, param);
            gvWorkOrder.DataSource = dt;
            gvWorkOrder.DataBind();
        }

        // 查询按钮点击
        protected void btnQuery_Click(object sender, EventArgs e)
        {
            string woNo = txtWorkOrderNo.Text.Trim();
            BindWorkOrderList(woNo);
        }

        // 导出Excel
        protected void btnExport_Click(object sender, EventArgs e)
        {
            // 重新查询数据
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

            // 导出逻辑
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.Clear();
            response.Buffer = true;
            response.Charset = "UTF-8";
            response.AppendHeader("Content-Disposition", "attachment;filename=工单列表_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentType = "application/vnd.ms-excel";

            System.IO.StringWriter sw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);

            // 构建HTML表格
            sw.WriteLine("<meta http-equiv='Content-Type' content='text/html;charset=UTF-8'>");
            sw.WriteLine("<table border='1'>");

            // 表头
            sw.WriteLine("<tr>");
            foreach (DataColumn col in dt.Columns)
            {
                sw.WriteLine("<td style='background-color:#f0f0f0;font-weight:bold;'>" + col.ColumnName + "</td>");
            }
            sw.WriteLine("</tr>");

            // 数据
            foreach (DataRow row in dt.Rows)
            {
                sw.WriteLine("<tr>");
                foreach (DataColumn col in dt.Columns)
                {
                    sw.WriteLine("<td>" + row[col].ToString() + "</td>");
                }
                sw.WriteLine("</tr>");
            }

            sw.WriteLine("</table>");

            response.Write(sw.ToString());
            response.Flush();
            response.End();
        }
    }
}