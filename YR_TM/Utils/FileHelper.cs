using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Threading.Tasks;
using System.Reflection;
using ExcelDataReader;

namespace YR_TM.Utils
{
    /// <summary>
    /// 通用读写工具类，支持JSON、Excel（.xlsx）
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 读取json文件,文件可自动反序化为指定类型
        /// </summary>
        public static T ReadJson<T>(string path)
        {
            if(!File.Exists(path))
                throw new FileNotFoundException("文件不存在：" + path);

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 写json文件
        /// </summary>
        public static void WriteJson(string path, object data, bool indented = true)
        {
            var json = JsonConvert.SerializeObject(data, indented ? Formatting.Indented : Formatting.None);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// 读 Excel 文件，返回DataTable
        /// </summary>
        public static DataTable ReadExcel(string path, int sheetIndex = 0)
        {
            var package = new ExcelPackage(new FileInfo(path));
            var worksheet = package.Workbook.Worksheets[sheetIndex];
            var dt = new DataTable(worksheet.Name);

            int colCount = worksheet.Dimension.End.Column;
            int rowCount = worksheet.Dimension.Start.Row;

            for (int col = 1; col <= colCount; col++)
                dt.Columns.Add(worksheet.Cells[1, col].Text);

            for (int row = 0; row <= rowCount; row++)
            {
                var dr = dt.NewRow();
                for (int col = 1; col <= colCount; col++)
                    dr[col - 1] = worksheet.Cells[row, col].Text;
                dt.Rows.Add(dr);                
            }
            return dt;
        }

        ///<summary>
        ///将DataTable写入 Excel 文件
        /// </summary>
        public static void WriteExcel(string path, DataTable table, string sheetName = "Sheet1")
        {
            var package = new ExcelPackage();
            var workSheet = package.Workbook.Worksheets[sheetName];

            //写入列名
            for (int i = 0; i < table.Columns.Count; i++)
            {
                workSheet.Cells[1, i + 1].Value = table.Columns[i].ColumnName;
            }

            // 写入行数据
            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    workSheet.Cells[row + 2, col + 1].Value = table.Rows[row][col];
                }
            }
            package.SaveAs(new FileInfo(path));
        }

        ///<summary>
        ///自动判断类型写入文件
        /// </summary>
        public static void WriteFile(string path, object data)
        {
            string ext = Path.GetExtension(path).ToLower();
            switch (ext)
            {
                case ".json": WriteJson(path, data); break;
                case ".xlsx":
                    if (data is DataTable dt)
                        WriteExcel(path, dt);
                    else
                        throw new InvalidCastException("写入Excel文件的数据类型必须是 DataTable");
                    break;
                default:
                    throw new NotSupportedException("不支持的文件类型：" + ext);
            }
        }

        public static IOConfig ReadIOExcel(string path)
        {
            IOConfig config = new IOConfig();

            byte[] header = new byte[4];
            if (!File.Exists(path))
            {
                return new IOConfig();
            }
            else
            {
                using (var fs = File.OpenRead(path))
                {
                    fs.Read(header, 0, 4);
                }
                Console.WriteLine(BitConverter.ToString(header));

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateOpenXmlReader(stream))
                    {
                        var ds = reader.AsDataSet();
                        var table = ds.Tables[0];

                        for (int i = 2; i < table.Rows.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(table.Rows[i][2]?.ToString()))
                            {
                                config.Inputs.Add(new IOPoint
                                {
                                    Address = table.Rows[i][2].ToString(),
                                    Name = table.Rows[i][3]?.ToString(),
                                    Description = table.Rows[i][5]?.ToString()
                                });
                            }

                            if (!string.IsNullOrEmpty(table.Rows[i][9]?.ToString()))
                            {
                                config.Outputs.Add(new IOPoint
                                {
                                    Address = table.Rows[i][9].ToString(),
                                    Name = table.Rows[i][10]?.ToString(),
                                    Description = table.Rows[i][12]?.ToString()
                                });
                            }
                        }
                    }
                }
                return config;
            }
        }

        #region Excel <--> 对象列表
        ///<summary>
        ///读取Excel文明并转为对象列表
        /// </summary>
        public static List<T> ReadExcelToList<T>(string path, int sheetIndex = 0) where T : new()
        {
            DataTable dt = ReadExcel(path, sheetIndex);
            var list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in dt.Rows)
            {
                var obj = new T();
                foreach (var prop in props)
                {
                    string colName = dt.Columns.Cast<DataColumn>().FirstOrDefault(c => string.Equals(c.ColumnName, prop.Name, StringComparison.OrdinalIgnoreCase))?.ColumnName;

                    if (colName != null && row[colName] != DBNull.Value)
                    {
                        try
                        {
                            object value = Convert.ChangeType(row[colName], prop.PropertyType);
                            prop.SetValue(obj, value);
                        }
                        catch { }
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        ///<summary>
        ///将对象列表写入 Excel文件
        /// </summary>
        public static void WriteListToExcel<T>(string path, List<T> list, string sheetName = "Sheet1")
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("列表为空，无法写入 Excel");

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            //写表头
            //写入列名
            for (int i = 0; i < props.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = props[i].Name;
            }

            // 写入行数据
            for (int row = 0; row < list.Count; row++)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = props[col].GetValue(list[row]);
                }
            }
            package.SaveAs(new FileInfo(path));
        }

        #endregion
    }
}
