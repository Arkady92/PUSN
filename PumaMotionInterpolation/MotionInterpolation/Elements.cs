using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace MotionInterpolation
{
    public class Element
    {
        static double tubeDiameter = 0.5;
        public TubeVisual3D Tube { get; set; }
        static Random r = new Random();

        public Element()
        {
            Tube = new TubeVisual3D();
            Tube.Path = new Point3DCollection();
            Tube.Path.Add(new Point3D(-15, 0, 0));
            Tube.Path.Add(new Point3D(15, 0, 0));
            Tube.Diameter = tubeDiameter;
            Tube.IsSectionClosed = true;
            Tube.Fill = new SolidColorBrush(Colors.Silver);
            Tube.IsPathClosed = false;

            Sphere = new CubeVisual3D();
            //Sphere.Center = new Point3D(15, 0, 0);
            Sphere.Fill = new SolidColorBrush(Colors.Silver);
            Sphere.SideLength = 0.75;
        }

        public void Refresh()
        {
            Tube.Path[0] = new Point3D(Begin.Frame.P.X, Begin.Frame.P.Y, Begin.Frame.P.Z);
            Tube.Path[1] = new Point3D(End.Frame.P.X, End.Frame.P.Y, End.Frame.P.Z);


            //Sphere.Transform = new MatrixTransform3D(new Matrix3D(End.Frame.X.X, End.Frame.X.Y, End.Frame.X.Z, 0,
            //    End.Frame.Y.X, End.Frame.Y.Y, End.Frame.Y.Z, 0,
            //    End.Frame.Z.X, End.Frame.Z.Y, End.Frame.Z.Z, 0,0,0,0, 1));
            Sphere.Center = new Point3D(End.Frame.P.X, End.Frame.P.Y, End.Frame.P.Z);
        }
        public Joint Begin { get; set; }
        public Joint End { get; set; }
        public double Length { get; set; }
        public CubeVisual3D Sphere { get; internal set; }
    }

    public class Joint
    {
        public Frame Frame { get; set; }
        public int Id;

        private static int id = 0;

        public Joint()
        {
            this.Frame = new Frame();
            Id = id++;
        }
    }

    public class Frame
    {
        public Vector3D X { get; set; }
        public Vector3D Y { get; set; }
        public Vector3D Z { get; set; }

        public Position P { get; set; }

        public Frame()
        {
            X = new Vector3D(1.0f, 0, 0);
            Y = new Vector3D(0, 1.0f, 0.0f);
            Z = new Vector3D(0, 0, 1.0f);
            P = new Position();
        }

        public Frame(Frame other)
        {
            X = new Vector3D(other.X.X, other.X.Y, other.X.Z);
            Y = new Vector3D(other.Y.X, other.Y.Y, other.Y.Z);
            Z = new Vector3D(other.Z.X, other.Z.Y, other.Z.Z);
            P = new Position(other.P);
        }
    }
}
