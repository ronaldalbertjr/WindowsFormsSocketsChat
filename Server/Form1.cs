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

namespace Server
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        byte[] buffer;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Preparando o sockets
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            //Pegar Ip do usuario
            textMyIp.Text = GetLocalIp();
            textFriendIp.Text = GetLocalIp();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            epLocal = new IPEndPoint(IPAddress.Parse(textMyIp.Text), Convert.ToInt32(textMyPort.Text));
            sck.Bind(epLocal);

            epRemote = new IPEndPoint(IPAddress.Parse(textFriendIp.Text), Convert.ToInt32(textFriendPort.Text));
            sck.Connect(epRemote);

            buffer = new byte[1500];
            sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
        }

        string GetLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach(IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    return ip.ToString();
                }
            }
            return "";
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            ASCIIEncoding aEncondig = new ASCIIEncoding();
            byte[] sendingMessages = new byte[1500];
            sendingMessages = aEncondig.GetBytes(textMessages.Text);
            sck.Send(sendingMessages);
            listMessages.Items.Add("Me: " + textMessages.Text);
            textMessages.Text = "";
        }

        void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                byte[] receivedData = new byte[1500];
                receivedData = (byte[])aResult.AsyncState;
                ASCIIEncoding aEncondig = new ASCIIEncoding();
                string receivedMessages = aEncondig.GetString(receivedData);

                listMessages.Items.Add("Friend: " + receivedMessages);

                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
