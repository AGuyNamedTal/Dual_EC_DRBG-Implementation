using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TalV.ECCBackdoor.ECMath;
using TalV.ECCBackdoor.Properties;
using TalV.ECCBackdoor.RNG;
using Console = Colorful.Console;

namespace TalV.ECCBackdoor
{
    public class Program
    {
        private static void Main(string[] args)
        {
            EllipticCurve ellipticCurve = GetCurve(args);

            BigInteger secretE = 401;
            ECRngParams rngParams = GenerateParameters(ellipticCurve, secretE);

            ECRng rng = new ECRng(rngParams);
            Console.WriteLine($"Bytes trimmed of output (X of r*Q): {ECRng.TrimmedBytes}", AppColors.Neutral);

            byte[] randomOutput = GenerateRandomData(rng, 70);

            ECRngStateCracker stateCracker = new ECRngStateCracker(rngParams, secretE);
            Console.WriteLine("Finding inner state of RNG", AppColors.Attacker);
            ECRng recoveredRng = null;
            try
            {
                recoveredRng = stateCracker.Crack(randomOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cracker failed: {ex.Message}", AppColors.Error);
                goto End;
            }

            if (recoveredRng == null)
            {
                Console.WriteLine("Cracker failed", AppColors.Error);
                goto End;
            }

            byte[] rngNext = rng.Next();
            Console.WriteLine($"RNG next:\n{rngNext.ToHex()}", AppColors.Victim);
            byte[] recoveredRngNext = recoveredRng.Next();
            Console.WriteLine($"Cracked RNG next:\n{recoveredRngNext.ToHex()}", AppColors.Attacker);
            if (rngNext.SequenceEqual(recoveredRngNext))
            {
                Console.WriteLine("Successfully predicted next values", AppColors.Attacker);
            }
            else
            {
                Console.WriteLine($"Couldn't predict next values {AppColors.Error}");
            }
            End:
            Console.WriteLine("Press any key to exit", AppColors.Neutral);
            Console.ReadKey(true);
        }
        private static EllipticCurve GetCurve(string[] args)
        {
            string json;
            if (args.Length > 0)
            {
                string jsonFileName = args[0];
                Console.WriteLine($"Reading curve from file {jsonFileName}", AppColors.Neutral);
                json = File.ReadAllText(jsonFileName);
            }
            else
            {
                //https://neuromancer.sk/std/secg/secp256r1
                Console.WriteLine("Using default curve from resources", AppColors.Neutral);
                json = Encoding.UTF8.GetString(Resources.secp256r1_json);
            }

            //http://www.christelbach.com/ECCalculator.aspx

            return EllipticCurve.FromJson(json);
        }

        private static ECRngParams GenerateParameters(EllipticCurve curve, BigInteger secretE)
        {
            Console.WriteLine($"Using secret e = {secretE.ToString()}", AppColors.Attacker);

            curve.TryGetPoint(1044, out BigPoint q);
            BigPoint p = curve.Multiply(q, secretE);

            Console.WriteLine($"Using points on curve \nQ:\n{q}\nP:\n{p}\n", AppColors.Victim);

            return new ECRngParams(curve, p, q);
        }

        private static byte[] GenerateRandomData(ECRng rng, int length)
        {
            byte[] randomOutput = new byte[length];
            Console.WriteLine($"Generating public random data of length {length}", AppColors.Victim);
            rng.Next(randomOutput);
            Console.WriteLine($"\n{randomOutput.ToHex()}\n", AppColors.Victim);
            return randomOutput;
        }



    }
}
