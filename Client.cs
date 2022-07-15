using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellcomClient
{
    public class Client
    {
        public SerialPort port;
        byte[] outByteData,inByteData;
        private bool GotAllWord;
        StringBuilder stringBuilder;
        CancellationTokenSource cancelTokenSource;
            
        public Client()
        {
            cancelTokenSource = new CancellationTokenSource();
            stringBuilder = new StringBuilder();
            Task checkAllReceivedTask = new Task(CheckString, cancelTokenSource.Token);
            checkAllReceivedTask.Start();
            SetUpConnection();
        }

        public void SendMessage(string outData)
        {
            outByteData=System.Text.Encoding.UTF8.GetBytes(outData);
            port.Write(outByteData,0,outData.Length);
        }
        
        void SetUpConnection()
        {
            port = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
            port.NewLine = "\n";
            port.Open();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (stringBuilder)
            {
                SerialPort tempPort = (SerialPort)sender;
                inByteData = new byte[tempPort.BytesToRead];
                tempPort.Read(inByteData, 0, inByteData.Length);
                string inData = Encoding.UTF8.GetString(inByteData);
                stringBuilder.Append(inData.Split("\0").First());
            }
        }

        private void CheckString()
        {
            while (true)
            {
                string data = stringBuilder.ToString();
                if (data!="" && (data.First() > 48 || data.First() < 58))
                {
                    data = data.Remove(0, 1);
                    if ((data.Contains("Cellcom") || "DONE" == data || "BYE" == data))
                    {
                        Console.WriteLine($"Server: {stringBuilder.ToString()}");
                        stringBuilder.Clear();
                    }
                }
            }
        }
    }
}
