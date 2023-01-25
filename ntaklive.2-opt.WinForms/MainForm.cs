using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ntaklive._2_opt.WinForms.Services;
using ntaklive.GeneticAlgorithm.WinForms.Services;

// ReSharper disable LocalizableElement

namespace ntaklive._2_opt.WinForms;

public partial class MainForm : Form
{
    public static readonly object Lock = new();

    public MainForm()
    {
        InitializeComponent();
    }

    private void worksheetDropDown_DropDown(object sender, EventArgs e)
    {
        IWorkbookService workbookService = new WorkbookService();

        ICollection<WorksheetComboItem> worksheets = workbookService.GetWorksheetNamesFromActiveExcelWorkbook()
            .Select((x, i) => new WorksheetComboItem {Id = i, Text = x}).ToArray();

        var dropDown = (sender as ComboBox)!;
        dropDown.DataSource = worksheets;
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            App.LoadState();

            matrixSizeTextBox.Text = App.MatrixSize.ToString();
            generationsNumberTextBox.Text = App.Iterations.ToString();
            matrixPointXValueTextBox.Text = App.MatrixX.ToString();
            matrixPointYValueTextBox.Text = App.MatrixY.ToString();

            LogInformation("Предыдущие настройки были восстановлены");
        }
        catch
        {
            LogInformation("Не удалось восстановить предыдущие настройки");
            // ignore
        }

        worksheetDropDown_DropDown(worksheetComboBox, EventArgs.Empty);
    }

    private void startGAButton_Click(object sender, EventArgs e)
    {
        var thread = new Thread((startButton) =>
        {
            var button = (startButton as Button)!;

            try
            {
                button.Enabled = false;

                ClearLog();

                IMatrixParser matrixParser = new MatrixParser();

                LogInformation("Загрузка расстояний...");

                App.Distances = matrixParser.ParseMatrixFromActiveExcelWorkbook(App.SelectedWorksheet,
                    new Point(App.MatrixX, App.MatrixY), App.MatrixSize);

                var stopwatch = Stopwatch.StartNew();
                
                var tourList = new ConcurrentBag<TwoOpt.Tour>();

                var threads = new ConcurrentDictionary<Thread, int>();

                var semaphoreSlim = new SemaphoreSlim(Environment.ProcessorCount - 1);

                for (var index = 0; index < App.Iterations; index++)
                {
                    semaphoreSlim.Wait();

                    var thread = new Thread(o =>
                    {
                        (ConcurrentBag<TwoOpt.Tour>? bag, SemaphoreSlim semaphore, int i,
                                ConcurrentDictionary<Thread, int> threadList, Thread currentThread) =
                            o as (ConcurrentBag<TwoOpt.Tour>, SemaphoreSlim, int, ConcurrentDictionary<Thread, int>, Thread)
                                ? ??
                            (null, null, 0, null, null)!;

                        var sw = Stopwatch.StartNew();

                        IEnumerable<TwoOpt.Tour> tours = TwoOpt.Find(App.MatrixSize);
                        TwoOpt.Tour bestTour = tours.MinBy(x => x.Cost());

                        bag.Add(bestTour);

                        threadList.TryRemove(currentThread, out _);

                        sw.Stop();
                        LogInformation($"{i}: {sw.ElapsedMilliseconds}ms");

                        semaphore.Release();
                    }) {IsBackground = true, Priority = ThreadPriority.Highest};

                    threads.TryAdd(thread, 0);
                    thread.Start(
                        new ValueTuple<ConcurrentBag<TwoOpt.Tour>, SemaphoreSlim, int, ConcurrentDictionary<Thread, int>,
                            Thread>(tourList, semaphoreSlim, index, threads, thread));

                    Thread.Sleep(App.MatrixSize >= 100 ? 40 : 0);
                }

                while (!threads.IsEmpty)
                {
                    Thread.Sleep(1000);
                }

                TwoOpt.Tour bestTour = tourList.MinBy(x => x.Cost());

                LogInformation($"Затрачено времени (мс): {stopwatch.ElapsedMilliseconds}");
                LogInformation($"Лучший результат: {bestTour.Cost()}");
                LogInformation($"Визуализация: {bestTour}");
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            finally
            {
                button.Enabled = true;
            }
        }) {Priority = ThreadPriority.Highest};


        thread.Start(startGAButton);
    }

    private void generationsNumberTextBox_TextChanged(object sender, EventArgs e)
    {
        var control = (sender as TextBox)!;

        if (int.TryParse(control.Text, out int value))
        {
            App.Iterations = value;
        }
        else
        {
            resultValueRichTextBox.Text = "Неверно указано количество поколений";
        }
    }

    private void worksheetComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        var control = (sender as ComboBox)!;

        App.SelectedWorksheet = (control.DataSource as ICollection<WorksheetComboItem>)!.First(x => x.Id ==
            (control.SelectedItem as WorksheetComboItem)!.Id).Text;
    }

    private void matrixSizeTextBox_TextChanged(object sender, EventArgs e)
    {
        var control = (sender as TextBox)!;

        if (int.TryParse(control.Text, out int value))
        {
            App.MatrixSize = value;
        }
        else
        {
            resultValueRichTextBox.Text = "Неверно указан размер матрицы";
        }
    }

    private void matrixPointXValueTextBox_TextChanged(object sender, EventArgs e)
    {
        var control = (sender as TextBox)!;

        if (int.TryParse(control.Text, out int value))
        {
            App.MatrixX = value;
        }
        else
        {
            resultValueRichTextBox.Text = "Неверно указана точка X матрицы";
        }
    }

    private void matrixPointYValueTextBox_TextChanged(object sender, EventArgs e)
    {
        var control = (sender as TextBox)!;

        if (int.TryParse(control.Text, out int value))
        {
            App.MatrixY = value;
        }
        else
        {
            resultValueRichTextBox.Text = "Неверно указана точка Y матрицы";
        }
    }

    private void LogInformation(string message)
    {
        lock (Lock)
        {
            resultValueRichTextBox.Text =
                resultValueRichTextBox.Text += $"({DateTime.Now.ToLongTimeString()}) [INF]: {message}\n";
        }
    }

    private void LogException(Exception exception)
    {
        lock (Lock)
        {
            resultValueRichTextBox.Text = resultValueRichTextBox.Text +=
                $"({DateTime.Now.ToLongTimeString()}) [ERR]: {exception}\n";
        }
    }

    private void ClearLog()
    {
        resultValueRichTextBox.Text = string.Empty;
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        App.SaveState();
    }

    private class WorksheetComboItem
    {
        public int Id { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}