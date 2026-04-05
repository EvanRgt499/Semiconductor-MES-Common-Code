using System;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;
using MES.Common;

namespace MES.UI
{
    public partial class FrmWorkOrderQuery : Form
    {
        public FrmWorkOrderQuery()
        {
            InitializeComponent();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                string woNo = txtWorkOrderNo.Text.Trim();

                if (string.IsNullOrEmpty(woNo))
                {
                    MessageBox.Show("请输入工单号");
                    return;
                }

                string sql = @"
                    SELECT work_order_no, part_no, qty_plan, qty_complete,
                           status, line_code, create_time
                    FROM mes_work_order
                    WHERE work_order_no = :work_order_no";

                OracleParameter[] param = {
                    new OracleParameter(":work_order_no", woNo)
                };

                DataTable dt = OracleHelper.Query(sql, param);
                dgvWorkOrder.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("异常：" + ex.Message);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvWorkOrder.DataSource == null || dgvWorkOrder.Rows.Count == 0)
            {
                MessageBox.Show("无数据可导出");
                return;
            }

            DataGridViewExport.Export(dgvWorkOrder);
            MessageBox.Show("导出完成");
        }
    }
}