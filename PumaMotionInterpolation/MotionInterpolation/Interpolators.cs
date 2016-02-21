using System;
using System.Windows.Media.Media3D;

namespace MotionInterpolation
{
    public class RealTimeInterpolator
    {

        public double GetValue(double normalizedTime, double startVal, double endVal)
        {
            endVal = CalibrateEulerAngle(startVal, endVal);
            double diff = endVal - startVal;

            return startVal + diff*normalizedTime;
        }

        public Vector3D GetVectorValue(double normalizedTime, Vector3D startVal, Vector3D endVal)
        {
            Vector3D diff = endVal - startVal;

            return startVal + diff * normalizedTime;
        }

        private double CalibrateEulerAngle(double startAngle, double endAngle)
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

    public class SphericalLinearInterpolator
    {
        private Quaternion startQuaternion;
        private Quaternion endQuaternion;

        public void SetupInterpolator(Quaternion startQuaternion, Quaternion endQuaternion)
        {
            this.startQuaternion = startQuaternion;
            this.endQuaternion = endQuaternion;
        }

        public void CalculateCurrentQuaternion(ref Quaternion currentQuaternion, double timeFactor)
        {
            double dotProduct = startQuaternion.X * endQuaternion.Y + startQuaternion.Y * endQuaternion.Y
                + startQuaternion.Z * endQuaternion.Z + startQuaternion.W * endQuaternion.W;

            var a = Math.Acos(dotProduct);
            a = Math.Abs(a);

            var firstFactor = Math.Sin((1 - timeFactor) * a) / Math.Sin(a);
            var secondFactor = Math.Sin(timeFactor * a) / Math.Sin(a);
            var x = firstFactor * startQuaternion.X + secondFactor * endQuaternion.X;
            var y = firstFactor * startQuaternion.Y + secondFactor * endQuaternion.Y;
            var z = firstFactor * startQuaternion.Z + secondFactor * endQuaternion.Z;
            var w = firstFactor * startQuaternion.W + secondFactor * endQuaternion.W;
            currentQuaternion = new Quaternion(x, y, z, w);
            currentQuaternion.Normalize();
        }
    }

    public class LinearInterpolator
    {
        private double StartAngleR;
        private double StartAngleP;
        private double StartAngleY;
        private double EndAngleR;
        private double EndAngleP;
        private double EndAngleY;

        private double StartPositionX;
        private double StartPositionY;
        private double StartPositionZ;
        private double EndPositionX;
        private double EndPositionY;
        private double EndPositionZ;

        private Quaternion startQuaternion;
        private Quaternion endQuaternion;

        public void SetupInterpolator(double StartAngleR, double StartAngleP, double StartAngleY, double EndAngleR,
            double EndAngleP, double EndAngleY, double StartPositionX,
         double StartPositionY,
         double StartPositionZ,
         double EndPositionX,
         double EndPositionY,
         double EndPositionZ,
            Quaternion startQuaternion,
            Quaternion endQuaternion
            )
        {
            this.StartAngleP = StartAngleP;
            this.StartAngleR = StartAngleR;
            this.StartAngleY = StartAngleY;

            this.EndAngleR = EndAngleR;
            this.EndAngleP = EndAngleP;
            this.EndAngleY = EndAngleY;

            this.StartPositionX = StartPositionX;
            this.StartPositionY = StartPositionY;
            this.StartPositionZ = StartPositionZ;

            this.EndPositionX = EndPositionX;
            this.EndPositionY = EndPositionY;
            this.EndPositionZ = EndPositionZ;

            this.startQuaternion = startQuaternion;
            this.endQuaternion = endQuaternion;
        }

        public void CalculateCurrentAngle(ref Rotation currentRotation, double normalizedTime)
        {
            currentRotation.R = StartAngleR + normalizedTime * (EndAngleR - StartAngleR);
            currentRotation.P = StartAngleP + normalizedTime * (EndAngleP - StartAngleP);
            currentRotation.Y = StartAngleY + normalizedTime * (EndAngleY - StartAngleY);
        }

        public void CalculateCurrentPosition(ref Position currentPosition, double normalizedTime)
        {
            currentPosition.X = StartPositionX + normalizedTime * (EndPositionX - StartPositionX);
            currentPosition.Y = StartPositionY + normalizedTime * (EndPositionY - StartPositionY);
            currentPosition.Z = StartPositionZ + normalizedTime * (EndPositionZ - StartPositionZ);
        }

        public void CalculateCurrentQuaternion(ref Quaternion currentQuaternion, double timeFactor)
        {
            var x = startQuaternion.X * (1 - timeFactor) + endQuaternion.X * timeFactor;
            var y = startQuaternion.Y * (1 - timeFactor) + endQuaternion.Y * timeFactor;
            var z = startQuaternion.Z * (1 - timeFactor) + endQuaternion.Z * timeFactor;
            var w = startQuaternion.W * (1 - timeFactor) + endQuaternion.W * timeFactor;
            currentQuaternion = new Quaternion(x, y, z, w);
            currentQuaternion.Normalize();
        }
    }
}
