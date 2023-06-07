using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Game
{
    internal class EntityBlock
    {
        public int x { get; set; }
        public int y { get; set; }

        public string Direction { get; set; }

        public int Health { get; set; }
        public int Health_Max { get; set; }

        public int Breath { get; set; }
        public int Breath_Max { get; set; }

        public int JumpHeight { get; set; }

        public int Momentum_Vertical { get; set; }
        public int Momentum_Horizontal { get; set; }

        public float Speed_Base { get; set; }
        public float Speed_Max { get; set; }

        public EntityBlock()
        {
            x = 0;
            y = 0;

            Direction = "Still";

            Health = 100;
            Health_Max = 100;

            Breath = 1000;
            Breath_Max = 1000;

            JumpHeight = 12;

            Speed_Base = 0.25F;
            Speed_Max = 0.5F;
        }
    }
}
