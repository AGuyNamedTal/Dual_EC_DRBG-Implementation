using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
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
            Console.Title = "Dual EC DRBG Backdoor PoC";
            Console.WriteLine("I am Alice the victim!", AppColors.Victim);
            Console.WriteLine("I am Eve the attacker!", AppColors.Attacker);
            Console.WriteLine($"Bytes trimmed of output (X of r*Q): {ECRng.TrimmedBytes}", AppColors.Neutral);
            Console.WriteLine("(You can change it in ECRng.TrimmedBytes)", AppColors.Neutral);
            Console.WriteLine("Starting in one second...", AppColors.Neutral);
            Thread.Sleep(1000);


            EllipticCurve ellipticCurve = GetCurve(args);
            BigInteger secretE = Math.Abs("I am the baddie and I am the only one that knows this yay".GetHashCode());
            ECRngParams rngParams = GenerateParameters(ellipticCurve, secretE);

            ECRng rng = new ECRng(rngParams);


            byte[] randomOutput = GenerateRandomData(rng, 70);

            ECRngCracker cracker = new ECRngCracker(rngParams, secretE);
            Console.WriteLine("Finding inner state of RNG", AppColors.Attacker);
            ECRng recoveredRng;
            try
            {
                recoveredRng = cracker.Crack(randomOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nCracker failed: \n{ex.Message}", AppColors.Error);
                goto End;
            }

            if (recoveredRng == null)
            {
                Console.WriteLine("Cracker failed.", AppColors.Error);
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
                Console.WriteLine($"Couldn't predict next values (???)", AppColors.Error);
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
            // pick random value for q
            Random random = new Random();
            BigPoint q = null;
            bool foundPoint = false;
            while (!foundPoint)
            {
                int x = Math.Abs(random.Next());
                foundPoint = curve.TryGetPoint(x, out q);
            }

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
