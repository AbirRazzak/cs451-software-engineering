using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameServer
{
    public class GameModule : NancyModule
    {
        public GameModule()
        {
            Get["/"] = parameter =>
            {
                var ip = Request.UserHostAddress;
                Console.WriteLine("\nTest signal sent to GameModule from IP: " + ip);
                return "GameModule is online.";
            };

            Post["/move/{gameID}/{move}"] = param =>
            {
                // Relay a move to the other player
                var ip = Request.UserHostAddress;
                Console.WriteLine("\nMessage signal sent to GameModule from IP: " + ip);
                Console.WriteLine("\t\t Session ID: " + param.gameID);
                Console.WriteLine("\t\t Move: " + param.move);

                // Persist all the moves messages for the current game
                var moves = new List<string>();
                var latestMsg = "";
                if (FileHandler.GameDataExists(param.gameID))
                {
                    var contents = FileHandler.GetGameData(param.gameID);
                    JObject json = JObject.Parse(contents);
                    JToken moveToken = json.GetValue("Moves");
                    moves = moveToken.ToObject<List<string>>();
                    JToken msgToken = json.GetValue("LatestMessage");
                    latestMsg = msgToken.ToString();
                }
                moves.Add(param.move);

                FileHandler.SaveGameFile(param.gameID, latestMsg, moves);
                return "Move Sent.";
            };

            Get["/getlatestmove/{gameID}"] = param =>
            {
                // Send the latest move in the game
                var ip = Request.UserHostAddress;
                Console.WriteLine("\nGet latest move signal sent to GameModule from IP: " + ip);
                Console.WriteLine("\t\t Session ID: " + param.gameID);

                // Looks at game file on server for latest message
                List<string> moves = new List<string>();
                if (FileHandler.GameDataExists(param.gameID))
                {
                    var contents = FileHandler.GetGameData(param.gameID);
                    JObject json = JObject.Parse(contents);
                    JToken moveToken = json.GetValue("Moves");
                    moves = moveToken.ToObject<List<string>>();
                }

                var latestMove = moves[moves.Count - 1];
                Console.WriteLine("\t\t LatestMove: " + latestMove);
                return latestMove;
            };

            Get["/getallmoves/{gameID}"] = param =>
            {
                // Send a list of all the moves thus far in the game
                var ip = Request.UserHostAddress;
                Console.WriteLine("\nGet all moves signal sent to GameModule from IP: " + ip);
                Console.WriteLine("\t\t Session ID: " + param.gameID);

                // Looks at game file on server for latest message
                List<string> moves = new List<string>();
                if (FileHandler.GameDataExists(param.gameID))
                {
                    var contents = FileHandler.GetGameData(param.gameID);
                    JObject json = JObject.Parse(contents);
                    JToken moveToken = json.GetValue("Moves");
                    moves = moveToken.ToObject<List<string>>();
                }

                Console.WriteLine("\t\t Moves: ");
                StringBuilder response = new StringBuilder();
                foreach(string move in moves)
                {
                    Console.WriteLine("\t\t\t " + move);
                    response.AppendLine(move);
                }
                return response.ToString();
            };
            
            Post["/msg/{gameID}/{message}"] = param =>
            {
                // Relay a message to the other player
                var ip = Request.UserHostAddress;
                Console.WriteLine("\nMessage signal sent to GameModule from IP: " + ip);
                Console.WriteLine("\t\t Session ID: " + param.gameID);
                Console.WriteLine("\t\t Message: " + param.message);

                // Persist all the moves for the current game
                List<string> moves = null;
                if (FileHandler.GameDataExists(param.gameID))
                {
                    var contents = FileHandler.GetGameData(param.gameID);
                    JObject json = JObject.Parse(contents);
                    JToken moveToken = json.GetValue("Moves");
                    moves = moveToken.ToObject<List<string>>();
                }

                FileHandler.SaveGameFile(param.gameID, param.message, moves);
                return "Message sent.";
            };

            Get["/getlatestmsg/{gameID}"] = param =>
            {
                // Send the latest message
                var ip = Request.UserHostAddress;
                Console.WriteLine("\nGet latest message signal sent to GameModule from IP: " + ip);
                Console.WriteLine("\t\t Session ID: " + param.gameID);

                // Looks at game file on server for latest message
                String latestMsg = "";
                if(FileHandler.GameDataExists(param.gameID))
                {
                    var contents = FileHandler.GetGameData(param.gameID);
                    JObject json = JObject.Parse(contents);
                    JToken moveToken = json.GetValue("LatestMessage");
                    latestMsg = moveToken.ToString();
                }

                Console.WriteLine("\t\t Latest Message: " + latestMsg);
                return latestMsg;
            };
        }
    }
}
