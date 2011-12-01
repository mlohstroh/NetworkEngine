using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkEngine
{
    public struct GameData
    {
        public int Random_Key;
        public string GameID;
        public bool hasTwoPlayers;
        public int Player_One;
        public int Player_Two;

        public GameData(int rand_key, string id, bool twoPlayers, int player_one, int player_two)
        {
            this.Random_Key = rand_key;
            this.hasTwoPlayers = twoPlayers;
            this.GameID = id;
            this.Player_One = player_one;
            this.Player_Two = player_two;
        }
    }
}
