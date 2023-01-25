using System.Collections.Generic;
using System.Drawing;

namespace ntaklive.GeneticAlgorithm.WinForms.Services;

public interface IMatrixParser
{
    public Dictionary<(int y, int x), int> ParseMatrixFromActiveExcelWorkbook(string worksheetName,
        Point matrixLeftUpperCornerPoint, int matrixSize);
}