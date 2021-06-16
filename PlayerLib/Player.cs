using System;

namespace PlayerLib
{
    public class Player
    {
        public Position Position { get; set; }

        public Player (Position position)
        {
            Position = position;
        }
    }
}
