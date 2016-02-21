using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RobotKinematics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private SegmentsIntersector segmentsIntersector;
        private ColliderContainer colliderContainer;
        private ConfigurationSpace configurationSpace;
        private Robot robot;
        private LinearInterpolator linearInterpolator;
        private PathFinder pathFinder;
        private MouseSelector mouseSelector;
        private DispatcherTimer timer;
        private bool animationPending = false;
        private bool configuartionSpaceInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PreInitialize();
            InitializeColliders();
            InitializeRobot();
            //colliderContainer.GenerateAnglesArray(robot, dragCanvas, segmentsIntersector);
            //IntializeConfigurationSpace();
            //PostInitialize();
        }

        private void IntializeConfigurationSpace()
        {
            configurationSpace = new ConfigurationSpace();
            configurationSpace.Init(Image);
            configurationSpace.MarkUnreachableCells(colliderContainer.anglesArray);
        }

        private void InitializeRobot()
        {
            robot = new Robot(dragCanvas);
            //IList<Line> lines =  robot.GetLines();
            //Canvas.Children.Add(lines[0]);
            //Canvas.Children.Add(lines[1]);
        }

        private void ShowSelectedPosition(Point p)
        {
            robot.CalculateInverseKinematicsSecond(p.X, p.Y);
            robot.Reset(dragCanvas);
            IList<Line> lines = robot.GetLines();
            foreach (var line in lines)
            {
                line.Stroke = Brushes.Blue;
            }
            dragCanvas.Children.Add(lines[0]);
            dragCanvas.Children.Add(lines[1]);
        }

        private void TestRobotMovement()
        {
            double x1, y1, x2, y2;
            x1 = 100;
            y1 = 0;
            x2 = 25;
            y2 = -56;

            Point startPoint = new Point(x1, y1);
            Point endPoint = new Point(x2, y2);

            pathFinder.MoveRobot(startPoint, endPoint);
        }

        private void InitializeColliders()
        {
            colliderContainer = new ColliderContainer();
            colliderContainer.GenerateColliders(dragCanvas);
        }

        private void CheckIntersections()
        {
            Line l1 = new Line();
            l1.StrokeThickness = 3;
            l1.Stroke = Brushes.Blue;
            l1.X1 = 20;
            l1.Y1 = 20;
            l1.X2 = 300;
            l1.Y2 = 350;

            Line l2 = new Line();
            l2.X1 = 20;
            l2.Y1 = 0;
            l2.X2 = 0;
            l2.Y2 = 20;

            //Canvas.Children.Add(l1);

            if (colliderContainer == null)
                return;

            foreach (var extendedRectangle in colliderContainer.Colliders)
            {
                foreach (var line in extendedRectangle.GetLines())
                {
                    bool res = segmentsIntersector.Intersect(l1, line);
                    if (res)
                    {
                        Line l = new Line();
                        l.StrokeThickness = 3;
                        l.Stroke = Brushes.Red;
                        l.X1 = line.X1;
                        l.Y1 = line.Y1;
                        l.X2 = line.X2;
                        l.Y2 = line.Y2;
                        dragCanvas.Children.Add(l);
                    }
                }
            }
        }

        private void PreInitialize()
        {
            EditorMode = true;
            AnimationTime = 1500;
            L1 = 200;
            L2 = 100;

            segmentsIntersector = new SegmentsIntersector();
            mouseSelector = new MouseSelector();
            mouseSelector.EditorMode = EditorMode;
            linearInterpolator = new LinearInterpolator();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += TimerOnTick;
            timer.Stop();
        }

        private void PostInitialize()
        {
            pathFinder = new PathFinder(colliderContainer, configurationSpace, robot, linearInterpolator, timer);
        }

        private void onAnimationEnded()
        {
            animationPending = false;
            robot.Update(pathFinder.path[pathFinder.path.Count - 1], dragCanvas, mouseSelector);
            dragCanvas.Children.Remove(mouseSelector.FirstLine);
            dragCanvas.Children.Remove(mouseSelector.SecondLine);
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            animationPending = true;

            var currentTime = DateTime.Now;
            var timeDifference = currentTime - linearInterpolator.startTime - linearInterpolator.timeDelay;
            var elapsedMilliseconds = timeDifference.TotalMilliseconds;
            var animationTimeInMilliseconds = animationTime;
            var normalizedTime = elapsedMilliseconds / animationTimeInMilliseconds;
            if (normalizedTime >= 0 && normalizedTime <= 1)
            {
                Point p = linearInterpolator.GetValue(normalizedTime);
                robot.Update(p, dragCanvas, mouseSelector);
            }
            else
            {
                onAnimationEnded();
            }

        }

        private void Canvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!configuartionSpaceInitialized) return;
            Point canvasRelative = Mouse.GetPosition(dragCanvas);
            Point origin = new Point(dragCanvas.Width / 2, dragCanvas.Height / 2);
            Point originRelative = new Point(0, 0) + (canvasRelative - origin);

            mouseSelector.Set(canvasRelative, originRelative, dragCanvas);

            if (editorMode)
            {
                editorModeLogic();
            }
        }

        private void editorModeLogic()
        {
            if (configuartionSpaceInitialized && EditorMode)
                pathFinder.PresentPossibleSolutions(mouseSelector.StartPosition, segmentsIntersector, dragCanvas);
        }

        private void animationModeLogic()
        {
            //Resuming after pause
            if (animationPending)
            {
                linearInterpolator.timeDelay += DateTime.Now - linearInterpolator.stopTime;
                timer.Start();
                return;
            }
            //Starting from the begining
            if (mouseSelector.Start != null && mouseSelector.End != null)
            {
                timer.Stop();
                //Remove lines
                if (mouseSelector.FirstLine != null & mouseSelector.SecondLine != null)
                {
                    dragCanvas.Children.Remove(mouseSelector.FirstLine);
                    dragCanvas.Children.Remove(mouseSelector.SecondLine);
                    mouseSelector.FirstLine = mouseSelector.SecondLine = null;
                }
                pathFinder.MoveRobot(mouseSelector.StartPosition, mouseSelector.EndPosition);
            }
            else
            {
                MessageBox.Show("Configurations not selected");
            }
        }

        private void OnPlayButtonClicked(object sender, RoutedEventArgs e)
        {
            if (EditorMode)
                editorModeLogic();
            else
                animationModeLogic();
        }

        private void OnPauseButtonClicked(object sender, RoutedEventArgs e)
        {
            linearInterpolator.stopTime = DateTime.Now;
            timer.Stop();
        }

        private void OnRestartButtonClicked(object sender, RoutedEventArgs e)
        {
            //Stop timer and restart delay
            timer.Stop();
            linearInterpolator.timeDelay = new TimeSpan();

            //Remove points
            if (mouseSelector.Start != null && mouseSelector.End != null)
            {
                dragCanvas.Children.Remove(mouseSelector.Start);
                dragCanvas.Children.Remove(mouseSelector.End);
                mouseSelector.Start = mouseSelector.End = null;
            }

            //Remove lines
            if (mouseSelector.FirstLine != null & mouseSelector.SecondLine != null)
            {
                dragCanvas.Children.Remove(mouseSelector.FirstLine);
                dragCanvas.Children.Remove(mouseSelector.SecondLine);
                mouseSelector.FirstLine = mouseSelector.SecondLine = null;
            }

            //Remove possible area
            colliderContainer.Reset(dragCanvas);

            //Remove visualization of configuration space and generate again
            //configurationSpace.Reset();
            //colliderContainer.GenerateAnglesArray(robot, dragCanvas, segmentsIntersector);
            //configurationSpace.MarkUnreachableCells(colliderContainer.anglesArray);

        }

        //private void EditorModeValueChanged(object sender, RoutedEventArgs e)
        //{
        //    RadioButton radioButton = sender as RadioButton;

        //    if (radioButton.IsChecked.Value)
        //    {
        //        mouseSelector.EditorMode = true;
        //    }
        //    else
        //    {
        //        mouseSelector.EditorMode = false;
        //    }
        //    pathFinder.Reset(dragCanvas);
        //}

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!configuartionSpaceInitialized)
            {
                colliderContainer.GenerateAnglesArray(robot, dragCanvas, segmentsIntersector);
                IntializeConfigurationSpace();
                configurationSpace.MarkUnreachableCells(colliderContainer.anglesArray);
                PostInitialize();
                configuartionSpaceInitialized = true;
                SwitchToPathfindingButton.IsEnabled = true;
                return;
            }
            configurationSpace.Reset();
            colliderContainer.Reset(dragCanvas);
            colliderContainer.GenerateAnglesArray(robot, dragCanvas, segmentsIntersector);
            configurationSpace.MarkUnreachableCells(colliderContainer.anglesArray);
        }

        private void SwitchToPathfindingButton_Click(object sender, RoutedEventArgs e)
        {
            mouseSelector.EditorMode = false;
            EditorMode = false;
            pathFinder.Reset(dragCanvas);
            SwitchToPathfindingButton.IsEnabled = false;
            SwitchToConfigurationButton.IsEnabled = true;
            PlayButton.IsEnabled = true;
            GenerateButton.IsEnabled = false;
            ColliderButton.IsEnabled = false;
        }

        private void SwitchToConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            mouseSelector.EditorMode = true;
            EditorMode = true;
            pathFinder.Reset(dragCanvas);
            SwitchToPathfindingButton.IsEnabled = true;
            SwitchToConfigurationButton.IsEnabled = false;
            PlayButton.IsEnabled = false;
            GenerateButton.IsEnabled = true;
            ColliderButton.IsEnabled = true;

            dragCanvas.Children.Remove(mouseSelector.FirstLine);
            dragCanvas.Children.Remove(mouseSelector.SecondLine);
            dragCanvas.Children.Remove(mouseSelector.Start);
            dragCanvas.Children.Remove(mouseSelector.End);
        }

        private void CollidersButton_Click(object sender, RoutedEventArgs e)
        {
            colliderContainer.ReGenerateColliders(dragCanvas);
        }
    }
}
