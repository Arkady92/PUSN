using HelixToolkit.Wpf;
using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace MotionInterpolation
{
    public partial class MainWindow
    {
        private Vector3D xDirection;
        private Vector3D yDirection;
        private Vector3D zDirection;
        CombinedManipulator frameEuler;
        CombinedManipulator frameQuaternion;

        private void InitializeScene()
        {
            const double maxVal = 8;

            var arrowX = new ArrowVisual3D();
            arrowX.Direction = new Vector3D(1, 0, 0);
            arrowX.Point1 = new Point3D(0, 0, 0);
            arrowX.Point2 = new Point3D(maxVal, 0, 0);
            arrowX.Diameter = 0.1;
            arrowX.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportLeft.Children.Add(arrowX);

            arrowX = new ArrowVisual3D();
            arrowX.Direction = new Vector3D(1, 0, 0);
            arrowX.Point1 = new Point3D(0, 0, 0);
            arrowX.Point2 = new Point3D(maxVal, 0, 0);
            arrowX.Diameter = 0.1;
            arrowX.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportRight.Children.Add(arrowX);

            var arrowMX = new ArrowVisual3D();
            arrowMX.Direction = new Vector3D(-1, 0, 0);
            arrowMX.Point1 = new Point3D(0, 0, 0);
            arrowMX.Point2 = new Point3D(-maxVal, 0, 0);
            arrowMX.Diameter = 0.1;
            arrowMX.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportLeft.Children.Add(arrowMX);

            arrowMX = new ArrowVisual3D();
            arrowMX.Direction = new Vector3D(-1, 0, 0);
            arrowMX.Point1 = new Point3D(0, 0, 0);
            arrowMX.Point2 = new Point3D(-maxVal, 0, 0);
            arrowMX.Diameter = 0.1;
            arrowMX.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportRight.Children.Add(arrowMX);

            var arrowY = new ArrowVisual3D();
            arrowY.Direction = new Vector3D(0, 1, 0);
            arrowY.Point1 = new Point3D(0, 0, 0);
            arrowY.Point2 = new Point3D(0, maxVal, 0);
            arrowY.Diameter = 0.1;
            arrowY.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportLeft.Children.Add(arrowY);

            arrowY = new ArrowVisual3D();
            arrowY.Direction = new Vector3D(0, 1, 0);
            arrowY.Point1 = new Point3D(0, 0, 0);
            arrowY.Point2 = new Point3D(0, maxVal, 0);
            arrowY.Diameter = 0.1;
            arrowY.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportRight.Children.Add(arrowY);

            var arrowMY = new ArrowVisual3D();
            arrowMY.Direction = new Vector3D(0, -1, 0);
            arrowMY.Point1 = new Point3D(0, 0, 0);
            arrowMY.Point2 = new Point3D(0, -maxVal, 0);
            arrowMY.Diameter = 0.1;
            arrowMY.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportLeft.Children.Add(arrowMY);

            arrowMY = new ArrowVisual3D();
            arrowMY.Direction = new Vector3D(0, -1, 0);
            arrowMY.Point1 = new Point3D(0, 0, 0);
            arrowMY.Point2 = new Point3D(0, -maxVal, 0);
            arrowMY.Diameter = 0.1;
            arrowMY.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportRight.Children.Add(arrowMY);

            var arrowZ = new ArrowVisual3D();
            arrowZ.Direction = new Vector3D(0, 0, 1);
            arrowZ.Point1 = new Point3D(0, 0, 0);
            arrowZ.Point2 = new Point3D(0, 0, maxVal);
            arrowZ.Diameter = 0.1;
            arrowZ.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportLeft.Children.Add(arrowZ);

            arrowZ = new ArrowVisual3D();
            arrowZ.Direction = new Vector3D(0, 0, 1);
            arrowZ.Point1 = new Point3D(0, 0, 0);
            arrowZ.Point2 = new Point3D(0, 0, maxVal);
            arrowZ.Diameter = 0.1;
            arrowZ.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportRight.Children.Add(arrowZ);

            var arrowMZ = new ArrowVisual3D();
            arrowMZ.Direction = new Vector3D(0, 0, -1);
            arrowMZ.Point1 = new Point3D(0, 0, 0);
            arrowMZ.Point2 = new Point3D(0, 0, -maxVal);
            arrowMZ.Diameter = 0.1;
            arrowMZ.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportLeft.Children.Add(arrowMZ);

            arrowMZ = new ArrowVisual3D();
            arrowMZ.Direction = new Vector3D(0, 0, -1);
            arrowMZ.Point1 = new Point3D(0, 0, 0);
            arrowMZ.Point2 = new Point3D(0, 0, -maxVal);
            arrowMZ.Diameter = 0.1;
            arrowMZ.Fill = System.Windows.Media.Brushes.Black;
            HelixViewportRight.Children.Add(arrowMZ);

            var xArrowText = new TextVisual3D();
            xArrowText.Text = "X";
            xArrowText.Position = new Point3D(maxVal - 0.5, 0, 0.5);
            xArrowText.Height = 0.5;
            xArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportLeft.Children.Add(xArrowText);

            xArrowText = new TextVisual3D();
            xArrowText.Text = "X";
            xArrowText.Position = new Point3D(maxVal - 0.5, 0, 0.5);
            xArrowText.Height = 0.5;
            xArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportRight.Children.Add(xArrowText);

            var yArrowText = new TextVisual3D();
            yArrowText.Text = "Y";
            yArrowText.Position = new Point3D(0, maxVal - 0.5, 0.5);
            yArrowText.Height = 0.5;
            yArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportLeft.Children.Add(yArrowText);

            yArrowText = new TextVisual3D();
            yArrowText.Text = "Y";
            yArrowText.Position = new Point3D(0, maxVal - 0.5, 0.5);
            yArrowText.Height = 0.5;
            yArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportRight.Children.Add(yArrowText);

            var zArrowText = new TextVisual3D();
            zArrowText.Text = "Z";
            zArrowText.Position = new Point3D(0.5, 0, maxVal - 0.5);
            zArrowText.Height = 0.5;
            zArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportLeft.Children.Add(zArrowText);

            zArrowText = new TextVisual3D();
            zArrowText.Text = "Z";
            zArrowText.Position = new Point3D(0.5, 0, maxVal - 0.5);
            zArrowText.Height = 0.5;
            zArrowText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportRight.Children.Add(zArrowText);

            var leftText = new TextVisual3D();
            leftText.Text = "Euler Angles Interpolation";
            leftText.Position = new Point3D(0, 0, maxVal + 0.5);
            leftText.Height = 1;
            leftText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportLeft.Children.Add(leftText);

            var rightText = new TextVisual3D();
            rightText.Text = "Quaternion Interpolation";
            rightText.Position = new Point3D(0, 0, maxVal + 0.5);
            rightText.Height = 1;
            rightText.FontWeight = System.Windows.FontWeights.Bold;
            HelixViewportRight.Children.Add(rightText);

            SetupStartConfiguration();
            SetupEndConfiguration();

            frameEuler = new CombinedManipulator()
            {
                CanRotateX = false,
                CanRotateY = false,
                CanRotateZ = false
            };

            frameQuaternion = new CombinedManipulator()
            {
                CanRotateX = false,
                CanRotateY = false,
                CanRotateZ = false
            };
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (buttonsFlags[0]) return;
            for (int i = 0; i < buttonsFlags.Length; i++)
                buttonsFlags[i] = false;
            buttonsFlags[0] = true;
            AllFramesModeButton.IsEnabled = false;
            AllFramesModeClearButton.IsEnabled = false;

            if (!animationStarted)
            {
                animationStarted = true;
                CalibrateEulerAngles();
                HelixViewportLeft.Children.Remove(FrameStartEulerManipulator);
                HelixViewportLeft.Children.Remove(FrameEndEulerManipulator);
                HelixViewportRight.Children.Remove(FrameStartQuaternionManipulator);
                HelixViewportRight.Children.Remove(FrameEndQuaternionManipulator);
                currentPosition = new Vector3D(StartPositionX, StartPositionY, StartPositionZ);
                currentAngleR = startAngleR;
                currentAngleP = startAngleP;
                currentAngleY = startAngleY;
                currentQuaternion = startQuaternion;

                SetupCurrentConfiguration();

                HelixViewportLeft.Children.Add(frameEuler);
                HelixViewportRight.Children.Add(frameQuaternion);
                startTime = DateTime.Now;
                dispatcherTimer.Start();
                return;
            }
            timeDelay += DateTime.Now - stopTime;
            dispatcherTimer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (buttonsFlags[1]) return;
            for (int i = 0; i < buttonsFlags.Length; i++)
                buttonsFlags[i] = false;
            buttonsFlags[1] = true;
            dispatcherTimer.Stop();
            stopTime = DateTime.Now;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (buttonsFlags[2]) return;
            for (int i = 0; i < buttonsFlags.Length; i++)
                buttonsFlags[i] = false;
            buttonsFlags[2] = true;
            dispatcherTimer.Stop();
            animationStarted = false;
            timeDelay = new TimeSpan();

            HelixViewportLeft.Children.Remove(frameEuler);
            HelixViewportRight.Children.Remove(frameQuaternion);

            HelixViewportLeft.Children.Add(FrameStartEulerManipulator);
            HelixViewportLeft.Children.Add(FrameEndEulerManipulator);
            HelixViewportRight.Children.Add(FrameStartQuaternionManipulator);
            HelixViewportRight.Children.Add(FrameEndQuaternionManipulator);

            AllFramesModeButton.IsEnabled = true;
            AllFramesModeClearButton.IsEnabled = true;
        }

        private void StartApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            SetupStartConfiguration();
        }


        private void EndApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            SetupEndConfiguration();
        }

        private void ApplyConfigurationFromEulerAnglesSceneButton_Click(object sender, RoutedEventArgs e)
        {
            var startMat = FrameStartEuler.Transform.Value;
            var endMat = FrameEndEuler.Transform.Value;
            //FrameStartQuaternion.Transform = new MatrixTransform3D(startMat);
            //FrameEndQuaternion.Transform = new MatrixTransform3D(endMat);
            ExtractDataFromTransformationMatrices(startMat, endMat);
            SetupStartConfiguration();
            SetupEndConfiguration();
        }

        private void ApplyConfigurationFromQuaternionSceneButton_Click(object sender, RoutedEventArgs e)
        {
            var startMat = FrameStartQuaternion.Transform.Value;
            var endMat = FrameEndQuaternion.Transform.Value;
            //FrameStartEuler.Transform = new MatrixTransform3D(startMat);
            //FrameEndEuler.Transform = new MatrixTransform3D(endMat);
            ExtractDataFromTransformationMatrices(startMat, endMat);
            SetupStartConfiguration();
            SetupEndConfiguration();
        }
    }
}
