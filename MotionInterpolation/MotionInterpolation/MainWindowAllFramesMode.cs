using HelixToolkit.Wpf;
using System.Windows;
using System.Windows.Media.Media3D;

namespace MotionInterpolation
{
    public partial class MainWindow
    {
        CombinedManipulator[] allFramesEuler;
        CombinedManipulator[] allFramesQuaternion;

        private void AllFramesModeButton_Click(object sender, RoutedEventArgs e)
        {
            HelixViewportLeft.Children.Remove(FrameStartEulerManipulator);
            HelixViewportLeft.Children.Remove(FrameEndEulerManipulator);
            HelixViewportRight.Children.Remove(FrameStartQuaternionManipulator);
            HelixViewportRight.Children.Remove(FrameEndQuaternionManipulator);

            PlayButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            FramesCountBox.IsEnabled = false;
            AllFramesModeButton.IsEnabled = false;
            AllFramesModeClearButton.IsEnabled = true;

            if (FramesCount < 2)
                FramesCount = 2;

            allFramesEuler = new CombinedManipulator[FramesCount];
            allFramesQuaternion = new CombinedManipulator[FramesCount];

            CalibrateEulerAngles();
            for (int i = 0; i < FramesCount; i++)
            {
                var timeFactor = i / (FramesCount - 1.0);
                CalculateCurrentPosition(timeFactor);
                CalculateCurrentAngle(timeFactor);
                CalculateCurrentQuaternion(timeFactor);
                SetupCurrentConfiguration();

                allFramesEuler[i] = new CombinedManipulator()
                {
                    Transform = new MatrixTransform3D(frameEuler.Transform.Value),
                    CanRotateX = false,
                    CanRotateY = false,
                    CanRotateZ = false
                };
                allFramesQuaternion[i] = new CombinedManipulator()
                {
                    Transform = new MatrixTransform3D(frameQuaternion.Transform.Value),
                    CanRotateX = false,
                    CanRotateY = false,
                    CanRotateZ = false
                };

                HelixViewportLeft.Children.Add(allFramesEuler[i]);
                HelixViewportRight.Children.Add(allFramesQuaternion[i]);
            }
        }


        private void AllFramesModeClearButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            FramesCountBox.IsEnabled = true;
            AllFramesModeButton.IsEnabled = true;
            AllFramesModeClearButton.IsEnabled = false;

            HelixViewportLeft.Children.Add(FrameStartEulerManipulator);
            HelixViewportLeft.Children.Add(FrameEndEulerManipulator);
            HelixViewportRight.Children.Add(FrameStartQuaternionManipulator);
            HelixViewportRight.Children.Add(FrameEndQuaternionManipulator);

            for (int i = 0; i < FramesCount; i++)
            {
                HelixViewportLeft.Children.Remove(allFramesEuler[i]);
                HelixViewportRight.Children.Remove(allFramesQuaternion[i]);
            }
        }
    }
}
