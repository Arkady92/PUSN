using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace RobotKinematics
{
    public class SegmentsIntersector
    {
        bool liesBetween(Point p1, Point p2, Point p3)
        {
            if (Math.Min(p1.X, p2.X) <= p3.X && p3.X <= Math.Max(p1.X, p2.X))
                return true;
            //zakładamy, że p1,p2 i p3 są współliniowe   
            else
                return false;
        }
        double crossProduct(Point p1, Point p2, Point p3)
        {
            //zwracamy iloczyn wektorowy (p2-p1) i (p3-p1)
            return (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
        }

        public bool Intersect(Line l1, Line l2)
        {  //rozważamy odcinki (p1,p2) i (p3,p4)
            Point p1 = new Point(l1.X1, l1.Y1);
            Point p2 = new Point(l1.X2, l1.Y2);

            Point p3 = new Point(l2.X1, l2.Y1);
            Point p4 = new Point(l2.X2, l2.Y2);

            double S_1 = crossProduct(p1, p3, p2);
            double S_2 = crossProduct(p1, p4, p2);
            double S_3 = crossProduct(p3, p1, p4);
            double S_4 = crossProduct(p3, p2, p4);
            if (
                 ((S_1 > 0 && S_2 < 0) || (S_1 < 0 && S_2 > 0)) &&
                 ((S_3 < 0 && S_4 > 0) || (S_3 > 0 && S_4 < 0)))
                return true;
            else if (S_1 == 0 && liesBetween(p1, p2, p3))
                return true;
            else if (S_2 == 0 && liesBetween(p1, p2, p4))
                return true;
            else if (S_3 == 0 && liesBetween(p3, p4, p1))
                return true;
            else if (S_4 == 0 && liesBetween(p3, p4, p2))
                return true;
            else return false;
        }
    }
}
