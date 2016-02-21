using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotKinematics
{
    public class ColliderContainer
    {
        public IList<ExtendedRectangle> Colliders;
        private IList<Line> lineContainer; 
        public bool[,] anglesArray { get; set; }
        private const int N = 4;
        int maxAngle = 360;
        private Random rand;
        public ColliderContainer()
        {
            Colliders = new List<ExtendedRectangle>();
            rand = new Random();
            anglesArray = new bool[maxAngle, maxAngle];
            lineContainer = new List<Line>();
        }

        public void ReGenerateColliders(Canvas canvas)
        {
            foreach (var item in Colliders)
            {
                canvas.Children.Remove(item.rectangle);
            }
            Colliders.Clear();
            GenerateColliders(canvas);
        }

        public void GenerateColliders(Canvas canvas)
        {
            double left, right, top, bottom, width, height;
            var coords = new double[N, 2] { { 100, 100 }, { 350, 100 }, { 100, 350 }, { 350, 350 } };
            for (int i = 0; i < rand.Next(4) + 2; i++)
            {
                width = rand.Next(150) + 50;
                height = rand.Next(150) + 50;
                right = bottom = 0;
                left = rand.Next(50) + coords[i%4, 0];
                top = rand.Next(50) + coords[i%4, 1];
                var extendedRectangle = new ExtendedRectangle(width, height, left, top, right, bottom);
                extendedRectangle.rectangle.Fill = Brushes.Red;
                Colliders.Add(extendedRectangle);
                canvas.Children.Add(extendedRectangle.rectangle);
            }
        }

        public void Reset(Canvas Canvas)
        {
            foreach (var line in lineContainer)
            {
                Canvas.Children.Remove(line);
            }
        }

        public bool CheckCollision(Robot robot, SegmentsIntersector segmentsIntersector)
        {
            Line firstLine = robot.GetLines()[0];
            Line secondLine = robot.GetLines()[1];
            bool sentinel = false;
            foreach (var extendedRectangle in Colliders)
            {
                foreach (var line in extendedRectangle.GetLines())
                {
                    bool intersectionWithFirst = segmentsIntersector.Intersect(firstLine, line);
                    bool intersectionWithSecond = segmentsIntersector.Intersect(secondLine, line);

                    if (intersectionWithFirst || intersectionWithSecond)
                    {
                        sentinel = true;
                        break;
                    }
                }
                if (sentinel)
                {
                    break;
                }
            }
            if (sentinel)
            {
                return true;
            }
            return false;
        }

        public void GenerateAnglesArray(Robot robot, Canvas Canvas, SegmentsIntersector segmentsIntersector)
        {

            int positiveCounter = 0;
            for (int i = 0; i < maxAngle; i++)
                for (int j = 0; j < maxAngle; j++)
                {
                    robot.UpdateAngles(i, j);
                    robot.Reset(Canvas);
                    Line firstLine = robot.GetLines()[0];
                    Line secondLine = robot.GetLines()[1];
                    bool sentinel = false;
                    foreach (var extendedRectangle in Colliders)
                    {
                        foreach (var line in extendedRectangle.GetLines())
                        {
                            bool intersectionWithFirst = segmentsIntersector.Intersect(firstLine, line);
                            bool intersectionWithSecond = segmentsIntersector.Intersect(secondLine, line);

                            if (intersectionWithFirst || intersectionWithSecond)
                            {
                                sentinel = true;
                                break;
                            }
                        }
                        if (sentinel)
                        {
                            break;
                        }
                    }
                    if (sentinel)
                    {
                        anglesArray[i, j] = false;
                    }
                    else
                    {
                        anglesArray[i, j] = true;
                        positiveCounter++;
                        if (i%4 == 0 && j % 2 == 0)
                        {
                            //firstLine.Stroke = new SolidColorBrush() {Color = Color.FromArgb(50, 0, 255, 0)};
                            secondLine.Stroke = new SolidColorBrush() {Color = Color.FromArgb(50, 0, 0, 200)};
                            //lineContainer.Add(firstLine);
                            secondLine.X1 = secondLine.X2 - 1;
                            secondLine.Y1 = secondLine.Y2 - 1;
                            lineContainer.Add(secondLine);
                            Canvas.Children.Add(secondLine);
                        }
                    }
                }
            int d = 10;
        }
    }
}
