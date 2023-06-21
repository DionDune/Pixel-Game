namespace Pixel_Game
{
    public class Particle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string ParticleType { get; set; }
        public int Tag { get; set; }

        public Particle(int x, int y, string particleType, int tag = 0)
        {
            X = x;
            Y = y;
            Tag = tag;
            ParticleType = particleType;
        }
    }
}
