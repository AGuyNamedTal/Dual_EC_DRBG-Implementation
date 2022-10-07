using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TalV.ECCBackdoor.Properties;
using Console = Colorful.Console;

namespace TalV.ECCBackdoor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // todo: implement EllipticCurve.cs with math operations on points
            // implement multiply and add
            // implement algorithm constants
            // Generate random point on curve Q
            // Generate random secret e
            // Get P = Q*e
            // Implement DUAL_EC_DRBG


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

            EllipticCurve ellipticCurve = EllipticCurve.FromJson(json);
            BigInteger secretE = 401;
            Console.WriteLine($"Using secret e = {secretE.ToString()}", AppColors.Attacker);

            ellipticCurve.TryGetPoint(1044, out BigPoint q);
            BigPoint p = ellipticCurve.Multiply(q, secretE);

            Console.WriteLine($"Using points on curve \nQ:\n{q}\nP:\n{p}\n", AppColors.Victim);

            ECRngParams rngParams = new ECRngParams(ellipticCurve, p, q);

            ECRng rng = new ECRng(rngParams);
            byte[] randomOutput = new byte[70];
            Console.WriteLine($"Generating public random data of length {randomOutput.Length}", AppColors.Victim);
            rng.Next(randomOutput);
            Console.WriteLine($"\n{randomOutput.ToHex()}\n", AppColors.Victim);

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
                goto end;
            }

            if (recoveredRng == null)
            {
                Console.WriteLine("Cracker failed", AppColors.Error);
                goto end;
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
            Debugger.Break();
            end:
            Console.WriteLine("Press any key to exit", AppColors.Neutral);
            Console.ReadKey(true);
        }

    }
}
