using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using Microsoft.Office.Interop.Excel;
using ntaklive.GeneticAlgorithm.WinForms.Services;

namespace ntaklive._2_opt.WinForms.Services;

public class WorkbookService : IWorkbookService
{
    public IReadOnlyCollection<string> GetWorksheetNamesFromActiveExcelWorkbook()
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

        return GetWorksheetNamesFromActiveExcelWorkbookInternal(workbookFilename);
    }
    
    private static IReadOnlyCollection<string> GetWorksheetNamesFromActiveExcelWorkbookInternal(string workbookFilepath)
    {
        var tempFilepath = $"{Path.Combine(Path.GetDirectoryName(workbookFilepath)!, Path.GetFileNameWithoutExtension(workbookFilepath))}_temp{Path.GetExtension(workbookFilepath)}";
        
        try
        {
            File.Copy(workbookFilepath, tempFilepath, true);
        
            using var workbook = new XLWorkbook(tempFilepath);
            return workbook.Worksheets.Select(x => x.Name).ToArray();
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
    
    protected static void ReleaseObject(ref object? obj)
    {
        if (obj != null && Marshal.IsComObject(obj))
        {
            Marshal.ReleaseComObject(obj);
        }

        obj = null;
    }
}