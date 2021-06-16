namespace PlayerLib
{
    public class Position
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Angle { get; set; }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Angle: {Angle} ";
        }

        public Position(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
            this.Angle = 0;
        }
        public Position((double, double) position)
        {
            this.X = position.Item1;
            this.Y = position.Item2;
            this.Angle = 0;
        }
        public Position((double, double) position, double angle)
        {
            this.X = position.Item1;
            this.Y = position.Item2;
            this.Angle = angle;
        }
        public Position(double X, double Y, double angle)
        {
            this.X = X;
            this.Y = Y;
            this.Angle = angle;
        }
    }
}
