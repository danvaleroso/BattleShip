using System.Collections.Generic;

namespace WebFormsBattleField
{
    public class Ship
    {
        public string Name { get; }
        public string Owner { get; } 
        public int LengthOfShip { get;}
        public int Hit { get; set; }
        private int Width { get; set; }
        public List<List<int>> Directions { get; set; }

        public Ship(string name, string owner, int lengthOfShip)
        {
            Name = name;
            Owner = owner;
            LengthOfShip = lengthOfShip;
            Hit = 0;
            Width = 10;

            Directions = new List<List<int>> {
                Position(1),
                Position(Width)
            };
        }

        private List<int> Position(int n)
        {
            List<int> lis = new List<int>();
            for (int i = 0; i < LengthOfShip; i++)
            {
                lis.Add(i * n);
            }
            return lis;
        }
    }
}