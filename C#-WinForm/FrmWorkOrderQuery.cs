// 半导体封装MES系统 - 工单查询窗体
// 作者：[孟斯辰]
// 日期：2026-04-06
// 说明：用于查询和管理半导体封装工单

using System;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using MES.Common;

namespace MES.UI
{
    /// <summary>
    /// 工单查询窗体
    /// 用于半导体封装MES系统的工单管理
    /// </summary>
    public partial class FrmWorkOrderQuery : Form
    {
        // 分页相关变量
        private int currentPage = 1;
        private int pageSize = 20;
        private int totalRows = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public FrmWorkOrderQuery()
        {
            InitializeComponent();
            InitializeUI();
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        private void InitializeUI()
        {
            // 初始化日期选择器，默认查询最近30天数据
            dtpStartDate.Value = DateTime.Now.AddDays(-30);
            dtpEndDate.Value = DateTime.Now;

            // 初始化状态下拉框
            cboStatus.Items.Add("全部");
            cboStatus.Items.Add("RELEASE");  // 释放状态
            cboStatus.Items.Add("RUN");       // 运行状态
            cboStatus.Items.Add("COMPLETE");  // 完成状态
            cboStatus.SelectedIndex = 0;

            // 初始化分页控件
            txtPageSize.Text = pageSize.ToString();
            lblPageInfo.Text = "第 1 页，共 0 页";

            // 设置DataGridView列标题
            dgvWorkOrder.Columns["work_order_no"].HeaderText = "工单号";
            dgvWorkOrder.Columns["part_no"].HeaderText = "物料号";
            dgvWorkOrder.Columns["qty_plan"].HeaderText = "计划数量";
            dgvWorkOrder.Columns["qty_complete"].HeaderText = "已完成数量";
            dgvWorkOrder.Columns["status"].HeaderText = "状态";
            dgvWorkOrder.Columns["line_code"].HeaderText = "产线";
            dgvWorkOrder.Columns["create_time"].HeaderText = "创建时间";
        }

        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        private async void btnQuery_Click(object sender, EventArgs e)
        {
            // 重置为第一页
            currentPage = 1;
            // 异步绑定数据
            await BindWorkOrderListAsync();
        }

        /// <summary>
        /// 异步绑定工单列表
        /// </summary>
        private async Task BindWorkOrderListAsync()
        {
            try
            {
                // 获取查询条件
                string woNo = txtWorkOrderNo.Text.Trim();       // 工单号
                string partNo = txtPartNo.Text.Trim();         // 物料号
                string lineCode = txtLineCode.Text.Trim();     // 产线代码
                string status = cboStatus.SelectedItem.ToString();  // 状态
                DateTime startDate = dtpStartDate.Value;       // 开始日期
                DateTime endDate = dtpEndDate.Value;           // 结束日期

                // 构建查询条件
                string whereClause = " WHERE 1=1";
                if (!string.IsNullOrEmpty(woNo))
                    whereClause += " AND work_order_no LIKE :work_order_no";
                if (!string.IsNullOrEmpty(partNo))
                    whereClause += " AND part_no LIKE :part_no";
                if (!string.IsNullOrEmpty(lineCode))
                    whereClause += " AND line_code = :line_code";
                if (status != "全部")
                    whereClause += " AND status = :status";
                whereClause += " AND create_time BETWEEN :start_date AND :end_date";

                // 计算总数
                string countSql = "SELECT COUNT(*) FROM mes_work_order" + whereClause;
                OracleParameter[] countParam = GetParameters(woNo, partNo, lineCode, status, startDate, endDate);
                DataTable countDt = OracleHelper.Query(countSql, countParam);
                totalRows = Convert.ToInt32(countDt.Rows[0][0]);

                // 计算总页数
                int totalPages = (totalRows + pageSize - 1) / pageSize;
                lblPageInfo.Text = $"第 {currentPage} 页，共 {totalPages} 页";

                // 构建分页查询
                string sql = $@"
                    SELECT * FROM (
                        SELECT ROW_NUMBER() OVER (ORDER BY create_time DESC) AS rownum,
                               work_order_no, part_no, qty_plan, qty_complete,
                               status, line_code, create_time
                        FROM mes_work_order
                        {whereClause}
                    ) WHERE rownum BETWEEN :start_row AND :end_row";

                OracleParameter[] param = GetParameters(woNo, partNo, lineCode, status, startDate, endDate);
                Array.Resize(ref param, param.Length + 2);
                param[param.Length - 2] = new OracleParameter(":start_row", (currentPage - 1) * pageSize + 1);
                param[param.Length - 1] = new OracleParameter(":end_row", currentPage * pageSize);

                // 异步查询数据
                DataTable dt = await OracleHelper.QueryAsync(sql, param);
                dgvWorkOrder.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取SQL参数
        /// </summary>
        private OracleParameter[] GetParameters(string woNo, string partNo, string lineCode, string status, DateTime startDate, DateTime endDate)
        {
            var parameters = new System.Collections.Generic.List<OracleParameter>();

            if (!string.IsNullOrEmpty(woNo))
                parameters.Add(new OracleParameter(":work_order_no", "%" + woNo + "%"));
            if (!string.IsNullOrEmpty(partNo))
                parameters.Add(new OracleParameter(":part_no", "%" + partNo + "%"));
            if (!string.IsNullOrEmpty(lineCode))
                parameters.Add(new OracleParameter(":line_code", lineCode));
            if (status != "全部")
                parameters.Add(new OracleParameter(":status", status));
            parameters.Add(new OracleParameter(":start_date", startDate));
            parameters.Add(new OracleParameter(":end_date", endDate.AddDays(1).AddSeconds(-1)));

            return parameters.ToArray();
        }

        /// <summary>
        /// 导出按钮点击事件
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvWorkOrder.DataSource == null || dgvWorkOrder.Rows.Count == 0)
            {
                MessageBox.Show("无数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                DataGridViewExport.Export(dgvWorkOrder);
                MessageBox.Show("导出完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 第一页按钮点击事件
        /// </summary>
        private void btnFirstPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage = 1;
                BindWorkOrderListAsync();
            }
        }

        /// <summary>
        /// 上一页按钮点击事件
        /// </summary>
        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                BindWorkOrderListAsync();
            }
        }

        /// <summary>
        /// 下一页按钮点击事件
        /// </summary>
        private void btnNextPage_Click(object sender, EventArgs e)
        {
            int totalPages = (totalRows + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage++;
                BindWorkOrderListAsync();
            }
        }

        /// <summary>
        /// 最后一页按钮点击事件
        /// </summary>
        private void btnLastPage_Click(object sender, EventArgs e)
        {
            int totalPages = (totalRows + pageSize - 1) / pageSize;
            if (currentPage < totalPages)
            {
                currentPage = totalPages;
                BindWorkOrderListAsync();
            }
        }

        /// <summary>
        /// 页大小文本框变化事件
        /// </summary>
        private void txtPageSize_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(txtPageSize.Text, out int size) && size > 0)
            {
                pageSize = size;
                currentPage = 1;
                BindWorkOrderListAsync();
            }
        }
    }

    // 历史记录：
    // 2026-04-06：初始版本
    // 后续计划：添加工单详情查看、批量操作等功能
}