using System;

namespace SLAE
{
    public abstract class SLAEAlg
    {
        public double[] X;
        public string[] XStrings;
        public string[][] LStrings;
        public string[][] UStrings;

        public string Message { get; protected set; }
        public abstract bool Solve();
    }
}
