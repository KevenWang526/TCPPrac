using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPClient
{
    public partial class Form1 : Form
    {
        private Socket socket;
        private Thread thread;

        public Form1()
        {
            InitializeComponent();

            this.SendMessage.Text = "";
            this.AcceptMessage.Text = "";

            this.ClientState.Items.Clear();
            this.ServerIP.Text = "127.0.0.1";
            this.ListenPort.Text = "6888";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPAddress ip = IPAddress.Parse(this.ServerIP.Text);
            IPEndPoint port = new IPEndPoint(ip, int.Parse(this.ListenPort.Text));

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(port);
            }
            catch
            {
                MessageBox.Show("与服务器连接失败");
                return;
            }

            this.button1.Enabled = false;
            this.ClientState.Items.Add("与服务器连接成功");
            thread = new Thread(new ThreadStart(RunAcceptMessage));
            thread.Start();
        }

        private void RunAcceptMessage()
        {
            NetworkStream stream = new NetworkStream(socket);

            while (true)
            {
                try
                {
                    byte[] datasize = new byte[4];
                    stream.Read(datasize, 0, 4);

                    int size = BitConverter.ToInt32(datasize, 0);

                    byte[] message = new byte[size];

                    int dataleft = size;
                    int start = 0;

                    while (dataleft > 0)
                    {
                        int recv = stream.Read(message, start, dataleft);

                        start += recv;
                        dataleft -= recv;
                    }
                    this.Invoke((EventHandler)delegate
                    {
                        this.AcceptMessage.Rtf = System.Text.Encoding.Unicode.GetString(message);
                    });
                }
                catch (Exception)
                {
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
                NetworkStream netStream = new NetworkStream(socket);
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
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    thread.Abort();
                }
                this.ClientState.Items.Add("与主机断开连接");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            this.button1.Enabled = true;

        }
    }
}
