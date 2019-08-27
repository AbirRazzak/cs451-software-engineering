using System;
using GameServer;

using Nancy;
using Nancy.Testing;

using Shouldly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameServer.Test
{
    [TestClass]
    public class GameModuleTest
    {
        [TestMethod]
        public void CheckServerStatus()
        {
            var browser = CreateNewDefaultGameModule();

            var result = browser.Get("/", with => {
                with.HttpRequest();
            });

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            result.Body.AsString().ShouldBe("GameModule is online.");
        }

        [TestMethod]
        public void TestMessageRoutes()
        {
            var browser = CreateNewDefaultGameModule();

            // Test /msg logic
            var msgSend = browser.Post("/msg/abc/Hello", with => {
                with.HttpRequest();
            });

            msgSend.StatusCode.ShouldBe(HttpStatusCode.OK);
            msgSend.Body.AsString().ShouldBe("Message sent.");

            // Test /getlatestmsg logic
            var msgRecieve = browser.Get("/getlatestmsg/abc", with => {
                with.HttpRequest();
            });

            msgRecieve.StatusCode.ShouldBe(HttpStatusCode.OK);
            msgRecieve.Body.AsString().ShouldBe("Hello");
        }

        [TestMethod]
        public void TestMoveRoutes()
        {
            // Create a random gameID to prevent previous moves from messing up the test
            var gameID = GenerateRandomString();
            var browser = CreateNewDefaultGameModule();

            // Test /move logic
            var moveSend = browser.Post("/move/" + gameID +"/50,41", with => {
                with.HttpRequest();
            });

            moveSend.StatusCode.ShouldBe(HttpStatusCode.OK);
            moveSend.Body.AsString().ShouldBe("Move sent.");

            // Test /getlatestmove logic
            var moveLatest = browser.Get("/getlatestmove/" + gameID, with => {
                with.HttpRequest();
            });

            moveLatest.StatusCode.ShouldBe(HttpStatusCode.OK);
            moveLatest.Body.AsString().ShouldBe("50,41");

            // Test /getlatestmove logic
            browser.Post("/move/" + gameID + "/23,32", with => {
                with.HttpRequest();
            });

            var moveAll = browser.Get("/getallmoves/" + gameID, with => {
                with.HttpRequest();
            });

            moveAll.StatusCode.ShouldBe(HttpStatusCode.OK);
            moveAll.Body.AsString().ShouldBe("50,41\r\n23,32\r\n");
        }

        private Browser CreateNewDefaultGameModule()
        {
            return new Browser(with => with.Module(new GameModule()));
        }

        private string GenerateRandomString()
        {
            // Code block from https://stackoverflow.com/a/1344258
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
    }
}
