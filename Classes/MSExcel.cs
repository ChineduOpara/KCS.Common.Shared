using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace KCS.Common.Shared
{
    public static class MSExcel
    {
        public static ExcelWorksheet GetWorksheet(string sheetName, DataTable table)
        {
            var package = GetPackage(sheetName, table, true);
            return package.Workbook.Worksheets[sheetName];
        }

        public static byte[] GetBytes(this ExcelWorksheet worksheet)
        {
            using(ExcelPackage package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add(worksheet.Name, worksheet);
                return package.GetAsByteArray();
            }
        }

        public static ExcelPackage GetPackage(string sheetName, DataTable table, bool includeHeaders = true/*, OfficeProperties properties = null*/)
        {
            ExcelPackage package = new ExcelPackage();
            {
                int startRow = 0;
                var ws = package.Workbook.Worksheets.Add(sheetName);

                //#region Set Properties if provided
                //if (properties != null)
                //{
                //    package.Workbook.Properties.Author = properties.Author;
                //    package.Workbook.Properties.Category = properties.Category;
                //    package.Workbook.Properties.Comments = properties.Comments;
                //    package.Workbook.Properties.Company = properties.Company;
                //    package.Workbook.Properties.Keywords = properties.Keywords;
                //    package.Workbook.Properties.Manager = properties.Manager;
                //    package.Workbook.Properties.Status = properties.Status;
                //    package.Workbook.Properties.Subject = properties.Subject;
                //    package.Workbook.Properties.Title = properties.Title;
                //}
                //#endregion

                if (table.Rows.Count == 0)
                {
                    return package;
                }

                // Add the headers first.
                if (includeHeaders)
                {
                    DataRow dr = table.Rows[0];
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        var data = dr[col];
                        ws.Cells[1, col + 1].Value = data;
                    }
                    startRow = 1;
                }

                for (int row = startRow; row < table.Rows.Count; row++)
                {
                    DataRow dr = table.Rows[row];
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        var data = dr[col];
                        ws.Cells[row+1, col+1].Value = data;
                    }
                }

                return package;
            }
        }
    }
}
