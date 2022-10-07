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
            //for (int i = 0; i < bytes.Length / 2; i++)
            //{
            //    Swap(ref bytes[i], ref bytes[bytes.Length - i - 2]);
            //    Swap(ref bytes[i + 1], ref bytes[bytes.Length - i - 1]);
            //}
            return FromBytes(bytes);
            //hex = hex.ToUpper();
            //BigInteger HexChatToBigInt(char c)
            //{
            //    if (c >= '0' && c <= '9')
            //    {
            //        return new BigInteger((int)(c - '0'));
            //    }

            //    if (c >= 'A' && c <= 'F')
            //    {
            //        return new BigInteger(10 + (c - 'A'));
            //    }
            //    if (c >= 'a' && c <= 'f')
            //    {
            //        return new BigInteger(10 + (c - 'a'));
            //    }

            //    throw new FormatException();
            //}

            //BigInteger hexBase = 16;


            //char[] hexChars = hex.ToCharArray();

            //BigInteger result = 0;
            //for (int i = 0; i < hexChars.Length; i++)
            //{
            //    int power = hexChars.Length - i - 1;
            //    result += HexChatToBigInt(hexChars[i]) * BigInteger.Pow(hexBase, power);
            //}

            //return result;
        }


        public static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        // based on: https://gist.github.com/LaurentMazare/6745649
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
            BigInteger s = 0;
            BigInteger q = p - 1;
            while ((q & 1) == 0)
            {
                q >>= 1;
                ++s;
            }

            if (s == 1)
            {
                BigInteger r = BigInteger.ModPow(n, (p + 1) / 4, p);
                if ((r * r) % p == n)
                {
                    sqrt = r;
                    return true;
                }

                return false;
            }

            BigInteger z = 1;
            while (BigInteger.ModPow(++z, (p - 1) / 2, p) != p - 1)
            {

            }

            {
                BigInteger c = BigInteger.ModPow(z, q, p);
                BigInteger r = BigInteger.ModPow(n, (q + 1) / 2, p);
                BigInteger t = BigInteger.ModPow(n, q, p);
                BigInteger m = s;
                while (t != 1)
                {
                    BigInteger tt = t;
                    BigInteger i = 0;
                    while (tt != 1)
                    {
                        tt = (tt * tt) % p;
                        ++i;
                        if (i == m)
                        {
                            return false;
                        }
                    }

                    BigInteger b = BigInteger.ModPow(c, BigInteger.ModPow(2, m - i - 1, p - 1), p);
                    BigInteger b2 = (b * b) % p;
                    r = (r * b) % p;
                    t = (t * b2) % p;
                    c = b2;
                    m = i;
                }

                if ((r * r) % p == n)
                {
                    sqrt = r;
                    return true;
                }

                return false;
            }
        }


        public static void Swap<T>(ref T obj1, ref T obj2)
        {
            T temp = obj2;
            obj2 = obj1;
            obj1 = temp;
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
                bytes = bytes.Concat(new byte[1]).ToArray();
                return new BigInteger(bytes);
            }
            return bigInt;
        }
    }
}
