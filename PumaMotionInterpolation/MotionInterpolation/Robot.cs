using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace MotionInterpolation
{

    public class PUMA
    {
        private const int componentsCount = 5;
        private double a1, a2, q2, a3, a4, a5;

        public Element[] components;
        public Joint[] joints;
        private ModelVisual3D startFrame;
        private ModelVisual3D endFrame;
        private ModelVisual3D currentFrame;
        private Configuration startConfiguration;
        private Configuration endConfiguration;
        private Vector3D[] startInternalPositions;
        private Vector3D[] endInternalPositions;
        private EulerToQuaternionConverter converter;

        private MathHelper mathHelper;

        public PUMA(HelixViewport3D viewport, ModelVisual3D start, ModelVisual3D end)
        {
            startFrame = start;
            endFrame = end;
            converter = Singleton<EulerToQuaternionConverter>.Instance;
            mathHelper = Singleton<MathHelper>.Instance;
            Initialize(viewport);
        }

        public void SetMidFrameModel(ModelVisual3D model)
        {
            this.currentFrame = model;
        }

        public void Initialize(HelixViewport3D viewport)
        {
            components = new Element[componentsCount];
            for (int i = 0; i < componentsCount; i++)
                components[i] = new Element();
            joints = new Joint[componentsCount + 1];
            for (int i = 0; i < componentsCount + 1; i++)
                joints[i] = new Joint();
            for (int i = 0; i < componentsCount; i++)
            {
                components[i].Begin = joints[i];
                components[i].End = joints[i + 1];
                viewport.Children.Add(components[i].Tube);
                viewport.Children.Add(components[i].Sphere);
            }

            components[0].Length = 0.0f;
            components[1].Length = 5.0f;
            components[3].Length = 2.0f;
            components[4].Length = 1.0f;
        }

        private void CalculatePositions()
        {
            joints[1].Frame.P = new Position(joints[0].Frame.P);
            joints[2].Frame.P = new Position(joints[0].Frame.P.Value + components[1].Length * joints[0].Frame.Z);
            joints[4].Frame.P = new Position(joints[5].Frame.P.Value - components[4].Length * joints[5].Frame.X);

            Vector3D n = Vector3D.CrossProduct((joints[4].Frame.P.Value - joints[0].Frame.P.Value),
                (joints[2].Frame.P.Value - joints[0].Frame.P.Value));
            n.Normalize();
            Vector3D x5 = joints[5].Frame.X;
            Vector3D p3;
            if (CheckParallelity(n, x5))
            {
                p3 = CalculateP3Paralell(x5);
            }
            else
            {
                p3 = CalculateP3NotParalell(n, x5);
            }
            joints[3].Frame.P = new Position(p3);
        }

        private bool CheckParallelity(Vector3D u, Vector3D v)
        {
            double k1, k2, k3;
            k1 = u.X / v.X;
            k2 = u.Y / v.Y;
            k3 = u.Z / v.Z;
            if (k1 == k2 && k2 == k3)
                return true;
            else
                return false;
        }

        private Vector3D CalculateP3Paralell(Vector3D x5)
        {
            var p3 = new Vector3D();
            if (x5.X != 0)
            {
                p3 = Z3VectorParalell(x5);
            }
            else if (x5.Y != 0)
            {
                p3 = Z3VectorParalell(new Vector3D(x5.Y, x5.X, x5.Z));

            }
            else if (x5.Z != 0)
            {
                p3 = Z3VectorParalell(new Vector3D(x5.Z, x5.X, x5.Y));
            }
            return p3;
        }

        private Vector3D CalculateP3NotParalell(Vector3D n, Vector3D x5)
        {
            var pp3 = new Vector3D();
            if (x5.X != 0)
            {
                pp3 = Z3VectorNotParalell(n, x5);
            }
            else if (x5.Y != 0)
            {
                pp3 = Z3VectorNotParalell(n, new Vector3D(x5.Y, x5.X, x5.Z));
            }
            else if (x5.Z != 0)
            {
                pp3 = Z3VectorNotParalell(n, new Vector3D(x5.Z, x5.X, x5.Y));
                pp3 = new Vector3D(pp3.Y, pp3.Z, pp3.X);
            }
            var p3 = joints[4].Frame.P.Value + components[3].Length * pp3;
            return p3;
        }

        private Vector3D Z3VectorNotParalell(Vector3D n, Vector3D x5)
        {
            var z4 = new Vector3D();
            double a = n.Y - x5.Y / x5.X * n.X;
            double b = n.Z - x5.Z / x5.X * n.X;

            if (a != 0)
            {
                var first = ((b / a * x5.Y - x5.Z) / x5.X);
                var second = b / a;
                var third = 1;
                var val = first * first + second * second + third * third;
                z4.Z = Math.Sqrt(1 / val);
                z4.X = z4.Z * first;
                z4.Y = -z4.Z * second;
            }
            else if (b != 0)
            {
                var first = ((a / b * x5.Z - x5.Y) / x5.X);
                var second = a / b;
                var third = 1;
                var val = first * first + second * second + third * third;
                z4.Y = Math.Sqrt(1 / val);
                z4.X = z4.Y * first;
                z4.Z = -z4.Y * second;
            }
            return z4;
        }

        private Vector3D Z3VectorParalell(Vector3D x5)
        {
            double a = x5.Y / x5.X;
            double b = x5.Z / x5.X;
            double z4X, z4Y, z4Z = 0;

            int numberOfProbes = 100;
            double left = -1;
            double right = 1;
            double len = right - left;


            double minSquareLen = Double.MaxValue;
            var minP3 = new Vector3D();

            for (int i = 0; i < numberOfProbes; i++)
            {
                z4Z = left + (double)i / (double)(numberOfProbes - 1) * len;

                double delta = 2 * a * b * z4Z * 2 * a * b * z4Z - 4 * (a * a + 1) * (z4Z * z4Z * b * b + z4Z * z4Z - 1);
                z4Y = (-(2 * a * b * z4Z) + Math.Sqrt(delta)) / (2 * (2 * a * b * z4Z));
                z4X = -(z4Y * x5.Y + z4Z * x5.Z) / x5.X;

                Vector3D z4 = new Vector3D();
                if (x5.X != 0)
                {
                    z4 = new Vector3D(z4X, z4Y, z4Z);
                }
                else if (x5.Y != 0)
                {
                    z4 = new Vector3D(z4Y, z4X, z4Z);
                }
                else if (x5.Z != 0)
                {
                    z4 = new Vector3D(z4Y, z4Z, z4X);
                }
                var p3 = joints[4].Frame.P.Value + components[3].Length * z4;
                var q2 = p3 - joints[2].Frame.P.Value;
                var squareLen = Vector3D.DotProduct(q2, q2);
                if (squareLen < minSquareLen)
                {
                    minSquareLen = squareLen;
                    minP3 = p3;
                }
            }
            return minP3;
        }

        public void SetupEffectorPosRot(Position position, Rotation rotation)
        {
            joints[5].Frame.P = position;
            var q = new Quaternion();
            converter.Convert(rotation.R, rotation.P, rotation.Y, ref q);
            var m = converter.BuildMatrix3DFromQuaternion(q);
            joints[5].Frame.X = new Vector3D(m.M11, m.M12, m.M13);
            joints[5].Frame.X.Normalize();
            joints[5].Frame.Y = new Vector3D(m.M21, m.M22, m.M23);
            joints[5].Frame.Y.Normalize();
            joints[5].Frame.Z = new Vector3D(m.M31, m.M32, m.M33);
            joints[5].Frame.Z.Normalize();
        }

        public void SetupEffectorPosQuat(Position position, Quaternion quaternion)
        {
            joints[5].Frame.P = position;
            var m = converter.BuildMatrix3DFromQuaternion(quaternion);
            joints[5].Frame.X = new Vector3D(m.M11, m.M12, m.M13);
            joints[5].Frame.Y = new Vector3D(m.M21, m.M22, m.M23);
            joints[5].Frame.Z = new Vector3D(m.M31, m.M32, m.M33);
            joints[5].Frame.X.Normalize();
            joints[5].Frame.Y.Normalize();
            joints[5].Frame.Z.Normalize();
        }

        private void CalcaulateAngles()
        {
            Vector3D forward, right, up;

            forward = joints[5].Frame.X;
            right = joints[5].Frame.Y;
            up = joints[5].Frame.Z;

            a1 = Math.Atan2(joints[5].Frame.P.Y - components[4].Length*forward.Y, joints[5].Frame.P.X - components[4].Length*forward.X);
            var c1 = Math.Cos(a1);
            var s1 = Math.Sin(a1);
            a4 =  Math.Asin(c1 * forward.Y - s1 * forward.X);
            var c4 = Math.Cos(a4);
            var s4 = Math.Sin(a4);
            a5 = Math.Atan2((s1 * up.X - c1 * up.Y),
                (c1 * right.Y - s1 * right.X) );
            var c5 = Math.Cos(a5);
            var s5 = Math.Sin(a5);
            a2 = Math.Atan2(-c1 * c4 * (joints[5].Frame.P.Z - components[4].Length * forward.Z - components[1].Length) -
                    components[3].Length * (forward.X + s1 * s4), c4 * (joints[5].Frame.P.X - components[4].Length * forward.X) -
                    c1 * components[3].Length * forward.Z);
            var c2 = Math.Cos(a2);
            var s2 = Math.Sin(a2);

            q2 = (c4*(joints[5].Frame.P.X - components[4].Length*forward.X) -
                  c1*components[3].Length*forward.Z)/(c1*c2*c4);

            var a23 = Math.Atan2(-forward.Z/c4, (forward.X + s1*s4)/(c1*c4));
            a3 = a23 - a2;

            mathHelper.ConvertAngle(ref a1);
            mathHelper.ConvertAngle(ref a2);
            mathHelper.ConvertAngle(ref a3);
            mathHelper.ConvertAngle(ref a4);
            mathHelper.ConvertAngle(ref a5);
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < componentsCount; i++)
            {
                components[i].Refresh();
            }
        }

        private void TranslateInDirection(Vector3D dir, double val, ref Transform3DGroup eulerTransformGroup)
        {
            eulerTransformGroup.Children.Add(new TranslateTransform3D(val * dir.X, 0, 0));
            eulerTransformGroup.Children.Add(new TranslateTransform3D(0, val * dir.Y, 0));
            eulerTransformGroup.Children.Add(new TranslateTransform3D(0, 0, val * dir.Z));
        }

        private void RotateInDirection(Vector3D dir, Vector3D pos, double val, ref Transform3DGroup eulerTransformGroup)
        {
            eulerTransformGroup.Children.Add(new TranslateTransform3D(-pos.X, -pos.Y, -pos.Z));
            eulerTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(dir, val)));
            eulerTransformGroup.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
        }

        private Vector3D GetX(Transform3DGroup eulerTransformGroup)
        {
            return new Vector3D(eulerTransformGroup.Value.M11, eulerTransformGroup.Value.M12, eulerTransformGroup.Value.M13);
        }
        private Vector3D GetY(Transform3DGroup eulerTransformGroup)
        {
            return new Vector3D(eulerTransformGroup.Value.M21, eulerTransformGroup.Value.M22, eulerTransformGroup.Value.M23);
        }
        private Vector3D GetZ(Transform3DGroup eulerTransformGroup)
        {
            return new Vector3D(eulerTransformGroup.Value.M31, eulerTransformGroup.Value.M32, eulerTransformGroup.Value.M33);
        }

        private Vector3D GetPosition(Transform3DGroup eulerTransformGroup)
        {
            return new Vector3D(eulerTransformGroup.Value.OffsetX, eulerTransformGroup.Value.OffsetY, eulerTransformGroup.Value.OffsetZ);
        }

        private void SetupConfiguration(bool forInitialFrame)
        {
            var eulerTransformGroup = new Transform3DGroup();

            Vector3D dir, pos;

            dir = GetZ(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, a1, ref eulerTransformGroup);

            dir = GetZ(eulerTransformGroup);
            TranslateInDirection(dir, components[1].Length, ref eulerTransformGroup);

            dir = GetY(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, a2, ref eulerTransformGroup);

            dir = GetX(eulerTransformGroup);
            TranslateInDirection(dir, q2, ref eulerTransformGroup);

            dir = GetY(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, a3, ref eulerTransformGroup);

            dir = GetZ(eulerTransformGroup);
            TranslateInDirection(dir, -components[3].Length, ref eulerTransformGroup);

            dir = GetZ(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, a4, ref eulerTransformGroup);

            dir = GetX(eulerTransformGroup);
            TranslateInDirection(dir, components[4].Length, ref eulerTransformGroup);

            dir = GetX(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, a5, ref eulerTransformGroup);

            if (forInitialFrame)
            {
                startConfiguration = new Configuration() {A1 = a1, A2 = a2, Q2 = q2, A3 = a3, A4 = a4, A5 = a5};
                startInternalPositions = new Vector3D[6]
                {
                    joints[0].Frame.P.Value, joints[1].Frame.P.Value, joints[2].Frame.P.Value, joints[3].Frame.P.Value,
                    joints[4].Frame.P.Value, joints[5].Frame.P.Value
                };
                startFrame.Transform = eulerTransformGroup;
            }
            else
            {
                endConfiguration = new Configuration() {A1 = a1, A2 = a2, Q2 = q2, A3 = a3, A4 = a4, A5 = a5};
                endInternalPositions = new Vector3D[6]
                {
                    joints[0].Frame.P.Value, joints[1].Frame.P.Value, joints[2].Frame.P.Value, joints[3].Frame.P.Value,
                    joints[4].Frame.P.Value, joints[5].Frame.P.Value
                };
                endFrame.Transform = eulerTransformGroup;
            }
        }

        public void CalculateConfiguration(bool forInitialFrame)
        {
            CalculatePositions();
            if (forInitialFrame)
                UpdatePositions();
            CalcaulateAngles();
            SetupConfiguration(forInitialFrame);
        }

        private void GetInterpolatedPositions(Configuration interpolatedConfiguration)
        {
            var eulerTransformGroup = new Transform3DGroup();

            Position p0, p1, p2, p3, p4, p5;
            p0 = new Position(0,0,0);
            p1 = new Position(p0);

            Vector3D dir, pos;

            dir = GetZ(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, interpolatedConfiguration.A1, ref eulerTransformGroup);

            dir = GetZ(eulerTransformGroup);
            TranslateInDirection(dir, components[1].Length, ref eulerTransformGroup);

            p2 = new Position(eulerTransformGroup.Value.OffsetX, eulerTransformGroup.Value.OffsetY, eulerTransformGroup.Value.OffsetZ);

            dir = GetY(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, interpolatedConfiguration.A2, ref eulerTransformGroup);

            dir = GetX(eulerTransformGroup);
            TranslateInDirection(dir, interpolatedConfiguration.Q2, ref eulerTransformGroup);

            p3 = new Position(eulerTransformGroup.Value.OffsetX, eulerTransformGroup.Value.OffsetY, eulerTransformGroup.Value.OffsetZ);

            dir = GetY(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, interpolatedConfiguration.A3, ref eulerTransformGroup);

            dir = GetZ(eulerTransformGroup);
            TranslateInDirection(dir, -components[3].Length, ref eulerTransformGroup);

            p4 = new Position(eulerTransformGroup.Value.OffsetX, eulerTransformGroup.Value.OffsetY, eulerTransformGroup.Value.OffsetZ);

            dir = GetZ(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, interpolatedConfiguration.A4, ref eulerTransformGroup);

            dir = GetX(eulerTransformGroup);
            TranslateInDirection(dir, components[4].Length, ref eulerTransformGroup);

            p5 = new Position(eulerTransformGroup.Value.OffsetX, eulerTransformGroup.Value.OffsetY, eulerTransformGroup.Value.OffsetZ);

            dir = GetX(eulerTransformGroup);
            pos = GetPosition(eulerTransformGroup);
            RotateInDirection(dir, pos, interpolatedConfiguration.A5, ref eulerTransformGroup);


            joints[0].Frame.P = p0;
            joints[1].Frame.P = p1;
            joints[2].Frame.P = p2;
            joints[3].Frame.P = p3;
            joints[4].Frame.P = p4;
            joints[5].Frame.P = p5;

            currentFrame.Transform = eulerTransformGroup;
        }

        public void Interpolate(RealTimeInterpolator realTimeInterpolator, double normalizedTime)
        {
            Configuration interpolatedConfiguration = new Configuration();
            interpolatedConfiguration.A1 = realTimeInterpolator.GetValue(normalizedTime, startConfiguration.A1, endConfiguration.A1);
            interpolatedConfiguration.A2 = realTimeInterpolator.GetValue(normalizedTime, startConfiguration.A2, endConfiguration.A2);
            interpolatedConfiguration.Q2 = realTimeInterpolator.GetValue(normalizedTime, startConfiguration.Q2, endConfiguration.Q2);
            interpolatedConfiguration.A3 = realTimeInterpolator.GetValue(normalizedTime, startConfiguration.A3, endConfiguration.A3);
            interpolatedConfiguration.A4 = realTimeInterpolator.GetValue(normalizedTime, startConfiguration.A4, endConfiguration.A4);
            interpolatedConfiguration.A5 = realTimeInterpolator.GetValue(normalizedTime, startConfiguration.A5, endConfiguration.A5);

            GetInterpolatedPositions(interpolatedConfiguration);

            UpdatePositions();
        }
    }
}
