using System;
using Nancy;

namespace Server
{
    public class GameModule : NancyModule
    {
        public GameModule()
        {
            Get("/", x => {
                Console.WriteLine("Someone has connected.");
                return "Hello World.";
            });

            Get("/greet/{name}", x => {
                Console.WriteLine("Hi " + x.name);
                return string.Concat("Hello ", x.name);
            });
        }
    }
}
