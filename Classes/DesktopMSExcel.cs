using System;
using System.Data;
using System.IO;
using System.Collections;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Windows.Forms;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Provides methods to work with the Excel object.
	/// It was cobbled together over time, so it needs cleanup.
	/// </summary>
    public class DesktopMSExcel
    {
        private Excel.Application excelApp = null;
        private Excel.Workbook excelWorkbook = null;
        private Excel.Sheets excelSheets = null;
        private Excel.Worksheet excelWorksheet = null;

        private static object _missing = System.Reflection.Missing.Value;

        private static object _visible = true;
        private static object _false = false;
        private static object _true = true;

        private bool _app_visible = false;

        private object _filename;

		/// <summary>
		/// Contains the number of worksheets.
		/// </summary>
		public int WorksheetCount
		{
			get { return excelSheets == null ? 0 : excelSheets.Count; }
		}

		public Excel.Sheets Worksheets
		{
			get { return excelSheets; }
		}

		public System.Data.DataTable GetWorksheets()
		{
			System.Data.DataTable dt = new System.Data.DataTable("Worksheets");
			dt.EnsureColumns<string>("Id", "Name");

			if (WorksheetCount > 0)
			{
				foreach (Excel.Worksheet ws in Worksheets)
				{
					DataRow dr = dt.NewRow();
					dr["Id"] = ws.Name;
					dr["Name"] = ws.Name;
					dt.Rows.Add(dr);
				}
			}

			return dt;
		}

        #region OPEN WORKBOOK VARIABLES
        private object _update_links = 0;
        private object _read_only = _true;
        private object _format = 1;
        private object _password = _missing;
        private object _write_res_password = _missing;
        private object _ignore_read_only_recommend = _true;
        private object _origin = _missing;
        private object _delimiter = _missing;
        private object _editable = _false;
        private object _notify = _false;
        private object _converter = _missing;
        private object _add_t_mru = _false;
        private object _local = _false;
        private object _corrupt_load = _false;
        #endregion

        #region CLOSE WORKBOOK VARIABLES
        private object _save_changes = _false;
        private object _route_workbook = _false;
        #endregion

		/// <summary>
		/// Constructor.
		/// </summary>
        public DesktopMSExcel()
        {
            this.StartExcel();
        }

        /// <summary>
        /// visible is a parameter, either TRUE or FALSE, of type object.
        /// </summary>
        /// <param name="visible">Visible parameter, true for visible, false for non-visible. Default is true.</param>
        public DesktopMSExcel(bool visible)
        {
            this._app_visible = visible;
            this.StartExcel();
        }

        private void StartExcel()
        {
            if (this.excelApp == null)
            {
                this.excelApp = new Excel.ApplicationClass();
            }

            // Make Excel Visible
            this.excelApp.Visible = this._app_visible;
        }

        public void SetReadonly(bool sheetReadonly)
        {
            this._read_only = sheetReadonly;
        }

        public void StopExcel()
        {
            if (this.excelApp != null)
            {
                Process[] pProcess;
                pProcess = System.Diagnostics.Process.GetProcessesByName("Excel");
                pProcess[0].Kill();
            }
        }

        /// <summary>
        /// The following function will take in a filename, and a password
        /// associated, if needed, to open the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="password"></param>
        public bool OpenFile(string fileName, string password, bool readOnly, out string errorMessage)
        {
			errorMessage = string.Empty;
            _filename = fileName;

            if (password.Length > 0)
            {
                _password = password;
            }

            try
            {
                // Open a workbook in Excel
                this.excelWorkbook = this.excelApp.Workbooks.Open(
                    fileName, _update_links, readOnly, _format, _password,
                    _write_res_password, _ignore_read_only_recommend, _origin,
                    _delimiter, _editable, _notify, _converter, _add_t_mru,
                    _local, _corrupt_load);

                //load workbook sheets
                this.SetWorksheets();
				return true;
            }
            catch (Exception e)
            {
                this.CloseFile();
                errorMessage = e.Message;
				return false;
            }
        }

		public bool OpenFile(string fileName, string password, out string errorMessage)
        {
			return OpenFile(fileName, password, true, out errorMessage);
        }

        public void CreateNewWorkSheet()
        {
            excelWorkbook = (Excel.Workbook)excelApp.Workbooks.Add(Missing.Value);
            excelWorksheet = (Excel.Worksheet)excelWorkbook.ActiveSheet;
        }

        public void CloseFile()
        {
            ReleaseComObject(this.excelWorksheet);
            ReleaseComObject(this.excelSheets);

			if (excelWorkbook != null)
			{
				excelWorkbook.Close(_save_changes, Type.Missing, Type.Missing);
				//excelWorkbook.Close(_save_changes, _filename, _route_workbook);
				ReleaseComObject(this.excelWorkbook);
			}            

            this.excelApp.Quit();
            ReleaseComObject(this.excelApp);
        }

        public void SaveFile()
        {
            excelWorkbook.Save();
        }

        public void SaveAs(string fileName)
        {
            excelApp.DisplayAlerts = false;
            excelWorkbook.SaveAs(fileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        }

        /// <summary>
        /// Sets up the internal collection of sheets in the workbook
        /// </summary>
        public void SetWorksheets()
        {
            if (this.excelWorkbook != null)
            {
                excelSheets = excelWorkbook.Worksheets;
            }
        }

        /// <summary>
        /// Returns the maximum indexes for both the rows and columns in the currently active Wroksheet.
        /// </summary>
        public void GetMaxRowAndCol(out int maxRow, out int maxCol, out string cellRange)
        {
            maxRow = -1;
            maxCol = -1;

            if (excelWorksheet == null) throw new Exception("There is no currently Activate'd Worksheet.");

            Excel.Range lastcellRange = excelWorksheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing);

            maxRow = lastcellRange.Row;
            maxCol = lastcellRange.Column;

            string r1c1Column = ColRef2ColNo(excelWorksheet, lastcellRange.Column);
            cellRange = "A1:" + r1c1Column + lastcellRange.Row.ToString();

            return;
        }

        public List<string> ReadOneRow(int row, int maxCol)
        {
            bool ignoreMe;
            return ReadOneRow(row, maxCol, out ignoreMe);
        }

        /// <summary>
        /// Returns the maximum indexes for both the rows and columns in the currently active Worksheet.
        /// </summary>
        public List<string> ReadOneRow(int row, int maxCol, out bool isRowBlank)
        {
            Range oneCell = null;

            isRowBlank = true;

            bool isOnlyZero = true;

            List<string> oneExcelRowData = new List<string>(maxCol);

            if (excelWorksheet == null) throw new Exception("There is no currently Activated Worksheet.");

            for (int col = 1; col <= maxCol; col++)
            {
                try
                {
                    oneCell = (Range)excelWorksheet.Cells[row, col];
                }
                catch
                {
                    //Probably not enough cells.
                    break;
                }

                if (oneCell != null)
                {
                    string cellValue = "";

                    try
                    {
                        cellValue = ((oneCell.Value2 == null) ? "" : oneCell.Value2.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (isRowBlank && cellValue.Length > 0)
                    {
                        isRowBlank = false;

                        if (cellValue.Length > 0 && !cellValue.Equals("0")) isOnlyZero = false;
                    }

                    oneExcelRowData.Add(cellValue);
                }
            }

            if (!isRowBlank && isOnlyZero) isRowBlank = true;

            return oneExcelRowData;
        }

        public void DeleteOneRow(int rowNumber)
        {
            string rowSpecifier = "A" + rowNumber.ToString() + ":A" + rowNumber.ToString();

            Excel.Range range = this.excelWorksheet.get_Range(rowSpecifier, Type.Missing);
            range = range.EntireRow;

            range.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);

            ReleaseComObject(range);
        }

        /// <summary>
        /// Get the collection of sheets in the workbook.
        /// </summary>
        public List<string> GetWorksheetAndColumnNames()
        {
            List<string> wsList = new List<string>(15);

            if (this.excelWorkbook != null)
            {
                SetWorksheets();

                for (int i = 1; i <= excelSheets.Count; i++)
                {
                    Excel.Worksheet wrksht = (Excel.Worksheet)excelSheets[i];

                    string oneWrkshtName = "";

                    Excel.Range lastcellRange = null;
                    int maxRow = -1;
                    int maxCol = -1;
                    try
                    {
                        lastcellRange = wrksht.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing);

                        maxRow = lastcellRange.Row;
                        maxCol = lastcellRange.Column;

                        //Limit the cell range to 100 rows.
                        //We know that this used only for querying for header row info.
                        int maxCellRangeRow = lastcellRange.Row;
                        if (maxCellRangeRow > 100) maxCellRangeRow = 100;
                        string r1c1Column = ColRef2ColNo(wrksht, lastcellRange.Column);
                        string cellRange = "A1:" + r1c1Column + lastcellRange.Row.ToString();

                        oneWrkshtName = wrksht.Name + '~' + maxRow + '~' + maxCol + "~" + cellRange;
                    }
                    catch (Exception ex)
                    {
                        lastcellRange = null;

                        oneWrkshtName = wrksht.Name + "~ [PROTECTED, cannot open for processing]";
                    }

                    wsList.Add(oneWrkshtName);
                }
            }

            return wsList;
        }

        /// <summary>
        /// Search for ATP worksheet, if found return TRUE
        /// </summary>
        /// <returns>bool</returns>
        public bool FindWorksheet(string worksheetName)
        {
            bool ATP_SHEET_FOUND = false;

            if (this.excelSheets == null) SetWorksheets();

            if (this.excelSheets != null)
            {
                // Step thru the worksheet collection and see if ATP sheet is
                // available. If found return true;
                for (int i = 1; i <= this.excelSheets.Count; i++)
                {
                    this.excelWorksheet = (Excel.Worksheet)excelSheets.get_Item((object)i);
                    if (string.Compare(this.excelWorksheet.Name, worksheetName, true) == 0)
                    {
                        this.excelWorksheet.Activate();
                        ATP_SHEET_FOUND = true;
                        return ATP_SHEET_FOUND;
                    }
                }
            }
            return ATP_SHEET_FOUND;
        }

        public bool FindWorksheet(int index)
        {
            bool ATP_SHEET_FOUND = false;

            if (this.excelSheets == null) SetWorksheets();

            if (this.excelSheets != null)
            {
                this.excelWorksheet = (Excel.Worksheet)excelSheets[index];
                this.excelWorksheet.Select(Type.Missing);

                //this.excelWorksheet = (Excel.Worksheet)excelSheets.get_Item((object)index);
                //this.excelWorksheet.Activate();

                ATP_SHEET_FOUND = true;
                return ATP_SHEET_FOUND;
            }
            return ATP_SHEET_FOUND;
        }

        /// <summary>
        /// Return content of range from the selected range in a string array
        /// </summary>
        /// <param name="range">Range parameter: Example, GetRange("A1:D10") or GetRange(ColName)</param>
        public string[] GetRange(string range)
        {
            Excel.Range workingRangeCells = excelWorksheet.get_Range(range, Type.Missing);
            //workingRangeCells.Select();
            System.Array array = (System.Array)workingRangeCells.Cells.Value2;
            string[] arrayS = this.ConvertToStringArray(array);
            return arrayS;
        }

        /// <summary>
        /// Return content of a cell using a range reference
        /// </summary>
        /// <param name="range">parameter: Example, GetCellVal("A1")</param>
        public object GetCellVal(string range)
        {
            Excel.Range workingRangeCells = excelWorksheet.get_Range(range, Type.Missing);
            return workingRangeCells.Cells.Value2;
        }

        /// <summary>
        /// Return content of a cell usign a row and column number
        /// </summary>
        /// <param name="range">parameter: Example, GetCellVal(1,1)</param>
        public object GetCellVal(int rw, int cl)
        {
            Excel.Range workingRangeCells = (Excel.Range)excelWorksheet.Cells.get_Item(rw, cl);
            return workingRangeCells.Cells.Value2;
        }

        /// <summary>
        /// Return content of the worksheet work area (data)
        /// </summary>
        /// <param name="startR1C1">parameter: Example, GetWorkAreaRange("A1")</param>
        public object GetWorkAreaRange(string startR1C1)
        {
            if (startR1C1 == string.Empty) startR1C1 = "A1";

            string r1c1Column = GetLastCell(startR1C1);
            Excel.Range workingRangeCells = excelWorksheet.get_Range(r1c1Column, Type.Missing);
            return workingRangeCells.Cells.Value2;
        }

        /// <summary>
        /// Return the last cell with content in the XL worksheet
        /// </summary>
        public string GetLastCell(string startR1C1)
        {
            Excel.Range Rng = excelWorksheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell, Type.Missing);
            string r1c1Column = ColRef2ColNo(Rng.Column);
            string lastCellRng = startR1C1 + ":" + r1c1Column + Rng.Row.ToString();
            return lastCellRng;
        }

        /// <summary>
        /// Return a reference for a numeric column in excel
        /// </summary>
        /// <param name="range">parameter: Example, ColRef2ColNo(31) -> 'AD'</param>
        public string ColRef2ColNo(int iCol)
        {
            return ColRef2ColNo(null, iCol);
        }
        private string ColRef2ColNo(Excel.Worksheet wrksht, int iCol)
        {
            if (iCol < 1 || iCol > 256)
                return "#VALUE!";

            if (wrksht == null) wrksht = excelWorksheet;

            Excel.Range workingRangeCells = (Excel.Range)wrksht.Cells.get_Item(1, iCol);
            string colRef = workingRangeCells.Cells.get_Address(true, true, Excel.XlReferenceStyle.xlA1, Type.Missing, Type.Missing);
            return colRef.Substring(0, colRef.IndexOf("$", 1)).Replace("$", string.Empty);
        }

        /// <summary>
        /// Convert System.Array into string[]
        /// </summary>
        /// <param name="values">Values from range object</param>
        /// <returns>String[]</returns>
        private string[] ConvertToStringArray(System.Array values)
        {
            string[] newArray = new string[values.Length];

            int index = 0;
            for (int i = values.GetLowerBound(0); i <= values.GetUpperBound(0); i++)
            {
                for (int j = values.GetLowerBound(1); j <= values.GetUpperBound(1); j++)
                {
                    if (values.GetValue(i, j) == null)
                    {
                        newArray[index] = "";
                    }
                    else
                    {
                        newArray[index] = (string)values.GetValue(i, j).ToString();
                    }
                    index++;
                }
            }
            return newArray;
        }

        public Dictionary<string, string> GetNamedRanges()
        {
            //return dictionary with list of excel named ranges and corresponding excel addresses

            int numOfNames = excelApp.Names.Count;
            Excel.Name xlName;

            Dictionary<string, string> myDict = new Dictionary<string, string>(numOfNames);

            for (int i = 0; i < numOfNames; i++)
            {
                xlName = excelApp.Names.Item(i + 1, Type.Missing, Type.Missing);
                myDict.Add(xlName.Name.ToString().ToLower(), xlName.RefersTo.ToString());
            }

            return myDict;
        }

        public void SetCellValue(int row, string namedRange, string dataValue, Boolean isNumeric)
        {
            Range columnRange = excelWorksheet.get_Range(namedRange, Type.Missing);
            Range cellRange = (Range)columnRange.Cells[row, Type.Missing];

            if (isNumeric)
            {
                cellRange.Value2 = dataValue;
            }
            else
            {
                cellRange.Value2 = "'" + dataValue;
            }
        }

        public void SetCellValue(int row, int col, string dataValue, Boolean isNumeric)
        {
            excelWorksheet.Cells[row, col] = dataValue;

            if (isNumeric)
            {
                excelWorksheet.Cells[row, col] = dataValue;
            }
            else
            {
                excelWorksheet.Cells[row, col] = "'" + dataValue;
            }
        }

        /// <summary>
        /// Converts a culture info to an Excel format string.   Note this pattern is modeled after the Excel Macro.
        /// </summary>
        /// <param name="ci"></param>
        /// <returns></returns>
        public string GetCultureNumberString(CultureInfo ci)
        {
            //CultureInfo ci = new CultureInfo("fr-FR", false);

            string s = "[$";
            s += ci.NumberFormat.CurrencySymbol + "]#";
            //s += ci.NumberFormat.CurrencyGroupSeparator;
            s += ",";  // hard-code for Excel as Excel doesn't use anything else
            //s += "##0" + ci.NumberFormat.CurrencyDecimalSeparator;
            s += "##0" + "."; // hard-code for Excel as Excel doesn't use anything else
            for (int i = 0; i < ci.NumberFormat.CurrencyDecimalDigits; i++) s += "0";
            s += ";[RED]" + s.Replace("]", "]-");  //[RED] for negative
            return s;
        }
        public void SetCurrencyCellValue(int startRow, int col, object[,] dataValues, CultureInfo ci)
        {
            Exception ex = null;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    int endRow = startRow + dataValues.GetUpperBound(0);

                    Range cellRange = (Range)this.excelWorksheet.get_Range(excelWorksheet.Cells[startRow, col], excelWorksheet.Cells[endRow, col]);

                    cellRange.Value2 = dataValues;
                    cellRange.NumberFormat = GetCultureNumberString(ci);
                    return;
                }
                catch (Exception ex2)
                {
                    ex = ex2;
                }
                Thread.Sleep(1000);
            }
            throw ex;
        }

        public void SetImageCellValue(int startRow, int col, object[,] dataValues)
        {
            Exception ex = null;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    int endRow = startRow + dataValues.GetUpperBound(0);

                    int k=0;
                    for (int j = startRow; j <= endRow; j++)
                    {
                        if (dataValues[k, 0] != null && dataValues[k, 0] != DBNull.Value)
                        {
                            //Range cellRange = (Range)this.excelWorksheet.get_Range(excelWorksheet.Cells[startRow, col], excelWorksheet.Cells[endRow, col]);
                            Range cellRange = (Range)this.excelWorksheet.get_Range(excelWorksheet.Cells[j, col], excelWorksheet.Cells[j, col]);
                            cellRange.Select();
                            Clipboard.SetDataObject(dataValues[k, 0]);
                            this.excelWorksheet.Paste(cellRange, Missing.Value);
                        }
                        k++;
                    }

                    //cellRange.Value2 = dataValues;
                    return;
                }
                catch (Exception ex2)
                {
                    ex = ex2;
                }
                Thread.Sleep(1000);
            }
            throw ex;
        }

        /// <summary>
        /// Set a cells column wise with values
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="col"></param>
        /// <param name="dataValues"></param>
        /// <param name="isNumeric"></param>
        /// <returns>true if MaxLenght of a column is exceeded...Note the maxlen is discovered expermimentally...so if u can extend the maxlenght, fell free to change it</returns>
        public bool SetCellValue(int startRow, int col, object[,] dataValues, Boolean isNumeric)
        {
            Exception ex = null;
            bool maxLengthExceed = false;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    int endRow = startRow + dataValues.GetUpperBound(0);

                    Range cellRange = (Range)this.excelWorksheet.get_Range(excelWorksheet.Cells[startRow, col], excelWorksheet.Cells[endRow, col]);

                    //if (isNumeric)
                    //{
                    //    cellRange.Value2 = dataValues;
                    //}
                    //else
                    {
                        //June 6, 2007 KDR change to set numberformat to text as this method allows cut/paste in Excel without the single quote appearing when pasteing
                        cellRange.NumberFormat = "@";
                        int maxLen = 911;
                        for (int j = 0; j < dataValues.GetUpperBound(0); j++)
                        {
                            if (dataValues[j, 0].ToString().Length > maxLen)
                            {
                                dataValues[j, 0] = dataValues[j, 0].ToString().Substring(0, maxLen - 3) + "...";
                                maxLengthExceed = true;
                            }
                        }
                        cellRange.Value2 = dataValues;
                    }
                    return maxLengthExceed;
                }
                catch (Exception ex2)
                {
                    ex = ex2;
                }
                Thread.Sleep(1000);
            }
            throw ex;

        }

        public void SetCellValue(int row, int col, string dataValue, Boolean isNumeric, bool bold)
        {
            Exception ex = null;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    SetCellValue(row, col, dataValue, isNumeric);
                    Range range = (Range)excelWorksheet.Cells[row, col];
                    if (bold) range.Font.Bold = true;
                    return;
                }
                catch (Exception ex2)
                {
                    ex = ex2;
                }
                Thread.Sleep(1000);
            }
            throw ex;
        }

        public string GetCellValue(int row, string namedRange)
        {
            Range columnRange = excelWorksheet.get_Range(namedRange, Type.Missing);
            Range cellRange = (Range)columnRange.Cells[row, Type.Missing];

            if (cellRange.Value2 == null) return string.Empty;
            return cellRange.Value2.ToString();

        }

        /// <summary>
        /// Reset All columns to General celltype
        /// </summary>
        public void ResetCellTypes()
        {
            excelWorksheet.Cells.Select(); // select the while grid
            excelWorksheet.Cells.NumberFormat = "General";
            // position to A1 cell to make the whole unselect the whole grid
            Excel.Range selection = excelWorksheet.get_Range("A1","A1");
            selection.Select();

        }
        public void RefreshAllPivotTables()
        { //Refresh all pivot tables in a workbook - KDR June 14, 2007

            foreach (Worksheet ws in excelWorkbook.Worksheets)
            {
                Excel.PivotTables pivotTables1
                    = (Excel.PivotTables)ws.PivotTables(Type.Missing);

                if (pivotTables1.Count > 0)
                {
                    for (int i = 0; i <= pivotTables1.Count; i++)
                    {
                        try
                        {
                            pivotTables1.Item(i).RefreshTable();
                        }
                        catch
                        {
                            //some refreshes may fail so continue to refresh the others
                        }
                    }
                }
                else
                {
                    //MessageBox.Show("This workbook contains no pivot tables.");
                }
            }
        }


        public void DupRow(int fromRow, int toRow)
        {
            Excel.Range oRange = excelWorksheet.get_Range("A" + (fromRow).ToString(), Type.Missing);
            Excel.Range oRow = oRange.EntireRow;
            excelApp.CutCopyMode = XlCutCopyMode.xlCopy;
            oRow.Select();
            Excel.Range oRangeTo = excelWorksheet.get_Range("A" + (toRow).ToString(), Type.Missing);
            Excel.Range oRowTo = oRangeTo.EntireRow;
            oRow.Select();
            oRow.Copy(Type.Missing);
            oRowTo.Insert(XlInsertShiftDirection.xlShiftDown, Type.Missing);
            if ((bool)oRange.EntireRow.Hidden == true)
            {
                oRangeTo = excelWorksheet.get_Range("A" + (toRow).ToString(), Type.Missing);
                oRangeTo.EntireRow.Hidden = false;
            }
        }

        public int SeekRow(string colName, string textToSearch, ref string errMsg)
        {
            Excel.Range rng = excelWorksheet.Cells.Find(textToSearch,
                                                        excelWorksheet.Cells[1, 1],//get_Range(colName, Type.Missing), 
                                                        Excel.XlFindLookIn.xlValues,
                                                        Excel.XlLookAt.xlPart,
                                                        Type.Missing,
                                                        XlSearchDirection.xlNext,
                                                        false,
                                                        Type.Missing,
                                                        Type.Missing);
            if (rng == null) return -1;

            string adr = rng.get_Address(true, true, XlReferenceStyle.xlR1C1, Type.Missing, Type.Missing);

            int r = adr.IndexOf("R");
            int c = adr.IndexOf("C");
            string rowstr = adr.Substring(r + 1, c - r - 1);
            return int.Parse(rowstr);
            //try
            //{
            //    bool foundData = false;
            //    for (int row = 1; ; row++)
            //    {
            //        string colData = GetCellValue(row, colName);
            //        if (colData == textToSearch)
            //        {
            //            return row;
            //        }
            //        else
            //        {
            //            if (colData.Trim().Length != 0)
            //            {
            //                foundData = true;
            //            }
            //            else
            //            {
            //                if (foundData) return -1;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    errMsg = ex.Message;
            //}
            //return -1;
        }

		public string ReadToDataTable(ref System.Data.DataTable dtWrksht, string wrkshtName)
		{
			int maxCol, maxRow;
			string cellRange;
			GetMaxRowAndCol(out maxRow, out maxCol, out cellRange);
			return ReadToDataTable(ref dtWrksht, wrkshtName, maxRow, maxCol);
		}

        public string ReadToDataTable(ref System.Data.DataTable dtWrksht, string wrkshtName, int maxRow, int maxCol)
        {
            try
            {
                if (!FindWorksheet(wrkshtName)) throw new Exception("Unable to find the worksheet '" + wrkshtName + "'.");

				if (maxRow == 0) maxRow = int.MaxValue;
				if (maxCol == 0) maxCol = int.MaxValue;
                for (int row = 1; row <= maxRow; row++)
                {
                    DataRow drNew = dtWrksht.NewRow();

                    for (int col = 1; col <= maxCol; col++)
                    {
                        drNew[col - 1] = GetCellVal(row, col);
                    }

                    dtWrksht.Rows.Add(drNew);
                }
            }
            catch (Exception ex)
            {
                return ("Unable to read the Excel worksheet '" + wrkshtName + "' into a DataTable.\n\n" + ex.Message);
            }

            return "";
        }

        public string ReadToDataTable(System.Data.DataTable dtExcel, string colPrefix, int startRow, string indicatorColName, string endText)
        {
            try
            {

                Dictionary<string, string> names = GetNamedRanges();

                for (int row = startRow; ; row++)
                {
                    string colText = GetCellValue(row, indicatorColName);
                    if (colText == endText) break;

                    DataRow dr = dtExcel.NewRow();
                    foreach (DataColumn dc in dtExcel.Columns)
                    {
                        string fldName = colPrefix + dc.ColumnName;
                        fldName = fldName.ToLower();
                        if (names.ContainsKey(fldName))
                        {
                            object v = GetCellValue(row, fldName);
                            if (v == null || v.ToString() == "")
                            {
                                dr[dc.ColumnName] = DBNull.Value;
                            }
                            else
                            {
                                dr[dc.ColumnName] = GetCellValue(row, fldName);
                            }
                        }
                    }
                    dtExcel.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string WriteDataTable(System.Data.DataView dvExcel, string colPrefix, string startRowColName, string startRowSearchText, int hiddenRowOffSet, string keyRowColName)
        {
            string msg = "";
            int startRow = SeekRow(colPrefix + startRowColName, startRowSearchText, ref msg);
            if (startRow == -1)
            {
                if (msg != "") return msg;
                return "Can't find starting row with text " + startRowSearchText;
            }

            int hiddenRow = startRow + hiddenRowOffSet;

            int row = hiddenRow + 1;

            bool hideKeyCol = false;

            Excel.Range keyCol = excelWorksheet.get_Range(colPrefix + keyRowColName, Type.Missing);
            if ((bool)(keyCol.EntireColumn.Hidden) == true)
            {
                keyCol.EntireColumn.Hidden = false;
                hideKeyCol = true;
            }

            Dictionary<string, string> names = GetNamedRanges();

            foreach (DataRowView dr in dvExcel)
            {
                string key = dr[keyRowColName].ToString();
                string keyFldName = colPrefix + keyRowColName;
                keyFldName = keyFldName.ToLower();
                if (!names.ContainsKey(keyFldName)) return "key column name " + keyFldName + " not found in Excel File.";

                string errMsg = "";
                int row2 = SeekRow(keyFldName, key, ref errMsg);
                if (row2 == -1) // key not found
                {
                    if (errMsg != "") return errMsg;
                    DupRow(hiddenRow, row);
                }
                else
                {
                    row = row2;
                }
                foreach (DataColumn dc in dvExcel.Table.Columns)
                {
                    string colName = colPrefix + dc.ColumnName.ToLower();
                    if (names.ContainsKey(colName))
                    {
                        if (dc.DataType == typeof(System.Decimal))
                        {
                            SetCellValue(row, colName, dr[dc.ColumnName].ToString(), true);
                        }
                        else
                        {
                            if (dc.DataType == typeof(System.DateTime))
                            {
                                System.DateTime dt = System.DateTime.Parse(dr[dc.ColumnName].ToString());
                                SetCellValue(row, colName, dt.ToShortDateString(), false);
                            }
                            else
                            {
                                SetCellValue(row, colName, dr[dc.ColumnName].ToString(), false);
                            }
                        }
                    }
                }
                if (row2 == -1) row++;
            }

            if (hideKeyCol) keyCol.EntireColumn.Hidden = true;

            return "";
        }

        public void AutoFit()
        {
            this.excelWorksheet.Cells.EntireColumn.AutoFit();
        }

        public void MergColumn(int row, int fromCol, int toCol, int backColor, bool bold)
        {
            Exception ex = null;
            for (int i = 0; i <= 5; i++)
            {
                try
                {
                    Range colRange = (Range)this.excelWorksheet.get_Range(excelWorksheet.Cells[row, fromCol], excelWorksheet.Cells[row, toCol]);
                    colRange.Merge(Type.Missing);
                    colRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    colRange.Interior.Color = backColor;
                    if (bold) colRange.Font.Bold = true;
                    return;
                }
                catch (Exception ex2)
                {
                    ex = ex2;
                }
                Thread.Sleep(1000);
            }
            throw ex;

        }

        private void ReleaseComObject(object o)
        {
            try
            {
                if (o != null)
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
            }

            finally
            {
                o = null;
            }
		}

		#region Taken from Metadata control
		public static System.Data.DataTable ReadFromExcelFile(ref string fileName, out string errorMessage)
		{
			errorMessage = string.Empty;
			// Copy file to avoid any problems with user having the file open
			string tempFileName = System.IO.Path.GetFileName(System.IO.Path.GetTempFileName());
			string origExtension = System.IO.Path.GetExtension(fileName);

			tempFileName = tempFileName.Replace(".tmp", origExtension);
			tempFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempFileName);

			try
			{
				System.IO.File.Copy(fileName, tempFileName, true);
				fileName = tempFileName;
				return ReadData(fileName, out errorMessage);
			}
			catch
			{
				// Cannot copy, use original file
				return ReadData(fileName, out errorMessage);
			}			
		}

		/// <summary>
		/// Removes first row from first worksheet in workbook and saves file.
		/// </summary>
		public static bool RemoveFirstRow(string fileName, out string errorMessage)
		{
			DesktopMSExcel genExcel = null;
			errorMessage = string.Empty;

			try
			{
				genExcel = new DesktopMSExcel(false);

				bool openFileStatus = genExcel.OpenFile(fileName, string.Empty, false, out errorMessage);
				if (!openFileStatus)
				{
					errorMessage = "Cannot open Excel File!  Error is: " + errorMessage;
					return false;
				}

				if (!genExcel.FindWorksheet(1))
				{
					errorMessage = "Cannot find correct worksheet.";
					return false;
				}

				genExcel.DeleteOneRow(1);    // 1 based index
				genExcel.SaveFile();
			}
			catch (Exception ex)
			{
				string msg;

				if (ex.Message.Contains("is read-only"))
					// Default message is confusing, try to give more accurate one
					msg = "Error importing from Excel: File directory may be read-only";
				else
					msg = "Error importing from Excel: " + ex.Message;

				errorMessage = msg;
				return false;
			}
			finally
			{
				if (genExcel != null) genExcel.CloseFile();
			}

			return true;
		}

		public static System.Data.DataTable ReadData(string fileName, out string errorMessage)
		{
			string firstSheetName = GetFirstWorksheetName(fileName);
			return ReadData(fileName, firstSheetName, out errorMessage);
		}

		/// <summary>
		/// Use OleDb to open an Excel file, query the data, and write to a DataTable.
		/// </summary>
		public static System.Data.DataTable ReadData(string fileName, string sheetName, out string errorMessage)
		{
			System.Data.DataTable dtExcel = null;
			errorMessage = string.Empty;

			try
			{				
				string strConn = GetExcelOLEDBConnString(fileName);
				using (System.Data.OleDb.OleDbConnection con = new System.Data.OleDb.OleDbConnection(strConn))
				{
					con.Open();

					using (System.Data.OleDb.OleDbDataAdapter daGetExcel = new System.Data.OleDb.OleDbDataAdapter("SELECT * FROM [" + sheetName + "$]", con))
					{
						dtExcel = new System.Data.DataTable();
						daGetExcel.Fill(dtExcel);
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = "Cannot open Excel File!  Error is: " + ex.Message;
				return null;
			}

			return dtExcel;
		}

		public static string GetFirstWorksheetName(string excelFileName)
		{
			Excel.Application excelApp = null;
			Excel.Workbook excelWorkbook = null;
			string firstSheetName;

			try
			{
				excelApp = new Excel.ApplicationClass();
				excelWorkbook = excelApp.Workbooks.Open(
					excelFileName,                   // Filename
					0,                               // UpdateLinks
					true,                            // ReadOnly
					5,                               // Format
					"",                              // Password
					"",                              // WriteResPassword
					true,                            // IgnoreReadOnlyRecommended
					Excel.XlPlatform.xlWindows,      // Origin
					"\t",                            // Delimiter
					false,                           // Editable
					false,                           // Notify
					0,                               // Converter
					false,                           // AddToMru
					false,                           // Local
					Excel.XlCorruptLoad.xlNormalLoad // CorruptLoad
					);

				Excel.Worksheet excelSheet = (Excel.Worksheet)excelWorkbook.Worksheets[1];    // 1 based index (ala COM/VB)

				firstSheetName = excelSheet.Name;
			}
			catch
			{
				return "Sheet1";    // If there are problems don't prevent them from doing work; return the default name for first sheet
			}
			finally
			{
				if (excelWorkbook != null)
				{
					excelWorkbook.Close(false, excelFileName, null);
					Marshal.ReleaseComObject(excelWorkbook);
				}

				if (excelApp != null)
				{
					excelApp.Quit();
					excelApp = null;
				}
			}
			return firstSheetName;
		}

		/// <summary>
		/// Gets the connection string to be used when opening an Excel file. Support XLS and XLSX files.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetExcelOLEDBConnString(string fileName)
		{
			string connString = string.Empty;
			string ext = System.IO.Path.GetExtension(fileName).ToLower();

			if (ext == ".xls")
			{
				connString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties=Excel 8.0;";
			}
			else
			{
				if (ext == ".xlsx")
				{
					connString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES\"";
				}
			}

			return connString;
		}
		#endregion

	}
}

