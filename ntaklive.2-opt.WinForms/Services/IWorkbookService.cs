using System.Collections.Generic;

namespace ntaklive.GeneticAlgorithm.WinForms.Services;

public interface IWorkbookService
{
    public IReadOnlyCollection<string> GetWorksheetNamesFromActiveExcelWorkbook();
}