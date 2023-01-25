using System;
using System.Threading;

namespace ntaklive._2_opt.WinForms;

public class BasicRandomization : RandomizationBase
{
    private static readonly Random GlobalRandom = new Random();
    private static readonly object GlobalLock = new object();
    private static int? _seed;
    private static ThreadLocal<Random> _threadRandom = new ThreadLocal<Random>(new Func<Random>(BasicRandomization.NewRandom));

    private static Random NewRandom()
    {
        lock (BasicRandomization.GlobalLock)
            return new Random(BasicRandomization._seed ?? BasicRandomization.GlobalRandom.Next());
    }

    private static Random Instance => BasicRandomization._threadRandom.Value;

    public static void ResetSeed(int? seed)
    {
        BasicRandomization._seed = seed;
        BasicRandomization._threadRandom = new ThreadLocal<Random>(new Func<Random>(BasicRandomization.NewRandom));
    }

    public override int GetInt(int min, int max) => BasicRandomization.Instance.Next(min, max);

    public override float GetFloat() => (float) BasicRandomization.Instance.NextDouble();

    public override double GetDouble() => BasicRandomization.Instance.NextDouble();
}