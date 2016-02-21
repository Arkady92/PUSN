using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RobotKinematics
{
    public class LinearInterpolator
    {

        private Point[] nodes;

        public DateTime startTime { get; set; }
        public DateTime stopTime { get; set; }
        public TimeSpan timeDelay { get; set; }

        public void Setup(IList<Point> path)
        {
            nodes = path.ToArray();
            timeDelay = new TimeSpan();
        }

        public Point GetValue(double normalizedTime)
        {

            int x0, x1;
            double x, y0, y1, fi1, fi2;
            int maxIndex = nodes.Length - 1;
            x = normalizedTime*maxIndex;
            x0 = (int)(normalizedTime*maxIndex);
            x1 = x0 + 1;

            double diff = Math.Abs(x - x0);

            y0 = nodes[x0].X;
            y1 = nodes[x1].X;
            y1 = CalibrateAngle(y0, y1);
            fi1 = y0 + (y1 - y0)*diff;

            y0 = nodes[x0].Y;
            y1 = nodes[x1].Y;
            y1 = CalibrateAngle(y0, y1);
            fi2 = y0 + (y1 - y0) * diff;

            return new Point(fi1, fi2);
        }

        private double CalibrateAngle(double startAngle, double endAngle)
        {
            int diff = (int)(startAngle - endAngle);
            int div = diff / 360;
            endAngle += div * 360;
            if (endAngle > startAngle)
            {
                var d1 = endAngle - startAngle;
                var d2 = -(endAngle - startAngle - 360);
                if (d1 > d2)
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
    }
}
