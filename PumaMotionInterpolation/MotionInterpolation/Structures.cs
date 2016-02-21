using System.Windows.Media.Media3D;

namespace MotionInterpolation
{
    public class Position
    {
        public Vector3D Value;
        public double X { get { return Value.X; } set { Value.X = value; } }
        public double Y { get { return Value.Y; } set { Value.Y = value; } }
        public double Z { get { return Value.Z; } set { Value.Z = value; } }
        public Position(double X, double Y, double Z)
        {
            Value.X = X;
            Value.Y = Y;
            Value.Z = Z;
        }

        public Position()
        {
            X = Y = Z = 0;
        }
        public Position(Position other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }

        public Position(Vector3D other)
        {
            Value = other;
        }
    }

    public class Rotation
    {
        public double R { get; set; }
        public double P { get; set; }
        public double Y { get; set; }
        public Rotation(double R, double P, double Y)
        {
            this.R = R;
            this.P = P;
            this.Y = Y;
        }
    }

    public class Configuration
    {
        public double A1 { get; set; }
        public double A2 { get; set; }
        public double Q2 { get; set; }
        public double A3 { get; set; }
        public double A4 { get; set; }
        public double A5 { get; set; }
    }
}
