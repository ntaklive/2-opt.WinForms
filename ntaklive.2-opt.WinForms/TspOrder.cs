using System.Collections.Generic;

namespace ntaklive._2_opt.WinForms;

public class TspOrder : IEqualityComparer<TspOrder>
{
    public TspOrder(int id)
    {
        Id = id;
    }

    public int Id { get; }

    public bool Equals(TspOrder x, TspOrder y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Id == y.Id;
    }

    public int GetHashCode(TspOrder obj)
    {
        return obj.Id;
    }
}