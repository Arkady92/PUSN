using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RobotKinematics
{
    public enum Strategy
    {
        First,
        Second,
        Third,
        Fourth
    }
    public class PathFinder
    {
        public IList<Point> path { get; set; }

        private ColliderContainer colliderContainer;
        private ConfigurationSpace configurationSpace;
        private Robot robot;
        private LinearInterpolator linearInterpolator;
        private DispatcherTimer timer;

        public Line FirstLineSolution1 { get; set; }
        public Line SecondLineSolution1 { get; set; }

        public Line FirstLineSolution2 { get; set; }
        public Line SecondLineSolution2 { get; set; }

        public PathFinder(ColliderContainer colliderContainer, ConfigurationSpace configurationSpace, Robot robot, LinearInterpolator linearInterpolator, DispatcherTimer timer)
        {
            this.colliderContainer = colliderContainer;
            this.configurationSpace = configurationSpace;
            this.robot = robot;
            this.linearInterpolator = linearInterpolator;
            this.timer = timer;
        }

        public void Reset(Canvas dragCanvas)
        {
            //clears previous configurations if any
            if (FirstLineSolution1 != null && SecondLineSolution1 != null)
            {
                dragCanvas.Children.Remove(FirstLineSolution1);
                dragCanvas.Children.Remove(SecondLineSolution1);
            }

            if (FirstLineSolution2 != null && SecondLineSolution2 != null)
            {
                dragCanvas.Children.Remove(FirstLineSolution2);
                dragCanvas.Children.Remove(SecondLineSolution2);
            }
        }

        public void PresentPossibleSolutions(Point targetPoint, SegmentsIntersector segmentsIntersector, Canvas dragCanvas)
        {
            Reset(dragCanvas);
            FindSolutions(targetPoint, segmentsIntersector, dragCanvas, Strategy.First);
            FindSolutions(targetPoint, segmentsIntersector, dragCanvas, Strategy.Second);
        }

        public void FindSolutions(Point targetPoint, SegmentsIntersector segmentsIntersector, Canvas dragCanvas, Strategy strategy)
        {
            Point targetAngles = new Point();
            if (strategy == Strategy.First)
                targetAngles = robot.CalculateInverseKinematicsFirst(targetPoint.X, targetPoint.Y);
            else if (strategy == Strategy.Second)
            {
                targetAngles = robot.CalculateInverseKinematicsSecond(targetPoint.X, targetPoint.Y);
            }

            if (Double.IsNaN(targetAngles.X) || Double.IsNaN(targetAngles.Y))
            {
                //target point is unreachable from provided position
                return;
            }

            robot.Reset(dragCanvas);
            bool isCollision = colliderContainer.CheckCollision(robot, segmentsIntersector);
            if (isCollision)
            {
                //found configuration is in collision with colliders
                return;
            }

            IList<Line> lines = robot.GetLines();
            foreach (var line in lines)
            {
                line.Stroke = Brushes.Black;
            }
            if (strategy == Strategy.First)
            {
                if (FirstLineSolution1 != null && SecondLineSolution1 != null)
                {
                    dragCanvas.Children.Remove(FirstLineSolution1);
                    dragCanvas.Children.Remove(SecondLineSolution1);
                }
                FirstLineSolution1 = lines[0];
                SecondLineSolution1 = lines[1];
            }
            else if (strategy == Strategy.Second)
            {
                if (FirstLineSolution2 != null && SecondLineSolution2 != null)
                {
                    dragCanvas.Children.Remove(FirstLineSolution2);
                    dragCanvas.Children.Remove(SecondLineSolution2);
                }
                FirstLineSolution2 = lines[0];
                SecondLineSolution2 = lines[1];
            }
            else if (strategy == Strategy.Third)
            {
                if (FirstLineSolution1 != null && SecondLineSolution2 != null)
                {
                    dragCanvas.Children.Remove(FirstLineSolution1);
                    dragCanvas.Children.Remove(SecondLineSolution2);
                }
                FirstLineSolution1 = lines[0];
                SecondLineSolution2 = lines[1];
            }
            else if (strategy == Strategy.Fourth)
            {
                if (FirstLineSolution2 != null && SecondLineSolution1 != null)
                {
                    dragCanvas.Children.Remove(FirstLineSolution2);
                    dragCanvas.Children.Remove(SecondLineSolution1);
                }
                FirstLineSolution2 = lines[0];
                SecondLineSolution1 = lines[1];
            }
            dragCanvas.Children.Add(lines[0]);
            dragCanvas.Children.Add(lines[1]);
        }

        public void MoveRobot(Point start, Point end, Strategy strategy = Strategy.First)
        {
            //ShowSelectedPosition(start);
            //ShowSelectedPosition(end);

            Point startAngles = new Point(), endAngles = new Point();

            if (strategy == Strategy.First)
            {
                startAngles = robot.CalculateInverseKinematicsFirst(start.X, start.Y);
                endAngles = robot.CalculateInverseKinematicsFirst(end.X, end.Y); 
            }
            else if (strategy == Strategy.Second)
            {
                startAngles = robot.CalculateInverseKinematicsSecond(start.X, start.Y);
                endAngles = robot.CalculateInverseKinematicsSecond(end.X, end.Y);
            }
            else if (strategy == Strategy.Third)
            {
                startAngles = robot.CalculateInverseKinematicsFirst(start.X, start.Y);
                endAngles = robot.CalculateInverseKinematicsSecond(end.X, end.Y);
            }
            else if (strategy == Strategy.Fourth)
            {
                startAngles = robot.CalculateInverseKinematicsSecond(start.X, start.Y);
                endAngles = robot.CalculateInverseKinematicsFirst(end.X, end.Y);
            }

            if (Double.IsNaN(startAngles.X) || Double.IsNaN(startAngles.Y) || Double.IsNaN(endAngles.X) || Double.IsNaN(endAngles.Y))
            {
                MessageBox.Show("Chosen points cannot be reached by robot.");
                return;
            }
            //Convert from radians to euler
            startAngles.X *= 180.0f / Math.PI;
            startAngles.Y *= 180.0f / Math.PI;
            endAngles.X *= 180.0f / Math.PI;
            endAngles.Y *= 180.0f / Math.PI;

            //transform angles to span from 0 to 360
            startAngles.X = (int)(startAngles.X + 360) % 360;
            startAngles.Y = (int)(startAngles.Y + 360) % 360;
            endAngles.X = (int)(endAngles.X + 360) % 360;
            endAngles.Y = (int)(endAngles.Y + 360) % 360;

            configurationSpace.Reset();
            configurationSpace.MarkUnreachableCells(colliderContainer.anglesArray);

            bool success = configurationSpace.FloodFill(startAngles, endAngles);
            if (success)
            {
                configurationSpace.ColorFill();
                path = configurationSpace.TraverseBack(startAngles, endAngles);
                configurationSpace.Update();
                linearInterpolator.Setup(path);
                linearInterpolator.startTime = DateTime.Now;
                timer.Start();
            }
            else if (strategy == Strategy.First)
            {
                MoveRobot(start, end, Strategy.Second);
            }
            else if (strategy == Strategy.Second)
            {
                MoveRobot(start, end, Strategy.Third);
            }
            else if (strategy == Strategy.Third)
            {
                MoveRobot(start, end, Strategy.Fourth);
            }
            else
            {
                MessageBox.Show("Chosen points cannot be reached by robot.");
            }
        }
    }
}
