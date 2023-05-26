using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Game
{
    public class UIItem
    {
        public string Type { get; set; }

        public bool Active { get; set; }
        public int Active_Amount { get; set; }

        public int Size_X { get; set; }
        public int Size_Y { get; set; }

        public int Size_Sub_X { get; set; }
        public int Size_Sub_Y { get; set; }

        public int Location_X { get; set; }
        public int Location_Y { get; set; }


        public UIItem()
        {
            Type = "Checkbox";

            Active = false;
            Active_Amount = 0;

            Size_X = 50;
            Size_Y = 50;

            Size_Sub_X = 40;
            Size_Sub_Y = 40;

            Location_X = 0;
            Location_Y = 0;
        }
    }
}
