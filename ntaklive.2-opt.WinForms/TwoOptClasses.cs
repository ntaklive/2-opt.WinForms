using System;
using System.Collections.Generic;
using System.Linq;

namespace ntaklive._2_opt.WinForms;

public static class TwoOpt
{
    public static IEnumerable<Tour> Find(int size)
    {
        //create an initial tour out of nearest neighbors
        Stop[] stops = Enumerable.Range(0, size)
            .Shuffle(new BasicRandomization())
            // .OrderBy(x => x < firstId)
            .Select(i => new Stop(new TspOrder(i)))
            .NearestNeighbors()
            .ToArray();

        //create next pointers between them
        stops.Connect(true);

        //wrap in a tour object
        Tour startingTour = new Tour(stops);

        var tours = new List<Tour>();
        while (true)
        {
            Console.WriteLine(startingTour);
            tours.Add(startingTour);
            Tour newTour = startingTour.GenerateMutations().MinBy(tour => tour.Cost());
            if (newTour.Cost() < startingTour.Cost())
            {
                startingTour = newTour;
            }
            else
            {
                break;
            }
        }

        return tours;
    }

    public class Stop
    {
        public Stop(TspOrder tspOrder)
        {
            TspOrder = tspOrder;
        }


        public Stop Next { get; set; }

        public TspOrder TspOrder { get; set; }


        public Stop Clone()
        {
            return new Stop(TspOrder);
        }


        public static double Distance(Stop first, Stop other)
        {
            return App.GetDistanceBetween(first.TspOrder.Id, other.TspOrder.Id);
        }


        //list of nodes, including this one, that we can get to
        public IEnumerable<Stop> CanGetTo()
        {
            Stop current = this;
            while (true)
            {
                yield return current;
                current = current.Next;
                if (current == this)
                {
                    break;
                }
            }
        }


        public override bool Equals(object obj)
        {
            return TspOrder == ((Stop) obj).TspOrder;
        }


        public override int GetHashCode()
        {
            return TspOrder.GetHashCode();
        }


        public override string ToString()
        {
            return (TspOrder.Id + 1).ToString();
        }
    }


    public class Tour
    {
        public Tour(IEnumerable<Stop> stops)
        {
            Anchor = stops.First();
        }


        //the set of tours we can make with 2-opt out of this one
        public IEnumerable<Tour> GenerateMutations()
        {
            for (Stop stop = Anchor; stop.Next != Anchor; stop = stop.Next)
            {
                //skip the next one, since you can't swap with that
                Stop current = stop.Next.Next;
                while (current != Anchor)
                {
                    yield return CloneWithSwap(stop.TspOrder, current.TspOrder);
                    current = current.Next;
                }
            }
        }

        public Stop Anchor { get; set; }

        public Tour CloneWithSwap(TspOrder firstTspOrder, TspOrder secondTspOrder)
        {
            Stop firstFrom = null, secondFrom = null;
            IList<Stop> stops = UnconnectedClones();
            stops.Connect(true);

            foreach (Stop stop in stops)
            {
                if (stop.TspOrder == firstTspOrder)
                {
                    firstFrom = stop;
                }

                if (stop.TspOrder == secondTspOrder)
                {
                    secondFrom = stop;
                }
            }

            //the swap part
            Stop firstTo = firstFrom.Next;
            Stop secondTo = secondFrom.Next;

            //reverse all of the links between the swaps
            firstTo.CanGetTo()
                .TakeWhile(stop => stop != secondTo)
                .Reverse()
                .Connect(false);

            firstTo.Next = secondTo;
            firstFrom.Next = secondFrom;

            var tour = new Tour(stops);
            return tour;
        }

        public IList<Stop> UnconnectedClones()
        {
            return Cycle().Select(stop => stop.Clone()).ToList();
        }

        public double Cost()
        {
            return Cycle().Aggregate(
                0.0,
                (sum, stop) =>
                {
                    if (stop.Next != Anchor)
                    {
                        return sum + Stop.Distance(stop, stop.Next);
                    }
                    else
                    {
                        return sum + Stop.Distance(stop, stop);
                    }
                });
        }


        private IEnumerable<Stop> Cycle()
        {
            return Anchor.CanGetTo();
        }

        public override string ToString()
        {
            string path = string.Join(
                "->",
                Cycle().Select(stop => stop.ToString()).ToArray());
            return $"Cost: {Cost()}, Path:{path}";
        }
    }

    //take an ordered list of nodes and set their next properties
    private static void Connect(this IEnumerable<Stop> stops, bool loop)
    {
        Stop prev = null, first = null;
        foreach (Stop stop in stops)
        {
            if (first == null)
            {
                first = stop;
            }

            if (prev != null)
            {
                prev.Next = stop;
            }

            prev = stop;
        }

        if (loop)
        {
            prev.Next = first;
        }
    }

    //return an ordered nearest neighbor set
    private static IEnumerable<Stop> NearestNeighbors(this IEnumerable<Stop> stops)
    {
        List<Stop> stopsLeft = stops.ToList();
        for (Stop? stop = stopsLeft.FirstOrDefault(); stop != null; stop = stopsLeft.MinBy(s => Stop.Distance(stop, s)))
        {
            stopsLeft.Remove(stop);
            yield return stop;
        }
    }
}