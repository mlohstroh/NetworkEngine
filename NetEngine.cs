using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

namespace NetworkEngine
{
    /// <summary>
    /// Will handle all the necessary code for getting move data
    /// </summary>
    public class NetEngine
    {
        private string serverAddress;

        /// <summary>
        /// Constructor
        /// </summary>
        public NetEngine(string server) 
        {
            this.serverAddress = server;
        }

        /// <summary>
        /// Creates a new game and returns the random key
        /// </summary>
        /// <returns>The necessary random key</returns>
        public GameData CreateGame(ref Random seed, int player_id)
        {
            int randomKey = seed.Next();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/games.xml");

            //setting up necessary info for the request
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";
            request.MaximumAutomaticRedirections = 1;
            request.AllowAutoRedirect = true;

            //manually create xml file
            string data = "<?xml version=\"1.0\" encoding=\"utf-8\"?><game><random_key>" + randomKey.ToString() +"</random_key><player_id>" + player_id.ToString() + "</player_id></game>";

            byte[] bytes = Encoding.ASCII.GetBytes(data);
            try
            {
                request.ContentLength = bytes.Length;
                //write stream
                request.GetRequestStream().Write(bytes, 0, bytes.Length);

                //hopefully we've been redirected
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(reader);
                GameData gData = new GameData();
                if (xdoc.DocumentElement.Name == "game")
                {
                    for (int i = 0; i < xdoc.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode node = xdoc.DocumentElement.ChildNodes.Item(i);
                        switch (node.Name)
                        {
                            case "random-key":
                                gData.Random_Key = int.Parse(node.InnerText);
                                break;
                            case "id":
                                gData.GameID = node.InnerText;
                                break;
                            case "game-status":
                                gData.hasTwoPlayers = bool.Parse(node.InnerText);
                                break;
                        }
                    }
                }
                reader.Close();
                response.Close();
                //then stream was written sucessfully
                return gData;
            }
            catch (Exception ex)
            {
                //we won't see it but w/e i hate leaving these empty
                Console.WriteLine(ex.Message);
            }

            return new GameData();
        }

        public List<GameData> GetAllAvailableGames(int player_id)
        {
            List<GameData> allGames = new List<GameData>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/games.xml?player_id=" + player_id.ToString());
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());

                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(reader);

                if (xdoc.DocumentElement.Name == "games")
                {
                    for (int i = 0; i < xdoc.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode node = xdoc.DocumentElement.ChildNodes.Item(i);

                        GameData gData = new GameData();
                        if (node.Name == "game")
                        {
                            for (int j = 0; j < node.ChildNodes.Count; j++)
                            {
                                XmlNode gameNode = node.ChildNodes.Item(j);

                                //loop through getting all the data
                                switch (gameNode.Name)
                                {
                                    case "random_key":
                                        gData.Random_Key = int.Parse(gameNode.InnerText);
                                        break;
                                    case "id":
                                        gData.GameID = gameNode.InnerText;
                                        break;
                                    case "game-status":
                                        gData.hasTwoPlayers = bool.Parse(gameNode.InnerText);
                                        break;
                                    case "player-one-id":
                                        gData.Player_One = int.Parse(gameNode.InnerText);
                                        break;
                                    case "player-two-id":
                                        gData.Player_Two = int.Parse(gameNode.InnerText);
                                        break;
                                }
                            }

                        }
                        //add after all the data has been parsed
                        allGames.Add(gData);
                    }
                }
                return allGames;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return null;
        }

        public bool SendMoveData(GameData gData, string movedata)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/moves");
            //request.ContentType = 
            request.Method = "POST";
            
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes("game_id=" + gData.GameID + "&random_key=" + gData.Random_Key.ToString() + "&data=" + movedata);
                request.ContentLength = bytes.Length;
                request.GetRequestStream().Write(bytes, 0, bytes.Length);
                //maybe it will speed things up?    
                request.GetRequestStream().Close();
            }
            catch (Exception ex)
            {
                //we won't see it but w/e i hate leaving these empty
                Console.WriteLine(ex.Message);
            }

            //if the server doesn't accept data
            return true;
        }

        public PlayerData CreatePlayer(string name)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/player");
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.AllowAutoRedirect = true;

            //manually create xml file
            string data = "<?xml version=\"1.0\" encoding=\"utf-8\"?><name>" + name + "</name>";

            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                request.ContentLength = bytes.Length;
                request.GetRequestStream().Write(bytes, 0, bytes.Length);

                //hopefully we've been redirected
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string[] splitURI = response.ResponseUri.Segments;
                response.Close();
                return new PlayerData(name, int.Parse(splitURI[2]));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new PlayerData("Player", -1);
        }

        public PlayerData GetPlayer(int id)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/player/" + id.ToString() + ".xml");
            request.Method = "GET";

            try
            {
                //hopefully we've been redirected
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader webResponse = new StreamReader(response.GetResponseStream());
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(webResponse.ReadToEnd());
                string[] splitURI = response.ResponseUri.Segments;
                response.Close();
                return new PlayerData("Player Awesome Sauce" , int.Parse(splitURI[2]));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return new PlayerData("Player", id);
        }

        public string GetLatestMoveData(GameData gData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/games/" + gData.GameID + ".xml");

            try
            {
                //hopefully we've been redirected
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader webResponse = new StreamReader(response.GetResponseStream());
                XmlDocument xdoc = new XmlDocument();
                string xdata = webResponse.ReadToEnd();
                xdoc.LoadXml(xdata);
                response.Close();
                if (xdoc.DocumentElement.Name == "game")
                {
                    for (int i = 0; i < xdoc.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode node = xdoc.DocumentElement.ChildNodes.Item(i);
                        if (node.Name == "last-move-id")
                        {
                            return GetDataFromMoveID((int.Parse(node.InnerText)));
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private string GetDataFromMoveID(int move_id)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverAddress + "/moves/" + move_id.ToString() + ".xml");
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                StreamReader webResponse = new StreamReader(response.GetResponseStream());
                XmlDocument xdoc = new XmlDocument();
                string xdata = webResponse.ReadToEnd();
                xdoc.LoadXml(xdata);
                if (xdoc.DocumentElement.Name == "move")
                {
                    for (int i = 0; i < xdoc.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode node = xdoc.DocumentElement.ChildNodes.Item(i);
                        if (node.Name == "move-data")
                        {
                            return node.InnerText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                response.Close();
            }

            return null;
        }
    }
}
