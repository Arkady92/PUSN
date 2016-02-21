using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotKinematics
{
    public class MouseSelector
    {
        public Ellipse Start { get; set; }
        public Ellipse End { get; set; }

        public Point StartPosition { get; set; }
        public Point EndPosition { get; set; }

        public Line FirstLine { get; set; }
        public Line SecondLine { get; set; }

        public bool EditorMode { get; set; }

        private static bool selector = true;
        private int size = 10;

        public void Set(Point p, Point originRelative, Canvas canvas)
        {
            if (selector || EditorMode)
            {
                if (Start != null)
                    canvas.Children.Remove(Start);
                Start = new Ellipse()
                {
                    Fill = new SolidColorBrush() {Color = Colors.Yellow},
                    StrokeThickness = 2,
                    Stroke = Brushes.Black,
                    Width = size,
                    Height = size,
                    Margin = new Thickness(p.X - size / 2.0f, p.Y - size/2.0f,0, 0 )
                };
                StartPosition = originRelative;
                canvas.Children.Add(Start);
            }
            else
            {
                if (End != null)
                    canvas.Children.Remove(End);
                End = new Ellipse()
                {
                    Fill = new SolidColorBrush() { Color = Colors.Turquoise },
                    StrokeThickness = 2,
                    Stroke = Brushes.Black,
                    Width = 10,
                    Height = 10,
                    Margin = new Thickness(p.X - size / 2.0f, p.Y - size/2.0f,0, 0 )
                };
                EndPosition = originRelative;
                canvas.Children.Add(End);
            }
            selector = !selector;
        }
    }
}
