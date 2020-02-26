using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SQLiteCLIENT
{
    
    public class Connection
    {
        public string ip { get; set; }
        public int Port = 124;
        public string clientVersion = "0.30b";
        TcpClient client { get; set; }
        public SslStream stream { get; set; }
        public bool isReady = false;
        public bool isLoged = false;

        string version { get; set; }

        public  Connection(string ip)
        {
            this.ip = ip;
        }

        public TcpClient Open()
        {
            TcpClient client = new TcpClient(this.ip, this.Port);
            this.client = client;
            stream = new SslStream(client.GetStream(),false,new RemoteCertificateValidationCallback(ValidateServerCertificate),null);
            stream.AuthenticateAsClient(ip);
            this.isReady = true;
            stream.Flush();
            return client;
        }


        

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate,
        X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
        
            return true;
        }

        public string GetVersion()
        {
            if (version == null)
            {
                if (stream != null)
                {
                    version = Recv();
                    return version;
                }
            }
            return null;
        }

        

        public void Close(TcpClient client = null)
        {
            if (client == null)
            {
                if (this.client == null) return;
                client = this.client;
            }
            client.Close();
            isReady = false;
            isLoged = false;
            version = null;
            stream = null;

        }
       
        public void Send(string cmd)
        {
            if (!isReady) throw (new Exception("Connection not Ready"));
            if (version == null) GetVersion();
            byte[] data = Encoding.UTF8.GetBytes('\u0001' + cmd + '\n');
            stream.Write(data,0,data.Length);            
        }
       

        public string Recv()
        {
            if (!isReady) throw (new Exception("Connection not Ready"));
            string result;
            byte[] dataResult = new byte[client.ReceiveBufferSize];
            stream.Read(dataResult, 0, dataResult.Length);
            stream.Flush();
            result = Encoding.UTF8.GetString(dataResult);
            result = result.TrimEnd('\0');
            result = result.TrimEnd('\n');
            if (result.Length == 0) return "";
            if (result[0] == '\u0001' && result.Length == 1)
            {
                result = null;
                dataResult = new byte[client.ReceiveBufferSize];
                stream.Read(dataResult, 0, dataResult.Length);
                result = Encoding.UTF8.GetString(dataResult);
                result = result.TrimEnd('\0');
                result = result.TrimEnd('\n');
            }
            else if (result[0] == '\u0001')
            {
                result = result.TrimStart('\u0001');
            }
            return result;

        }

        public byte[] Recv(int byteLength) //TODO: update to accept telomer
        {
            if (!isReady) throw (new Exception("Connection not Ready"));
            byte[] dataRecv = new byte[byteLength];
            stream.Read(dataRecv, 0, dataRecv.Length);
            stream.Flush();

            return dataRecv;
        }

    }

}
