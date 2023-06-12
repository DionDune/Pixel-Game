using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Game
{
    internal class Projectile
    {
        public int x { get; set; }
        public int y { get; set; }

        public int Momentum_Vertical { get; set; }
        public int Momentum_Horizontal { get; set; }

        public Projectile()
        {
            x = 0;
            y = 0;

            Momentum_Vertical = 0;
            Momentum_Horizontal = 0;
        }
    }
}
