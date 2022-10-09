using System;
using System.Linq;
using System.Numerics;

namespace TalV.ECCBackdoor.ECMath
{
    public static class BigIntegerHelper
    {
        public static BigInteger FromHex(string hex)
        {
            if (hex.StartsWith("0x"))
            {
                hex = hex.Substring(2);
            }
            byte[] bytes = Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).Reverse()
                .ToArray();
            return FromBytes(bytes);
        }


        public static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        // https://rosettacode.org/wiki/Tonelli-Shanks_algorithm#C#
        /// <summary>
        /// Calculate the square root of n mod p, if one exists
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <param name="sqrt"></param>
        /// <returns>whether a square root exists</returns>
        public static bool TonelliShanksSqrt(this BigInteger n, BigInteger p, out BigInteger sqrt)
        {
            sqrt = BigInteger.Zero;
            if (BigInteger.ModPow(n, (p - 1) / 2, p) != 1)
            {
                return false;
            }

            BigInteger q = p - 1;
            BigInteger ss = 0;
            while ((q & 1) == 0)
            {
                ss += 1;
                q >>= 1;
            }

            if (ss == 1)
            {
                sqrt = BigInteger.ModPow(n, (p + 1) / 4, p);
                return true;
            }

            BigInteger z = 2;
            while (BigInteger.ModPow(z, (p - 1) / 2, p) != p - 1)
            {
                z += 1;
            }
            BigInteger c = BigInteger.ModPow(z, q, p);
            BigInteger r = BigInteger.ModPow(n, (q + 1) / 2, p);
            BigInteger t = BigInteger.ModPow(n, q, p);
            BigInteger m = ss;

            while (true)
            {
                if (t == 1)
                {
                    sqrt = r;
                    return true;
                }
                BigInteger i = 0;
                BigInteger zz = t;
                while (zz != 1 && i < (m - 1))
                {
                    zz = zz * zz % p;
                    i += 1;
                }
                BigInteger b = c;
                BigInteger e = m - i - 1;
                while (e > 0)
                {
                    b = b * b % p;
                    e -= 1;
                }
                r = r * b % p;
                c = b * b % p;
                t = t * c % p;
                m = i;
            }
        }


        public static BigInteger ModInversePrime(this BigInteger n, BigInteger p)
        {
            //https://en.wikipedia.org/wiki/Modular_multiplicative_inverse#Using_Euler's_theorem
            return BigInteger.ModPow(n, p - 2, p);
        }

        public static BigInteger Mod(this BigInteger n, BigInteger m)
        {
            BigInteger result = n % m;
            if (result < 0)
            {
                return m + result;
            }

            return result;
        }



        /// <summary>
        /// Assumes number is always positive
        /// </summary>
        /// <param name="bigInt"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this BigInteger bigInt)
        {
            byte[] bytes = bigInt.ToByteArray();
            if (bytes.LastOrDefault() == 0)
            {
                return bytes.Take(bytes.Length - 1).ToArray();
            }

            return bytes;
        }
        /// <summary>
        /// Assumes number is always positive
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BigInteger FromBytes(byte[] bytes)
        {
            BigInteger bigInt = new BigInteger(bytes);
            if (bigInt < 0)
            {
                // add back sign bit
                bytes = bytes.Concat(new byte[1]).ToArray();
                return new BigInteger(bytes);
            }
            return bigInt;
        }
    }
}
