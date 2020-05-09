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
using System.IO;
using System.IO.Compression;
using SmazSharp;
namespace ServQLClient
{
    
    public class Connection
    {
        public string ip { get; set; }
        public int Port = 124;
        public string clientVersion = "0.30b";
        TcpClient client { get; set; }
        public SslStream stream { get; set; }
        public bool isReady = false;

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
            version = null;
            stream = null;

        }
       
        public void Send(string cmd)
        {
            if (!isReady) throw (new Exception("Connection not Ready"));
            if (version == null) GetVersion();
            cmd += '\n';

            byte[] data = Encoding.UTF8.GetBytes(cmd);
            //byte[] compressedData = Compress(data);
            //byte[] compressedData = Encoding.UTF8.GetBytes(Convert.ToBase64String(data));
            byte[] realdata = new byte[data.Length + 1];
            realdata[0] = 1;
            data.CopyTo(realdata, 1);
            stream.Write(realdata,0,realdata.Length);            
        }
       

        //public string Recv()
        //{
        //    if (!isReady) throw (new Exception("Connection not Ready"));
        //    string result = "";
        //    byte[] dataResult = new byte[client.ReceiveBufferSize];
        //    stream.Read(dataResult, 0, dataResult.Length);
        //    stream.Flush();
        //    if (dataResult[0] == 1 )
        //    {
        //        dataResult = dataResult.Skip(1).ToArray();
        //        //result = Encoding.UTF8.GetString(Decompress(dataResult));
        //        result = Smaz.Decompress(dataResult);

        //    }
        //    else
        //    {
        //        //result = Encoding.UTF8.GetString(Decompress(dataResult));
        //        result = Smaz.Decompress(dataResult);

        //    }
        //    result = result.TrimEnd('\0');
        //    result = result.TrimEnd('\n');
        //    return result;

        //}

        public string Recv()
        {
            if (!isReady) throw (new Exception("Connection not Ready"));
            string result = "";
            string recived;
            byte[] dataResult;
            do
            {
                dataResult = new byte[10100];
                stream.Read(dataResult, 0, dataResult.Length);
                if (dataResult[0] == 1)
                {
                    dataResult = dataResult.Skip(1).ToArray();
                }
                result += Encoding.UTF8.GetString(dataResult).Replace("\0", "");
               // result += Encoding.UTF8.GetString(Convert.FromBase64String(recived));

            } while (dataResult[9999] != 0);

            return result;
        }


        public byte[] Compress(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }
            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();
            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);
            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return gzBuffer;


        }


        public byte[] Decompress(byte[] gzBuffer)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return buffer;
            }
        }

    }

}
