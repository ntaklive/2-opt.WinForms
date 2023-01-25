using System;
using System.Collections.Generic;
using System.Linq;

namespace ntaklive._2_opt.WinForms;

public abstract class RandomizationBase : IRandomization
{
    public abstract int GetInt(int min, int max);

    public abstract float GetFloat();

    public abstract double GetDouble();

    public virtual int[] GetInts(int length, int min, int max)
    {
        int[] ints = new int[length];
        for (int index = 0; index < length; ++index)
            ints[index] = this.GetInt(min, max);
        return ints;
    }

    public virtual int[] GetUniqueInts(int length, int min, int max)
    {
        int count = max - min;
        List<int> intList = count >= length
            ? Enumerable.Range(min, count).ToList<int>()
            : throw new ArgumentOutOfRangeException(nameof(length),
                "The length is {0}, but the possible unique values between {1} (inclusive) and {2} (exclusive) are {3}."
                    .With((object) length, (object) min, (object) max, (object) count));
        int[] uniqueInts = new int[length];
        for (int index1 = 0; index1 < length; ++index1)
        {
            int index2 = this.GetInt(0, intList.Count);
            uniqueInts[index1] = intList[index2];
            intList.RemoveAt(index2);
        }

        return uniqueInts;
    }

    public float GetFloat(float min, float max) => min + (max - min) * this.GetFloat();

    public virtual double GetDouble(double min, double max) => min + (max - min) * this.GetDouble();
}