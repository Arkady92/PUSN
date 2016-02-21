using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RobotKinematics
{
    public class ConfigurationSpace
    {
        BitmapSource bitmap;
        PixelFormat pf = PixelFormats.Rgb24;
        int width, height, rawStride;
        byte[] pixelData;
        private int[,] floodArray;
        private Image image;

        public int FoundPathLength { get; set; }

        public void Init(Image image)
        {
            this.image = image;

            width = (int)image.Width;
            height = (int)image.Height;
            rawStride = (width * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * height];

            floodArray =new int[width,height];
        }

        public void SetPixel(int x, int y, Color c)
        {
            int xIndex = x * 3;
            int yIndex = y * rawStride;
            pixelData[xIndex + yIndex] = c.R;
            pixelData[xIndex + yIndex + 1] = c.G;
            pixelData[xIndex + yIndex + 2] = c.B;
        }

        public void Update()
        {
            bitmap = BitmapSource.Create(width, height,
                96, 96, pf, null, pixelData, rawStride);
            image.Source = bitmap;
        }

        public void MarkUnreachableCells(bool[,] anglesArray)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    if (!anglesArray[i, j])
                    {
                        SetPixel(i,j,Colors.Red);
                        floodArray[i, j] = -1;
                    }
                }
            Update();
        }

        private int trimIndex(int index)
        {
            return (index + width)%width;
        }

        public void Reset()
        {
            pixelData = new byte[rawStride * height];
            floodArray = new int[width, height];
            Update();
        }

        //public void Remove()
        //{
        //    pixelData = new byte[rawStride * height];
        //    floodArray = new int[width, height];
        //    Update();
        //}

        private Color GetColorGradient(int iteration)
        {
            int left = 0;
            int right = 255;
            int targetLen = right - left;

            int targetVal = (int)((double) targetLen/(double) FoundPathLength*iteration);
            byte targetV = (byte) targetVal;

            return Color.FromRgb(0, targetV, 0);
        }

        public void ColorFill()
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    //Sprawdzamy czy zostało odwiedzone
                    if (floodArray[i, j] > 0)
                    {
                        SetPixel(i, j, GetColorGradient(floodArray[i, j]));
                    }
                }
        }

        private bool checkNextPoint(Point p, Point parent, Point endPoint, IDictionary<Point, Point> activePoints)
        {
            //sprawdzamy czy punkt nie jest przeszkodą i czy nie został jeszcze odwiedzony
            if (floodArray[(int) p.X, (int) p.Y] == 0)
            {
                activePoints.Add(new KeyValuePair<Point, Point>(p,p));
                int parentIteration = floodArray[(int) parent.X, (int) parent.Y];
                floodArray[(int)p.X, (int)p.Y] = parentIteration + 1;
                //SetPixel((int)p.X, (int)p.Y, Colors.Green);
                if (p == endPoint)
                {
                    FoundPathLength = parentIteration + 1;
                    return true;
                }
            }
            return false;
        }

        private bool checkNextTraversePoint(Point p, Point parent)
        {
            int parentIteration = floodArray[(int)parent.X, (int)parent.Y];
            int iteration = floodArray[(int) p.X, (int) p.Y];
            if (parentIteration == iteration + 1)
            {
                SetPixel((int) p.X, (int) p.Y, Colors.White);
                return true;
            }
            return false;
        }

        public IList<Point> TraverseBack(Point start, Point end)
        {
            Point activePoint = new Point(end.X, end.Y);
            IList<Point> inversePath = new List<Point>();
            inversePath.Add(activePoint);
            while (activePoint != start)
            {
                int left = trimIndex((int)activePoint.X - 1);
                int right = trimIndex((int)activePoint.X + 1);
                int top = trimIndex((int)activePoint.Y + 1);
                int bottom = trimIndex((int)activePoint.Y - 1);

                //Rozpatrzmy sąsiadów
                var topCell = new Point(activePoint.X, top);
                var bottomCell = new Point(activePoint.X, bottom);
                var leftCell = new Point(left, activePoint.Y);
                var rightCell = new Point(right, activePoint.Y);

                if (checkNextTraversePoint(topCell, activePoint))
                    activePoint = topCell;
                else if (checkNextTraversePoint(bottomCell, activePoint))
                    activePoint = bottomCell;
                else if (checkNextTraversePoint(leftCell, activePoint))
                    activePoint = leftCell;
                else if (checkNextTraversePoint(rightCell, activePoint))
                    activePoint = rightCell;

                inversePath.Add(activePoint);
            }

            inversePath.Reverse();

            return inversePath;
        }

        public bool FloodFill(Point start, Point end)
        {
            IDictionary<Point, Point> activePoints = new Dictionary<Point, Point>();
            activePoints.Add(new KeyValuePair<Point, Point>(start, start));
            floodArray[(int) start.X, (int) start.Y] = 1;
            int iter = 0;
            bool sentinel = false;
            while (activePoints.Count > 0)
            {
                IDictionary<Point, Point> newActivePoints = new Dictionary<Point, Point>();
                foreach (var point in activePoints)
                {
                    var activePoint = point.Value;

                    int left = trimIndex((int)activePoint.X - 1);
                    int right = trimIndex((int)activePoint.X + 1);
                    int top = trimIndex((int)activePoint.Y + 1);
                    int bottom = trimIndex((int)activePoint.Y - 1);

                    //Rozpatrzmy sąsiadów
                    var topCell = new Point(activePoint.X, top);
                    var bottomCell = new Point(activePoint.X, bottom);
                    var leftCell = new Point(left, activePoint.Y);
                    var rightCell = new Point(right, activePoint.Y);

                    sentinel = sentinel || checkNextPoint(topCell, activePoint, end, newActivePoints);
                    sentinel = sentinel || checkNextPoint(bottomCell, activePoint, end, newActivePoints);
                    sentinel = sentinel || checkNextPoint(leftCell, activePoint, end, newActivePoints);
                    sentinel = sentinel || checkNextPoint(rightCell, activePoint, end, newActivePoints);

                    iter++;
                    if (sentinel)
                    {
                        break;
                    }
                }

                activePoints = newActivePoints;
                if (sentinel)
                {
                    break;
                }
            }
            if (sentinel)
                return true;
            else
                return false;
        }
    }
}
