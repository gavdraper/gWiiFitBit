using System;

namespace gWiiFitBit.Lib
{
    public class Weight
    {
        private float weight;
        public int Stone
        {
            get
            {
                var stone = weight * 0.157473;
                return (int)Math.Floor((double)stone);
            }
        }
        public int Pounds
        {
            get
            {
                var stone = weight * 0.157473;
                var dec = stone - (int)Math.Floor((double)stone);
                var pounds = dec * 14;
                return (int)Math.Floor((double)pounds);
            }
        }

        public float TotalStone
        {
            get{
                var stone = weight * 0.157473;
                return (float)stone;
            }
        }

        public Weight(float kg)
        {
            weight = kg;
        }

        public override string ToString()
        {
            return string.Format("{0}st {1}lb", Stone, Pounds);    
        }
    }
}
