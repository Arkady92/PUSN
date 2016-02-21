using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotKinematics
{
    public class Component
    {
        public double Fi0 { get; set; }
        public double Fi { get; set; }
        public double l { get; set; }
        public Point Start { get; set; }

        public Line Line{
            get
            {
                double x, y;
                x = l*Math.Cos(Fi0 + Fi);
                y = l*Math.Sin(Fi0 + Fi);
                return new Line()
                {
                    X1 = Start.X,
                    Y1 = Start.Y,
                    X2 = Start.X + x,
                    Y2 = Start.Y + y,

                    StrokeThickness = 3,
                    Stroke = Brushes.Black

                };
            }
        }
    }
}
