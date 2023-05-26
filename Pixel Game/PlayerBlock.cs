using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Game
{
    internal class PlayerBlock
    {
        public int x { get; set; }
        public int y { get; set; }

        public int Health { get; set; }
        public int Health_Max { get; set; }

        public int Breath { get; set; }
        public int Breath_Max { get; set; }

        public int JumpHeight { get; set; }

        public int Momentum_Vertical { get; set; }
        public int Momentum_Horizontal { get; set; }

        public float Speed_Base { get; set; }
        public float Speed_Max { get; set; }

        public PlayerBlock()
        {
            x = 0;
            y = 0;
        }
    }
}
