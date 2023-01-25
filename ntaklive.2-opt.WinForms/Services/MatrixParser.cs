using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using Microsoft.Office.Interop.Excel;
using Point = System.Drawing.Point;

namespace ntaklive.GeneticAlgorithm.WinForms.Services;

public class MatrixParser : IMatrixParser
{
    public Dictionary<(int y, int x), int> ParseMatrixFromActiveExcelWorkbook(string worksheetName, Point matrixLeftUpperCornerPoint, int matrixSize)
    {
        object? excelCom = Marshal.GetActiveObject("Excel.Application");
        if (excelCom == null)
        {
            throw new InvalidOperationException("Unable to connect with an active Excel application");
        }
        
        object? application = excelCom as Application;
        object? activeWorkbook = (application as Application)!.ActiveWorkbook;

        string workbookFilename = (activeWorkbook as Workbook)!.FullName;

        ReleaseObject(ref application);
        ReleaseObject(ref activeWorkbook);
        
        return ParseMatrixFromActiveExcelWorksheetInternal(workbookFilename, worksheetName, matrixLeftUpperCornerPoint, matrixSize);
    }
    
    private static Dictionary<(int y, int x), int> ParseMatrixFromActiveExcelWorksheetInternal(string workbookFilepath, string worksheetName, Point matrixLeftUpperCornerPoint, int matrixSize)
    {
        var tempFilepath = $"{Path.Combine(Path.GetDirectoryName(workbookFilepath)!, Path.GetFileNameWithoutExtension(workbookFilepath))}_temp{Path.GetExtension(workbookFilepath)}";
        
        try
        {
            File.Copy(workbookFilepath, tempFilepath, true);
        
            using var workbook = new XLWorkbook(tempFilepath);
            IXLWorksheet worksheet = workbook.Worksheets.First(x => x.Name == worksheetName);

            var dictionary = new Dictionary<(int y, int x), int>();
            
            for (var column = 0; column < matrixSize; column++)
            {
                IReadOnlyList<int> lengthsColumn = ParseColumn<int>(worksheet, matrixLeftUpperCornerPoint.X-1 + column, matrixLeftUpperCornerPoint.Y-1);

                for (int row = 0; row < lengthsColumn.Count; row++)
                {
                    dictionary.Add(new ValueTuple<int, int>(row, column), lengthsColumn[row]);
                }
            }

            return dictionary;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("An unexpected error was occured while parsing", exception);
        }
        finally
        {
            var tempFile = new FileInfo(tempFilepath);
            if (tempFile.Exists)
            {
                tempFile.Delete();
            }
        }
    }
    
    /// <summary>
    /// Parse the values of the specified column
    /// </summary>
    /// <param name="worksheet">Excel worksheet</param>
    /// <param name="columnOffset">Column id offset</param>
    /// <param name="rowOffset">Row id offset</param>
    /// <typeparam name="T">Type of the column value</typeparam>
    /// <returns>The list of values of the parsed column </returns>
    /// <exception cref="InvalidOperationException">It's unable to read a value of the specified column</exception>
    private static IReadOnlyList<T> ParseColumn<T>(IXLWorksheet worksheet, int columnOffset, int rowOffset)
    {
        IXLRangeRows visibleRows = worksheet.RangeUsed().Rows(row => !row.WorksheetRow().IsHidden);

        var values = new List<T>();
        foreach (IXLRangeRow row in visibleRows.TakeLast(visibleRows.Count() - rowOffset))
        {
            IXLCell? cell = row.Cell(columnOffset);
            
            if (cell == null)
            {
                continue;
            }

            cell.TryGetValue(out T value);

            values.Add(value);
        }

        return values.AsReadOnly();
    }
    
    protected static void ReleaseObject(ref object? obj)
    {
        if (obj != null && Marshal.IsComObject(obj))
        {
            Marshal.ReleaseComObject(obj);
        }

        obj = null;
    }
}