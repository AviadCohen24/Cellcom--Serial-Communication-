using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CellcomServer
{
    public class Server
    {
        public SerialPort port;
        List<string> ClientsId;
        System.Timers.Timer timer;
        CancellationTokenSource CancelToken;
        public Task TaskPrint;
        public bool _continue;
        public Byte[] outDataByte;
        public CancellationTokenSource cancellationTokenSource;
        Task SendEverySecondTask;

        public Server()
        {
            ClientsId = new List<string>();
            cancellationTokenSource = new CancellationTokenSource();
            SetUpConnection();
            SendEverySecondTask = Task.Run(SendEverySecondAsync, cancellationTokenSource.Token);
            Console.WriteLine("Server started");
        }

        private async void SendEverySecondAsync()
        {
            while (_continue)
            {
                foreach (var client in ClientsId)
                {
                    string outData = $"{client}Cellcom";
                    Console.WriteLine(outData);
                    outDataByte = Encoding.UTF8.GetBytes(outData);
                    port.Write(outDataByte, 0, outDataByte.Length);
                    Console.WriteLine($"Length: {outDataByte.Length}");
                }
                await Task.Delay(1000);
            }
        }
      
        void SetUpConnection()
        {
            port = new SerialPort("COM2", 9600, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
            _continue = true;
            port.Open();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort tempPort = (SerialPort)sender;
            byte[] inDataByte=new byte[tempPort.BytesToRead];
            tempPort.Read(inDataByte,0, tempPort.BytesToRead);
            string inData = System.Text.Encoding.UTF8.GetString(inDataByte);
            ReceivedAction(inData) ;
        }

        private void ReceivedAction(string data)
        {
            string id = data.Split("-").First();
            string action = data.Split("-").Last();
            switch (action)
            {
                case "NEW":
                    CreateNewConversation(id);
                    break;
                case "JOIN":
                    JoinClientToService(id);
                    break;
                case "STOP":
                    CloseConversation(id);
                    break;
                case "QUIT":
                    QuitAndClosePort();
                    break;
            }
        }

        private void QuitAndClosePort()
        {
            _continue = false;
            //cancellationTokenSource.Cancel();
            port.Close();
        }

        private void CloseConversation(string id)
        {
            ClientsId.Remove(id);
            string outData = $"{id}BYE";
            outDataByte=Encoding.UTF8.GetBytes(outData);
            port.Write(outDataByte,0,outData.Length);
        }

        private void JoinClientToService(string id)
        {
            CancelToken = new CancellationTokenSource();
            TaskPrint=Task.Run(printOneToTen,CancelToken.Token);
            string outData = $"{id}DONE";
            outDataByte = Encoding.UTF8.GetBytes(outData);
            port.Write(outDataByte, 0, outData.Length);
        }

        private void printOneToTen()
        {
            for (int i = 1; i < 10; i++)
            {
                Console.Write($"{i}...");
            }
            Console.WriteLine("10");
            CancelToken.Cancel();
        }

        private void CreateNewConversation(string id)
        {
            ClientsId.Add(id);
        }
    }
}
