// 半导体封装MES系统 - 数据导出工具类
// 作者：[孟斯辰]
// 日期：2026-04-06
// 说明：用于将DataGridView数据导出到Excel文件

using System;
using System.Windows.Forms;
using OfficeOpenXml;

namespace MES.Common
{
    /// <summary>
    /// 数据导出工具类
    /// 用于半导体封装MES系统的数据导出功能
    /// </summary>
    public static class DataGridViewExport
    {
        /// <summary>
        /// 导出DataGridView数据到Excel文件
        /// 支持工单、生产记录、质量数据等导出
        /// </summary>
        /// <param name="dgv">要导出的DataGridView控件</param>
        public static void Export(DataGridView dgv)
        {
            // 检查是否有数据
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("没有数据可导出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 打开保存文件对话框
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
            sfd.FileName = "MES导出_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            sfd.Title = "导出到Excel";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                // 创建Excel包
                using (ExcelPackage package = new ExcelPackage())
                {
                    // 添加工作表
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("数据导出");

                    // 设置表头
                    int headerCol = 1;
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        if (dgv.Columns[i].Visible)
                        {
                            // 设置表头文本
                            worksheet.Cells[1, headerCol].Value = dgv.Columns[i].HeaderText;
                            // 设置表头样式
                            worksheet.Cells[1, headerCol].Style.Font.Bold = true;
                            worksheet.Cells[1, headerCol].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[1, headerCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                            headerCol++;
                        }
                    }

                    // 填充数据
                    int rowIndex = 2;
                    for (int i = 0; i < dgv.Rows.Count; i++)
                    {
                        // 跳过新行
                        if (dgv.Rows[i].IsNewRow) continue;
                        
                        int colIndex = 1;
                        for (int j = 0; j < dgv.Columns.Count; j++)
                        {
                            if (dgv.Columns[j].Visible)
                            {
                                var cellValue = dgv.Rows[i].Cells[j].Value;
                                if (cellValue != null)
                                {
                                    // 根据数据类型设置单元格值和格式
                                    if (cellValue is DateTime dateValue)
                                    {
                                        worksheet.Cells[rowIndex, colIndex].Value = dateValue;
                                        worksheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                                    }
                                    else if (cellValue is decimal || cellValue is double || cellValue is int)
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

                // 导出成功提示
                MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // 导出失败提示
                MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // 历史记录：
    // 2026-04-06：初始版本
    // 后续计划：添加导出模板、多工作表导出等功能
}