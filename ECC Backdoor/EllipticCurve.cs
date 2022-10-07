using Colorful;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Text;
using TalV.ECCBackdoor.Properties;

namespace TalV.ECCBackdoor
{
    /// <summary>
    /// A function of the form y^2 = x^3 + ax + b (mod p)
    /// </summary>
    public class EllipticCurve
    {
        public readonly string Name;


        public readonly BigInteger A;
        public readonly BigInteger B;
        public readonly BigInteger P;
        /// <summary>
        /// Field size in bits
        /// </summary>
        public readonly int FieldSize;



        public EllipticCurve(BigInteger a, BigInteger b, BigInteger p, int fieldSize, string name)
        {
            A = a;
            B = b;
            P = p;
            FieldSize = fieldSize;
            Name = name;
        }

        public static EllipticCurve FromJson(string json)
        {
            JToken curve =
                (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(Resources.secp256r1_json));
            string name = curve["name"].ToString();
            Console.WriteLine($"Loading curve {name}", AppColors.Neutral);
            return new EllipticCurve(
                a: BigIntegerHelper.FromHex(curve["params"]["a"]["raw"].ToString()),
                b: BigIntegerHelper.FromHex(curve["params"]["b"]["raw"].ToString()),
                p: BigIntegerHelper.FromHex(curve["field"]["p"].ToString()),
                fieldSize: int.Parse(curve["field"]["bits"].ToString()),
                name: name);
        }


        #region Point Operations

        //https://en.wikipedia.org/wiki/Elliptic_curve_point_multiplication#Point_operations
        public BigPoint AddPoints(BigPoint point1, BigPoint point2)
        {
            if (point1 == point2)
            {
                return DoublePoint(point1);
            }
            // Calculate m = dy/dx = dy * (dx ^ -1)
            if (point2.X > point1.X)
            {
                BigIntegerHelper.Swap(ref point1, ref point2);
            }

            BigInteger dx = point1.X - point2.X;
            BigInteger dy = point1.Y - point2.Y;
            BigInteger dxInverse = dx.ModInversePrime(P);
            BigInteger m = (dy * dxInverse).Mod(P);
            BigInteger rX = (m * m - point1.X - point2.X).Mod(P);
            BigInteger rY = (m * (point2.X - rX) - point2.Y).Mod(P);
            return new BigPoint(rX, rY);
        }


        public BigPoint DoublePoint(BigPoint point)
        {
            // Calculate m = (3x^2 + a)/2y = (3x^2+a) * (2y)^ -1
            BigInteger m =
             ((3 * (point.X * point.X) + A) * ((2 * point.Y).ModInversePrime(P))).Mod(P);
            BigInteger rX = (m * m - point.X - point.X).Mod(P);
            BigInteger rY = (m * (point.X - rX) - point.Y).Mod(P);
            return new BigPoint(rX, rY);
        }
        /// <summary>
        /// Using the double and add algorithm
        /// </summary>
        /// <param name="point"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public BigPoint Multiply(BigPoint point, BigInteger k)
        {
            if (k == 1)
            {
                return point;
            }
            if (k % 2 == 1)
            {
                return AddPoints(point, Multiply(point, k - 1));
            }
            return Multiply(DoublePoint(point), k / 2);
        }



        #endregion

        public bool TryGetY(BigInteger x, out BigInteger y)
        {
            return (BigInteger.ModPow(x, 3, P) + ((A * x).Mod(P)) + B).Mod(P).TonelliShanksSqrt(P, out y);
        }

        public bool TryGetPoint(BigInteger x, out BigPoint point)
        {
            bool res = TryGetY(x, out BigInteger y);
            if (!res)
            {
                point = null;
                return false;
            }
            point = new BigPoint(x, y);
            return true;
        }
        public bool TryGetY(BigInteger x, out (BigInteger, BigInteger) yValues)
        {
            if (!TryGetY(x, out BigInteger y))
            {
                yValues = (BigInteger.Zero, BigInteger.Zero);
                return false;
            }

            yValues = (y, (-y).Mod(P));
            return true;
        }
        public bool TryGetPoints(BigInteger x, out (BigPoint, BigPoint) points)
        {
            bool res = TryGetY(x, out (BigInteger, BigInteger) yVals);
            if (!res)
            {
                points = (null, null);
                return false;
            }
            points = (new BigPoint(x, yVals.Item1), new BigPoint(x, yVals.Item2));
            return true;
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
