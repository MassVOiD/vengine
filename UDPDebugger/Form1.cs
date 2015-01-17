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

namespace UDPDebugger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Dictionary<string, string> data = new Dictionary<string, string>();
        Dictionary<string, ListViewItem> items = new Dictionary<string, ListViewItem>();

        UdpClient server;

        private void Form1_Load(object sender, EventArgs e)
        {
            server = new UdpClient(8765);
            server.BeginReceive(onReceive, null);
        }

        void onReceive(IAsyncResult iar)
        {
            IPEndPoint ipend = new IPEndPoint(IPAddress.Any, 8765);
            byte[] d = server.EndReceive(iar, ref ipend);
            server.BeginReceive(onReceive, null);
            string str = Encoding.UTF8.GetString(d);
            string[] parts = str.Split('=');
            if (data.ContainsKey(parts[0]))
            {
                data[parts[0]] = parts[1];
                items[parts[0]].Text = parts[0] + " = " + parts[1];
                listView1.Refresh();
            }
            else
            {
                data.Add(parts[0], parts[1]);
                ListViewItem item = new ListViewItem(parts[0] + " = " + parts[1]);
                items.Add(parts[0], item);
                listView1.Items.Add(item);
            }
        }
    }
}
