using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using Shouldly;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace GameServer.Test
{
    [TestClass]
    public class FileHandlerTest
    {
        [TestMethod]
        public void SaveGameFile_OnlyGameID()
        {
            try
            {
                // Generate the expected output
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("GameID");
                    writer.WriteValue("test001");
                    writer.WritePropertyName("LatestMessage");
                    writer.WriteValue("");
                    writer.WritePropertyName("Moves");
                    writer.WriteStartArray();
                    writer.WriteEnd();
                    writer.WriteEndObject();
                }
                var expectedJsonContent = sb.ToString();

                // Run the SaveGameFile method with only a gameID and compare the
                // JSON contents with the sample JSON contents
                if(!FileHandler.SaveGameFile("test001"))
                {
                    Assert.Fail("SaveGameFile() failed. Could not save the json file properly.");
                }
                var expectedFilePath = @".\save_data\test001.json";
                using (var sr = File.OpenText(expectedFilePath))
                {
                    var fileContents = sr.ReadToEnd();
                    fileContents.ShouldBe(expectedJsonContent);
                }
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [TestMethod]
        public void SaveGameFile_WithLatestMessage()
        {
            try
            {
                // Generate the expected output
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("GameID");
                    writer.WriteValue("test002");
                    writer.WritePropertyName("LatestMessage");
                    writer.WriteValue("Hello!");
                    writer.WritePropertyName("Moves");
                    writer.WriteStartArray();
                    writer.WriteEnd();
                    writer.WriteEndObject();
                }
                var expectedJsonContent = sb.ToString();

                if (!FileHandler.SaveGameFile("test002", "Hello!"))
                {
                    Assert.Fail("SaveGameFile() failed. Could not save the json file properly.");
                }
                var expectedFilePath = @".\save_data\test002.json";
                using (var sr = File.OpenText(expectedFilePath))
                {
                    var fileContents = sr.ReadToEnd();
                    fileContents.ShouldBe(expectedJsonContent);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [TestMethod]
        public void SaveGameFile_WithMessageAndMoves()
        {
            try
            {
                // Generate the expected output
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("GameID");
                    writer.WriteValue("test003");
                    writer.WritePropertyName("LatestMessage");
                    writer.WriteValue("Hello!");
                    writer.WritePropertyName("Moves");
                    writer.WriteStartArray();
                    writer.WriteValue("B2 to A3");
                    writer.WriteValue("G7 to H6");
                    writer.WriteEnd();
                    writer.WriteEndObject();
                }
                var expectedJsonContent = sb.ToString();

                List<string> moves = new List<string>();
                moves.Add("B2 to A3");
                moves.Add("G7 to H6");
                if (!FileHandler.SaveGameFile("test003", "Hello!", moves))
                {
                    Assert.Fail("SaveGameFile() failed. Could not save the json file properly.");
                }
                var expectedFilePath = @".\save_data\test003.json";
                using (var sr = File.OpenText(expectedFilePath))
                {
                    var fileContents = sr.ReadToEnd();
                    fileContents.ShouldBe(expectedJsonContent);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [TestMethod]
        public void GetGameData()
        {
            try
            {
                // Generate the expected output
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartObject();
                    writer.WritePropertyName("GameID");
                    writer.WriteValue("test004");
                    writer.WritePropertyName("LatestMessage");
                    writer.WriteValue("");
                    writer.WritePropertyName("Moves");
                    writer.WriteStartArray();
                    writer.WriteEnd();
                    writer.WriteEndObject();
                }
                var expectedJsonContent = sb.ToString();

                // Run the SaveGameFile method with only a gameID and compare the
                // JSON contents with the sample JSON contents
                if (!FileHandler.SaveGameFile("test004"))
                {
                    Assert.Fail("SaveGameFile() failed. Could not save the json file properly.");
                }

                var data = FileHandler.GetGameData("test004");
                data.ShouldBe(expectedJsonContent);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [TestMethod]
        public void GameFileExists()
        {
            try
            {
                FileHandler.SaveGameFile("test005");
                var exists = FileHandler.GameDataExists("test005");
                exists.ShouldBe(true);
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }
    }
}
