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
        }

        private Browser CreateNewDefaultGameModule()
        {
            return new Browser(with => with.Module(new GameModule()));
        }
    }
}
