using System.Diagnostics;

namespace Common
{
    public class Tuple<T1>
    {
        public Tuple(T1 first)
        {
            First = first;
        }

        public T1 First { get; set; }
    }
    [DebuggerDisplay("({First}, {Second})")]
    public class Tuple<T1, T2> : Tuple<T1>
    {
        public Tuple(T1 first, T2 second)
            : base(first)
        {
            Item2 = second;
        }

        public T2 Item2 { get; set; }
    }

    public class Tuple<T1, T2, T3> : Tuple<T1, T2>
    {
        public Tuple(T1 first, T2 second, T3 third)
            : base(first, second)
        {
            Third = third;
        }

        public T3 Third { get; set; }
    }
}