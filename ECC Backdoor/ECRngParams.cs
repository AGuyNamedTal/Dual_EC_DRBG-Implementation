namespace TalV.ECCBackdoor
{
    public class ECRngParams
    {

        public readonly EllipticCurve Curve;
        public readonly BigPoint P;
        public readonly BigPoint Q;
        public ECRngParams(EllipticCurve curve, BigPoint p, BigPoint q)
        {
            Curve = curve;
            P = p;
            Q = q;
        }
    }
}
