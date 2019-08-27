using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    class Chat
    {
        public TextBox Chatbox { get; set; }
        public TextBox ChatInput { get; set; }
        public Button SendButton { get; set; }
        public string ServerEndpoint { get; set; }
        public string GameId { get; set; }
        private Timer MoveTimer;
        private string LastMessage = "";

        public Chat(TextBox chatbox, TextBox chatInput, Button sendButton, string gameId) {
            Chatbox = chatbox;
            ChatInput = chatInput;
            SendButton = sendButton;
            SendButton.Click += SendMessage;
            ServerEndpoint = "http://localhost:55555";
            GameId = gameId;
            //ServerEndpoint = Environment.GetEnvironmentVariable("CheckersServer");
            StartGettingMessages();
        }

        private void SendMessage(Object sender, RoutedEventArgs e)
        {
            string message = ChatInput.Text;
            if (message != "")
            {
                WebRequest wr = WebRequest.Create(ServerEndpoint + "/msg/" + GameId + "/" + message);
                wr.Method = "POST";
                wr.ContentLength = 0;
                wr.GetResponse();
                Chatbox.Text += "\n Me:" + message;
                ChatInput.Text = "";
                LastMessage = message;
            }
            
        }

        private void StartGettingMessages()
        {

            MoveTimer = new Timer(5000);
            MoveTimer.Elapsed += GetMessage;
            MoveTimer.AutoReset = true;
            MoveTimer.Enabled = true;
        }

        private void GetMessage(Object Source, ElapsedEventArgs e)
        {
            WebRequest wr = WebRequest.Create(ServerEndpoint + "/getlatestmsg/" + GameId);
            wr.Method = "GET";
            WebResponse resp = wr.GetResponse();
            string message = LastMessage;
            using (Stream dataStream = resp.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                message = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (message != LastMessage)
                {
                    Chatbox.Text += "\n Opponent:" + message;
                    LastMessage = message;
                }
            });
        }
    }
}
