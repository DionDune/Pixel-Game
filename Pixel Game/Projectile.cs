using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Game
{
    internal class Projectile
    {
        public int Health { get; set; }

        public string type { get; set; }

        public int x { get; set; }
        public int y { get; set; }

        public int Momentum_Vertical { get; set; }
        public int Momentum_Horizontal { get; set; }


        public float float_X { get; set; }
        public float float_Y { get; set; }

        public float angle_X { get; set; }
        public float angle_Y { get; set; }

        public float Momentum_Power { get; set; }

        public Projectile()
        {
            type = "standard";

            x = 0;
            y = 0;

            Momentum_Vertical = 0;
            Momentum_Horizontal = 0;
        }
    }
}
