using System;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

namespace MES.Common
{
    public static class DataGridViewExport
    {
        public static void Export(DataGridView dgv)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel|*.xlsx";
            sfd.FileName = "MES导出_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            Application app = new Application();
            Workbook wb = app.Workbooks.Add();
            Worksheet ws = (Worksheet)wb.Worksheets[1];

            // 表头
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                ws.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
            }

            // 数据
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    ws.Cells[i + 2, j + 1] = dgv.Rows[i].Cells[j].Value?.ToString() ?? "";
                }
            }

            ws.Columns.AutoFit();
            wb.SaveAs(sfd.FileName);
            wb.Close();
            app.Quit();
        }
    }
}