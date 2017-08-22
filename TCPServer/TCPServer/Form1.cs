using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Socket socket;
        private Socket clientSocket;
        private Thread thread;

        private Thread ListenThread;

        public Form1()
        {
            InitializeComponent();
            this.ServerState.Items.Clear();
            this.AcceptMessage.Text = "";
            this.SendMessage.Text = "";
            this.ServerIP.Text = "127.0.0.1";
            this.ListenPort.Text = "6888";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;

            this.Invoke((EventHandler)delegate
            {
                this.ServerState.Items.Add("服务器监听已启动!");
            });

            ListenThread = new Thread(new ThreadStart(RunListen));
            ListenThread.IsBackground = true;
            ListenThread.Start();
        }

        private void RunListen()
        {

            IPAddress ip = IPAddress.Parse(this.ServerIP.Text);
            IPEndPoint port = new IPEndPoint(ip, Int32.Parse(this.ListenPort.Text));

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(port);
            socket.Listen(10);

            clientSocket = socket.Accept();
            this.Invoke((EventHandler)delegate
            {
                this.ServerState.Items.Add("与客户 " + clientSocket.RemoteEndPoint.ToString() + "建立连接");
            });

            thread = new Thread(new ThreadStart(RunAcceptMessage));
            thread.Start();
        }

        void RunAcceptMessage()
        {
            NetworkStream netStream = new NetworkStream(clientSocket);

            while (true)
            {
                this.Invoke((EventHandler)delegate
                {
                    this.ServerState.Items.Add(DateTime.Now.Ticks);
                });
                try
                {
                    byte[] datasize = new byte[4];

                    netStream.Read(datasize, 0, 4);
                    int size = BitConverter.ToInt32(datasize, 0);

                    byte[] message = new byte[size];

                    int dataleft = size;
                    int start = 0;

                    while (dataleft > 0)
                    {
                        int recv = netStream.Read(message, start, dataleft);
                        start += recv;
                        dataleft -= recv;
                    }

                    this.Invoke((EventHandler)delegate
                    {
                        this.AcceptMessage.Rtf = System.Text.Encoding.Unicode.GetString(message);
                    });
                }
                catch
                {

                    this.Invoke((EventHandler)delegate
                    {
                        this.ServerState.Items.Add("与客户断开链接");
                    });
                    break;

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string str = this.SendMessage.Rtf;
            int i = str.Length;
            if (i == 0)
                return;
            else
            {
                i *= 2;
            }

            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(i);

            byte[] sendbytes = System.Text.Encoding.Unicode.GetBytes(str);

            try
            {
                NetworkStream netStream = new NetworkStream(clientSocket);
                netStream.Write(datasize, 0, 4);
                netStream.Write(sendbytes, 0, sendbytes.Length);
                netStream.Flush();

                this.SendMessage.Rtf = "";
            }
            catch 
            {
                MessageBox.Show("无法发送");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = true;

            try
            {
                socket.Close();
                if(clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    thread.Abort();
                }

                ListenThread.Abort();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }
    }
}
