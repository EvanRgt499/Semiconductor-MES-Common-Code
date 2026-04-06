using System;
using System.Windows.Forms;
using OfficeOpenXml;

namespace MES.Common
{
    public static class DataGridViewExport
    {
        public static void Export(DataGridView dgv)
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
            sfd.FileName = "MES导出_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            sfd.Title = "导出到Excel";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("数据导出");

                    // 设置表头
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        if (dgv.Columns[i].Visible)
                        {
                            worksheet.Cells[1, i + 1].Value = dgv.Columns[i].HeaderText;
                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }
                    }

                    // 填充数据
                    int rowIndex = 2;
                    for (int i = 0; i < dgv.Rows.Count; i++)
                    {
                        if (dgv.Rows[i].IsNewRow) continue;
                        
                        int colIndex = 1;
                        for (int j = 0; j < dgv.Columns.Count; j++)
                        {
                            if (dgv.Columns[j].Visible)
                            {
                                var cellValue = dgv.Rows[i].Cells[j].Value;
                                if (cellValue != null)
                                {
                                    if (cellValue is DateTime dateValue)
                                    {
                                        worksheet.Cells[rowIndex, colIndex].Value = dateValue;
                                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                                    }
                                    else if (cellValue is decimal decimalValue || cellValue is double doubleValue || cellValue is int intValue)
                                    {
                                        worksheet.Cells[rowIndex, colIndex].Value = cellValue;
                                    }
                                    else
                                    {
                                        worksheet.Cells[rowIndex, colIndex].Value = cellValue.ToString();
                                    }
                                }
                                colIndex++;
                            }
                        }
                        rowIndex++;
                    }

                    // 自动调整列宽
                    worksheet.Cells.AutoFitColumns();

                    // 保存文件
                    System.IO.FileInfo file = new System.IO.FileInfo(sfd.FileName);
                    package.SaveAs(file);
                }

                MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}