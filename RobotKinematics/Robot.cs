using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace RobotKinematics
{
    public class Robot
    {
        public Component first;
        public Component second;

        public double Fi1 { get; set; }
        public double Fi2 { get; set; }

        private int l1 = 200, l2 = 100;

        
        public Robot(Canvas canvas)
        {
            double fi1 = 30;
            double fi2 = 20;
            UpdateAngles(fi1, fi2);
            Reset(canvas);
        }

        public void UpdateAngles(double fi1, double fi2)
        {
            Fi1 = Math.PI * fi1 / 180.0f;
            Fi2 = Math.PI * fi2 / 180.0f;
        }

        public void Reset(Canvas canvas)
        {
            Reset(canvas, l1, l2);
        }

        public void Reset(Canvas canvas, int l1, int l2)
        {
            this.l1 = l1;
            this.l2 = l2;
            first = new Component()
            {
                Fi0 = 0,
                Fi = Fi1,
                l = l1,
                Start = new Point(canvas.Width / 2, canvas.Height / 2)//new Point(0,0)
            };
            second = new Component()
            {
                Fi0 = Fi1,
                Fi = Fi2,
                l = l2
            };
        }

        public IList<Line> GetLines()
        {
            IList<Line> lines = new List<Line>();
            Line firstLine = first.Line;
            second.Start = new Point(firstLine.X2, firstLine.Y2);
            Line secondLine = second.Line;

            lines.Add(firstLine);
            lines.Add(secondLine);
            return lines;
        }

        public void Update(Point p, Canvas canvas, MouseSelector mouseSelector)
        {
            UpdateAngles(p.X, p.Y);
            Reset(canvas);


            if (mouseSelector.FirstLine != null & mouseSelector.SecondLine != null)
            {
                canvas.Children.Remove(mouseSelector.FirstLine);
                canvas.Children.Remove(mouseSelector.SecondLine);
            }
            IList<Line> lines = GetLines();
            mouseSelector.FirstLine = lines[0];
            mouseSelector.SecondLine = lines[1];
            canvas.Children.Add(lines[0]);
            canvas.Children.Add(lines[1]);
        }

        public Point CalculateInverseKinematicsFirst(double x, double y)
        {
            double l1 = first.l;
            double l2 = second.l;
            Fi2 = Math.Acos( (x*x + y*y - l1*l1 - l2*l2) / (2*l1*l2));
            Fi1 = -Math.Asin( (l2*Math.Sin(Fi2)) / Math.Sqrt(x*x + y*y) ) + Math.Atan2(y,x);
            return new Point(Fi1, Fi2);
        }

        public Point CalculateInverseKinematicsSecond(double x, double y)
        {
            double l1 = first.l;
            double l2 = second.l;
            Fi2 = -Math.Acos((x * x + y * y - l1 * l1 - l2 * l2) / (2 * l1 * l2));
            Fi1 = Math.Asin((l2 * Math.Sin(-Fi2)) / Math.Sqrt(x * x + y * y)) + Math.Atan2(y, x);
            return new Point(Fi1, Fi2);
        }

    }
}
