using System.Collections.Generic;

namespace WebFormsBattleField
{
    public class ShipCollection
    {
        public string OwnerName { get; }
        public List<Ship> ShipsList { get; }

        public ShipCollection(string ownerName)
        {
            OwnerName = ownerName;
            ShipsList = new List<Ship>
            {
                new Ship("Battleship-1", OwnerName, 4),
                new Ship("Cruiser-1", OwnerName, 3),
                new Ship("Cruiser-2", OwnerName, 3),
                new Ship("Destroyer-1", OwnerName, 2),
                new Ship("Destroyer-2", OwnerName, 2),
                new Ship("Destroyer-3", OwnerName, 2),
                new Ship("Submarine-1", OwnerName, 1),
                new Ship("Submarine-2", OwnerName, 1),
                new Ship("Submarine-3", OwnerName, 1),
                new Ship("Submarine-4", OwnerName, 1)
            };
        }
    }
}