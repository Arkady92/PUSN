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
    public class ExtendedRectangle
    {
        public Rectangle rectangle { get; set; }

        public Point topLeft
        {
            get
            {
                double x, y;
                x = rectangle.Margin.Left;
                y = rectangle.Margin.Top;
                return new Point(x, y);
            }

        }

        public Point topRight
        {
            get
            {
                double x, y;
                x = rectangle.Margin.Left + rectangle.Width;
                y = rectangle.Margin.Top;
                return new Point(x, y);
            }
        }

        public Point bottomLeft
        {
            get
            {
                double x, y;
                x = rectangle.Margin.Left;
                y = rectangle.Margin.Top + rectangle.Height;
                return new Point(x, y);
            }
        }

        public Point bottomRight
        {
            get
            {
                double x, y;
                x = rectangle.Margin.Left + rectangle.Width;
                y = rectangle.Margin.Top + rectangle.Height;
                return new Point(x, y);
            }
        }



        public ExtendedRectangle(double width, double height, double left, double top, double right, double bottom)
        {
            rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Margin = new Thickness(left, top, right, bottom),
                StrokeThickness = 3,
                Stroke = Brushes.Red,
            };
            DragCanvas.SetCanBeDragged(rectangle, true);
        }

        public IList<Line> GetLines()
        {
            IList<Line> lines = new List<Line>();
            //top edge
            Line topLine = new Line();
            topLine.X1 = topLeft.X;
            topLine.Y1 = topLeft.Y;
            topLine.X2 = topRight.X;
            topLine.Y2 = topRight.Y;
            lines.Add(topLine);

            //right edge
            Line rightLine = new Line();
            rightLine.X1 = topRight.X;
            rightLine.Y1 = topRight.Y;
            rightLine.X2 = bottomRight.X;
            rightLine.Y2 = bottomRight.Y;
            lines.Add(rightLine);

            //bottom edge
            Line bottomLine = new Line();
            bottomLine.X1 = bottomRight.X;
            bottomLine.Y1 = bottomRight.Y;
            bottomLine.X2 = bottomLeft.X;
            bottomLine.Y2 = bottomLeft.Y;
            lines.Add(bottomLine);

            //bottom edge
            Line leftLine = new Line();
            leftLine.X1 = bottomLeft.X;
            leftLine.Y1 = bottomLeft.Y;
            leftLine.X2 = topLeft.X;
            leftLine.Y2 = topLeft.Y;
            lines.Add(leftLine);

            return lines;
        }
    }
}
