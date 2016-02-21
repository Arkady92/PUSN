using HelixToolkit.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace MotionInterpolation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Quaternion startQuaternion;
        private Quaternion endQuaternion;
        DispatcherTimer dispatcherTimer;
        private Vector3D currentPosition;
        private double currentAngleR;
        private double currentAngleP;
        private double currentAngleY;
        private Quaternion currentQuaternion;
        private bool animationStarted = false;
        private DateTime startTime;
        private DateTime stopTime;
        private TimeSpan timeDelay;
        private bool[] buttonsFlags;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeVariables();
            InitializeScene();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
        }

        private void SetupStartConfiguration()
        {
            var eulerStartTransformGroup = new Transform3DGroup();
            var quaternionStartTransformGroup = new Transform3DGroup();

            eulerStartTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(xDirection, startAngleR)));
            eulerStartTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(yDirection, startAngleP)));
            eulerStartTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(zDirection, startAngleY)));
            eulerStartTransformGroup.Children.Add(new TranslateTransform3D(StartPositionX, StartPositionY, StartPositionZ));
            FrameStartEuler.Transform = eulerStartTransformGroup;

            startQuaternion = new Quaternion(StartQuaternionX, StartQuaternionY, StartQuaternionZ, StartQuaternionW);
            quaternionStartTransformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(startQuaternion)));
            quaternionStartTransformGroup.Children.Add(new TranslateTransform3D(StartPositionX, StartPositionY, StartPositionZ));
            FrameStartQuaternion.Transform = quaternionStartTransformGroup;
        }

        private void SetupEndConfiguration()
        {
            var eulerEndTransformGroup = new Transform3DGroup();
            var quaternionEndTransformGroup = new Transform3DGroup();

            eulerEndTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(xDirection, EndAngleR)));
            eulerEndTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(yDirection, EndAngleP)));
            eulerEndTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(zDirection, EndAngleY)));
            eulerEndTransformGroup.Children.Add(new TranslateTransform3D(EndPositionX, EndPositionY, EndPositionZ));
            FrameEndEuler.Transform = eulerEndTransformGroup;

            endQuaternion = new Quaternion(EndQuaternionX, EndQuaternionY, EndQuaternionZ, EndQuaternionW);
            quaternionEndTransformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(endQuaternion)));
            quaternionEndTransformGroup.Children.Add(new TranslateTransform3D(EndPositionX, EndPositionY, EndPositionZ));
            FrameEndQuaternion.Transform = quaternionEndTransformGroup;
        }

        private void SetupCurrentConfiguration()
        {
            var eulerTransformGroup = new Transform3DGroup();
            var quaternionTransformGroup = new Transform3DGroup();

            eulerTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(xDirection, currentAngleR)));
            eulerTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(yDirection, currentAngleP)));
            eulerTransformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(zDirection, currentAngleY)));
            eulerTransformGroup.Children.Add(new TranslateTransform3D(currentPosition.X, currentPosition.Y, currentPosition.Z));
            frameEuler.Transform = eulerTransformGroup;

            quaternionTransformGroup.Children.Add(new RotateTransform3D(new QuaternionRotation3D(currentQuaternion)));
            quaternionTransformGroup.Children.Add(new TranslateTransform3D(currentPosition.X, currentPosition.Y, currentPosition.Z));
            frameQuaternion.Transform = quaternionTransformGroup;
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            var timeDiff = currentTime - startTime - timeDelay;
            var time = timeDiff.Milliseconds + 1000 * timeDiff.Seconds;
            var timeFactor = time / (AnimationTime * 1000.0);
            if (timeFactor > 1)
            {
                ResetButton_Click(null, null);
                return;
            }

            CalculateCurrentPosition(timeFactor);
            CalculateCurrentAngle(timeFactor);
            CalculateCurrentQuaternion(timeFactor);

            SetupCurrentConfiguration();
        }

        private void CalculateCurrentQuaternion(double timeFactor)
        {
            if (LERPActivated)
            {
                var x = startQuaternion.X * (1 - timeFactor) + endQuaternion.X * timeFactor;
                var y = startQuaternion.Y * (1 - timeFactor) + endQuaternion.Y * timeFactor;
                var z = startQuaternion.Z * (1 - timeFactor) + endQuaternion.Z * timeFactor;
                var w = startQuaternion.W * (1 - timeFactor) + endQuaternion.W * timeFactor;
                currentQuaternion = new Quaternion(x, y, z, w);
                currentQuaternion.Normalize();
            }
            else
            {
                currentQuaternion = Quaternion.Slerp(startQuaternion, endQuaternion, timeFactor);

                double dotProduct = startQuaternion.X * endQuaternion.Y + startQuaternion.Y * endQuaternion.Y
                    + startQuaternion.Z * endQuaternion.Z + startQuaternion.W * endQuaternion.W;

                var theta = Math.Acos(dotProduct);
                if (theta < 0.0) theta = -theta;

                var startFactor = Math.Sin((1 - timeFactor) * theta) / Math.Sin(theta);
                var endFactor = Math.Sin(timeFactor * theta) / Math.Sin(theta);
                var x = startFactor * startQuaternion.X + endFactor * endQuaternion.X;
                var y = startFactor * startQuaternion.Y + endFactor * endQuaternion.Y;
                var z = startFactor * startQuaternion.Z + endFactor * endQuaternion.Z;
                var w = startFactor * startQuaternion.W + endFactor * endQuaternion.W;
                currentQuaternion = new Quaternion(x, y, z, w);
                currentQuaternion.Normalize();
            }
        }

        private void CalculateCurrentAngle(double timeFactor)
        {
            currentAngleR = StartAngleR + timeFactor * (EndAngleR - StartAngleR);
            currentAngleP = StartAngleP + timeFactor * (EndAngleP - StartAngleP);
            currentAngleY = StartAngleY + timeFactor * (EndAngleY - StartAngleY);
        }

        private void CalculateCurrentPosition(double timeFactor)
        {
            currentPosition.X = StartPositionX + timeFactor * (EndPositionX - StartPositionX);
            currentPosition.Y = StartPositionY + timeFactor * (EndPositionY - StartPositionY);
            currentPosition.Z = StartPositionZ + timeFactor * (EndPositionZ - StartPositionZ);
        }

        private void ExtractDataFromTransformationMatrices(Matrix3D startMat, Matrix3D endMat)
        {
            StartPositionX = startMat.OffsetX;
            StartPositionY = startMat.OffsetY;
            StartPositionZ = startMat.OffsetZ;
            EndPositionX = endMat.OffsetX;
            EndPositionY = endMat.OffsetY;
            EndPositionZ = endMat.OffsetZ;

            startQuaternion = ExtractQuaternion(startMat);
            endQuaternion = ExtractQuaternion(endMat);

            StartQuaternionX = Math.Round(startQuaternion.X, 2);
            StartQuaternionY = Math.Round(startQuaternion.Y, 2);
            StartQuaternionZ = Math.Round(startQuaternion.Z, 2);
            StartQuaternionW = Math.Round(startQuaternion.W, 2);
            EndQuaternionX = Math.Round(endQuaternion.X, 2);
            EndQuaternionY = Math.Round(endQuaternion.Y, 2);
            EndQuaternionZ = Math.Round(endQuaternion.Z, 2);
            EndQuaternionW = Math.Round(endQuaternion.W, 2);

            double r = 0, p = 0, y = 0;
            ExtractEulerAngles(startQuaternion, ref r, ref p, ref y);
            StartAngleR = Math.Round(r * 180 / Math.PI, 2);
            StartAngleP = Math.Round(p * 180 / Math.PI, 2);
            StartAngleY = Math.Round(y * 180 / Math.PI, 2);
            ExtractEulerAngles(endQuaternion, ref r, ref p, ref y);
            EndAngleR = Math.Round(r * 180 / Math.PI, 2);
            EndAngleP = Math.Round(p * 180 / Math.PI, 2);
            EndAngleY = Math.Round(y * 180 / Math.PI, 2);
            //StartAngleR = -Math.Round(Math.Asin(startMat.M32) * 180 / Math.PI, 1);
            //StartAngleY = Math.Round(Math.Atan2(startMat.M12, startMat.M22) * 180 / Math.PI, 1);
            //StartAngleP = Math.Round(Math.Atan2(startMat.M31, startMat.M33) * 180 / Math.PI, 1);
            //EndAngleR = -Math.Round(Math.Asin(endMat.M32) * 180 / Math.PI, 1);
            //EndAngleY = Math.Round(Math.Atan2(endMat.M12, endMat.M22) * 180 / Math.PI, 1);
            //EndAngleP = Math.Round(Math.Atan2(endMat.M31, endMat.M33) * 180 / Math.PI, 1);
        }

        private void CalibrateEulerAngles()
        {
            EndAngleR = CalibrateEulerAngle(StartAngleR, EndAngleR);
            EndAngleP = CalibrateEulerAngle(StartAngleP, EndAngleP);
            EndAngleY = CalibrateEulerAngle(StartAngleY, EndAngleY);
        }

        private double CalibrateEulerAngle(double startAngle, double endAngle)
        {
            int diff = (int)(startAngle - endAngle);
            int div = diff / 360;
            endAngle += div * 360;
            if(endAngle > startAngle)
            {
                var d1 = endAngle - startAngle;
                var d2 = -(endAngle - startAngle - 360);
                if(d1 > d2)
                {
                    endAngle -= 360;
                }
            }
            if (endAngle < startAngle)
            {
                var d1 = -(endAngle - startAngle);
                var d2 = endAngle - startAngle + 360;
                if (d1 > d2)
                {
                    endAngle += 360;
                }
            }
            return endAngle;
        }

        private void ExtractEulerAngles(Quaternion q, ref double r, ref double p, ref double y)
        {
            double test = q.X * q.Y + q.Z * q.W;
            if (test > 0.499)
            { // singularity at north pole
                p = 2 * Math.Atan2(q.X, q.W);
                y = Math.PI / 2;
                r = 0;
                return;
            }
            if (test < -0.499)
            { // singularity at south pole
                p = -2 * Math.Atan2(q.X, q.W);
                y = -Math.PI / 2;
                r = 0;
                return;
            }
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;
            p = Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * sqy - 2 * sqz);
            y = Math.Asin(2 * test);
            r = Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * sqx - 2 * sqz);
        }

        private Quaternion ExtractQuaternion(Matrix3D mat)
        {
            var tr = mat.M11 + mat.M22 + mat.M33;
            double qx, qy, qz, qw;

            if (tr > 0)
            {
                var S = Math.Sqrt(tr + 1.0) * 2;
                qw = 0.25 * S;
                qx = (mat.M23 - mat.M32) / S;
                qy = (mat.M31 - mat.M13) / S;
                qz = (mat.M12 - mat.M21) / S;
            }
            else if ((mat.M11 > mat.M22) & (mat.M11 > mat.M33))
            {
                var S = Math.Sqrt(1.0 + mat.M11 - mat.M22 - mat.M33) * 2;
                qw = (mat.M23 - mat.M32) / S;
                qx = 0.25 * S;
                qy = (mat.M21 + mat.M12) / S;
                qz = (mat.M31 + mat.M13) / S;
            }
            else if (mat.M22 > mat.M33)
            {
                var S = Math.Sqrt(1.0 + mat.M22 - mat.M11 - mat.M33) * 2;
                qw = (mat.M31 - mat.M13) / S;
                qx = (mat.M21 + mat.M12) / S;
                qy = 0.25 * S;
                qz = (mat.M32 + mat.M23) / S;
            }
            else
            {
                var S = Math.Sqrt(1.0 + mat.M33 - mat.M11 - mat.M22) * 2;
                qw = (mat.M12 - mat.M21) / S;
                qx = (mat.M31 + mat.M13) / S;
                qy = (mat.M32 + mat.M23) / S;
                qz = 0.25 * S;
            }

            return new Quaternion(qx, qy, qz, qw);
        }
    }
}
