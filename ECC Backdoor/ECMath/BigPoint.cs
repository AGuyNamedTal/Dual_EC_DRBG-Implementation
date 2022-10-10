using System.Numerics;

namespace TalV.ECCBackdoor.ECMath
{
    public class BigPoint
    {
        public readonly BigInteger X;
        public readonly BigInteger Y;

        public BigPoint(BigInteger x, BigInteger y)
        {
            X = x;
            Y = y;
        }
        public static bool operator ==(BigPoint obj1, BigPoint obj2)
        {
            bool obj1Null = ReferenceEquals(null, obj1);
            bool obj2Null = ReferenceEquals(null, obj2);
            if (obj1Null && obj2Null)
            {
                return true;
            }

            if (obj1Null || obj2Null)
            {
                return false;
            }

            return obj1.Equals(obj2);
        }
        public static bool operator !=(BigPoint obj1, BigPoint obj2)
        {
            return !(obj1 == obj2);
        }

        protected bool Equals(BigPoint other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((BigPoint)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
        public override string ToString()
        {
            return $"X = {X}, Y = {Y}";
        }

    }
}
