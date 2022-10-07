using System;
using System.Diagnostics;
using System.Numerics;
using TalV.ECCBackdoor.ECMath;

namespace TalV.ECCBackdoor.RNG
{
    /// <summary>
    /// Implementation of Dual EC DRBG
    /// based on https://drive.google.com/file/d/15Hd-Gdgxmpuf1Oq-HDd0X7800M2igSYX/view?usp=sharing 
    /// </summary>
    public class ECRng
    {

        /// <summary>
        /// How many bytes to trim at the end.
        /// In the real algorithm 2 bytes are trimmed, but for testing and debugging
        /// purposes this is set to 1 byte
        /// </summary>
        public const int TrimmedBytes = 1;


        /// <summary>
        /// Represents the current state of the RNG
        /// </summary>
        private BigInteger _s;
        private readonly EllipticCurve _curve;
        private readonly BigPoint _p;
        private readonly BigPoint _q;
        public readonly int OutputSize;

        public ECRng(ECRngParams rngParams, BigInteger seed)
        {
            _s = seed;
            _curve = rngParams.Curve;
            int fieldSizeBytes = _curve.FieldSize / 8;
            OutputSize = fieldSizeBytes - TrimmedBytes /*cut off 16 bits*/;
            _p = rngParams.P;
            _q = rngParams.Q;
        }

        public ECRng(ECRngParams rngParams) : this(rngParams, Environment.TickCount)
        {

        }

        public byte[] Next()
        {
            BigPoint sp = _curve.Multiply(_p, _s);
            BigInteger r = sp.X;
            BigPoint rp = _curve.Multiply(_p, r);
            _s = rp.X;
            BigPoint rq = _curve.Multiply(_q, r);
            BigInteger output = rq.X;
            if (output < 0)
            {
                Debugger.Break();
            }
            byte[] bytes = output.ToBytes();



            // trim the final 16 bits
            byte[] trimmedOutput = new byte[OutputSize];
            Array.Copy(bytes, trimmedOutput, trimmedOutput.Length);
            return trimmedOutput;
        }

        public void Next(byte[] output)
        {
            int totalToFill = output.Length;
            int filled = 0;
            int currentIndex = 0;
            while (filled < totalToFill)
            {
                byte[] random = Next();
                int toFill = Math.Min(random.Length, totalToFill - currentIndex);
                filled += toFill;

                Array.Copy(random, 0, output, currentIndex, toFill);
                currentIndex += toFill;
            }
        }
    }
}
