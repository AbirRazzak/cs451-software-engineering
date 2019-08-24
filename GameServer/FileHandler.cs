using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace GameServer
{
    public static class FileHandler
    {
        private static readonly string SaveDirectory = @".\save_data\";

        public static bool SaveGameFile(string gameID, string msg = null, List<string> moves = null)
        {
            try
            {
                // If message or moves are null, then set them to empty values
                msg = msg ?? "";
                moves = moves ?? new List<string>();
                var json = GenerateGameFileContents(gameID, msg, moves);

                // Writes the JSON contents to a file at the file path
                // If a file with that name already exists, it is overridden
                var filePath = GenerateGameFilePath(gameID);
                Directory.CreateDirectory(SaveDirectory);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static string GetGameData(string gameID)
        {
            var filePath = GenerateGameFilePath(gameID);
            using (var sr = File.OpenText(filePath))
            {
                var fileContents = sr.ReadToEnd();
                return fileContents;
            }
        }

        public static bool GameDataExists(string gameID)
        {
            var filePath = GenerateGameFilePath(gameID);
            return File.Exists(filePath);
        }

        private static string GenerateGameFilePath(string gameID)
        {
            var name = new StringBuilder(SaveDirectory);
            name.Append(gameID);
            name.Append(".json");
            return name.ToString();
        }

        private static String GenerateGameFileContents(string gameID, string msg, List<string> moves)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            try
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartObject();
                    writer.WritePropertyName("GameID");
                    writer.WriteValue(gameID);
                    writer.WritePropertyName("LatestMessage");
                    writer.WriteValue(msg);
                    writer.WritePropertyName("Moves");
                    writer.WriteStartArray();
                    foreach(string move in moves)
                    {
                        writer.WriteValue(move);
                    }
                    writer.WriteEnd();
                    writer.WriteEndObject();
                }
                
                return sb.ToString();

            }
            catch (Exception e)
            {
                Console.WriteLine("JSON creation failed.\n" + e.ToString());
                return null;
            }
        }
    }
}
