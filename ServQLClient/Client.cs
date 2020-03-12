using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace SQLiteCLIENT
{
    public class Client
    {
        Connection connection { get; set; }
        string[] resultType = { "OK", "ERROR", "FILE" };
        public bool isLoged { get; set; }
        private Cache.Session Session;

        public Package.Response? Login(string user,string password)
        {
            if (connection.isReady)
            {
                Package.Login loginPackage = new Package.Login();
                loginPackage.User = user;
                loginPackage.Password = password;
                connection.Send(JsonSerializer.Serialize(loginPackage));
                
                Package.Response response = JsonSerializer.Deserialize<Package.Response>(connection.Recv());
                if (response.Result == resultType[0]) isLoged = true;
                return response;
            }
            return null;
        }

        public string[] splitOut(string OutD)
        {
            string[] result = OutD.Split(':');
            if (result.Length <= 1) throw (new Exception("Invalid string"));
            string[] Message = new string[result.Length - 1];
            Message = result.Skip(1).ToArray();
            result = new string[2] { result[0], string.Join(" ",Message) };
            return result;
        }

        public Func<string, Package.Response> NewDb, DelDb, Query,OpenDb,OpenDb2;
        public Func<Package.Response> GetDBs,CloseDb,GetTables;

        public Client(Connection connection)
        {
            this.connection = connection;
            NewDb = arg => sendCommand("new", arg);
            DelDb = arg => sendCommand("del", arg);
            Query = arg => sendCommand("exec", arg);
            //OpenDb = arg => sendCommand("open", arg);

            OpenDb = arg => { 
                Package.Response result = sendCommand("open", arg);
                if (result.Result == resultType[0]) Session.DataBase = arg;
                return result;
            };

            GetTables = () => sendCommand("exec", $"SELECT name FROM sqlite_master WHERE type = 'table'");
            CloseDb = () => sendCommand("close", "");
            GetDBs = () => sendCommand("list", "");

        }
        public bool TestCon()
        {
            if (!this.connection.isReady) throw (new Exception("Connection not Ready"));
            if (!this.connection.isLoged) throw (new Exception("Connection isnt Logged"));
            Package.Response response = sendCommand("test");
            if (response.Result == "OK" && response.Message == "OK") return true;
            return false;
            
        }
        public Package.Response sendCommand(string cmd, string args = "")
        {
            try
            {
                Package.Response response;
                string RAWrequest,RAWresponse;
                Package.Request requestPackage = new Package.Request();
                requestPackage.Command = cmd;
                requestPackage.Args = args.Split();
                RAWrequest = JsonSerializer.Serialize(requestPackage) + "\n";
                connection.Send(RAWrequest);
                RAWresponse = connection.Recv();
                response = JsonSerializer.Deserialize<Package.Response>(RAWresponse);
                if (response.Result == resultType[0]) return response;
                else return null;

            }
            catch (Exception E)
            {
                return null;
            }
        }
        public List<List<string>> formatTable(Package.Response response)
        {
            List<List<string>> table = new List<List<string>>();
            

            foreach (string protoRow in response.Data)
            {
                List<string> Row = protoRow.Split(',').ToList<string>();
                table.Add(Row);
            }

            return table;
        }

        public  static void getFile(string path,byte[] fileBytes)
        {
            string[] pathList = path.Split('/');
            path = pathList[pathList.Length - 1];
            File.Create(path);
            File.WriteAllBytes(path, fileBytes);

        }

        public class Package
        {
            [Serializable]
            public class Request
            {
                public String Command { get; set; }
                public String[] Args { get; set; }
                public String Hash { get; set; }

            }

            [Serializable]
            public class Response
            {
                public String Result { get; set; }
                public String Message { get; set; }
                public String[] Data { get; set; }
            }

            public Request GenPackage(string command, String[] args, String hash)
            {
                Request package = new Request();
                package.Command = command;
                package.Args = args;
                package.Hash = hash;
                return package;
            }

            [Serializable]
            public class Login
            {
                public int Type { get; set; }
                public String version { get; set; }
                public String User { get; set; }
                public String Password { get; set; }

            }
        }

    }
}
