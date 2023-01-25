using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ntaklive._2_opt.WinForms;

public static class App
{
    public static string SelectedWorksheet { get; internal set; }
    public static int MatrixSize { get; internal set; }
    public static int MatrixX { get; internal set; }
    public static int MatrixY { get; internal set; }
    public static int Iterations { get; internal set; }

    public static Dictionary<(int y, int x), int> Distances { get; internal set; }

    public static void SaveState()
    {
        var state = new AppState()
        {
            MatrixSize = App.MatrixSize,
            MatrixX = App.MatrixX,
            MatrixY = App.MatrixY,
            Iterations = App.Iterations,
        };
        
        File.WriteAllText(Path.Combine(CurrentDirectoryPath, "state.json"), JsonSerializer.Serialize(state));
    }

    public static void LoadState()
    {
        string json = File.ReadAllText(Path.Combine(CurrentDirectoryPath, "state.json"));
        var state = JsonSerializer.Deserialize<AppState>(json);

        App.MatrixSize = state.MatrixSize;
        App.MatrixX = state.MatrixX;
        App.MatrixY = state.MatrixY;
        App.Iterations = state.Iterations;
    }
    
    public static int GetDistanceBetween(int fromId, int toId)
    {
        return Distances[new ValueTuple<int, int>(fromId, toId)];
    }
    
    internal class AppState
    {
        public int MatrixSize { get; set; }
        public int MatrixX { get; set; }
        public int MatrixY { get; set; }
        public int Iterations { get; set; }
    }
    
    public static readonly string CurrentDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    
    // // Размер матрицы
    // public const int N = 100;
    // public const int Mod = 1;
    // public const string Csv100X100Filepath = @"D:\Desktop\test100x100.csv";
    // public const string Csv10X10Filepath = @"D:\Desktop\test10x10.csv";
    // public const string Csv4X4Filepath = @"D:\Desktop\test4x4.csv";
    //
    // public static Dictionary<(int y, int x), int> Distances { get; } =
    //     LoadPathLengthDictionaryFromCsv(Csv100X100Filepath);
    //
    // public static Dictionary<(int y, int x), int> LoadPathLengthDictionaryFromCsv(string filepath)
    // {
    //     string[] data = File.ReadAllLines(filepath);
    //
    //     var dictionary = new Dictionary<(int y, int x), int>();
    //     for (var i = 0; i < data.Length; i++)
    //     {
    //         string line = data[i];
    //
    //         string[] lengths = line.Split(',');
    //
    //         for (var j = 0; j < lengths.Length; j++)
    //         {
    //             int length = int.Parse(lengths[j]);
    //
    //             dictionary.Add(new ValueTuple<int, int>(j, i), length);
    //         }
    //     }
    //
    //     return dictionary;
    // }
    //
    // public static int GetDistanceBetween(int fromId, int toId)
    // {
    //     return Distances[new ValueTuple<int, int>(toId, fromId)];
    // }
}