using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkEngine
{
    public struct PlayerData
    {
        public string PlayerName;
        public int ID;

        public PlayerData(string name, int id)
        {
            this.ID = id;
            this.PlayerName = name;
        }
    }
}
