using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace ServQLClient
{
    public class Client
    {
        Connection connection { get; set; }
        string[] resultType = { "OK", "ERROR", "FILE" };
        public bool isLoged { get; set; }
        private Cache.Session Session;
        public Dictionary<String,DataBase> dataBases;

        public Package.Response? Login(string user,string password)
        {
            if (connection.isReady)
            {
                Package.Login loginPackage = new Package.Login();
                loginPackage.User = user;
                loginPackage.Password = password;
                connection.Send(JsonSerializer.Serialize(loginPackage));
                
                Package.Response response = JsonSerializer.Deserialize<Package.Response>(connection.Recv());
                if (response.Result == resultType[0])
                {
                    isLoged = true;
                    Session = new Cache.Session(user,"");
                }
                return response;
            }
            return null;
        }



        public Func<string, Package.Response> NewDb, DelDb, Query,OpenDb,OpenDb2;
        public Func<Package.Response> GetDBs,GetDBsI,CloseDb,GetTables;

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
            CloseDb = () =>
            {
                Session.DataBase = null;
                return sendCommand("close", "");
            };
            GetDBs = () => sendCommand("list", "");
            GetDBsI = () =>
            {
                Package.Response response = sendCommand("list", "");
                String[][] data = response.Data;

                for (int i = 0; i < response.Data[0].Length ;i++)
                {
                    if (response.Data[0][i] == Session.DataBase) data[0][i] = "* " + data[0][i];
                }

                return response;
            };


        }
        public bool TestCon()
        {
            if (!this.connection.isReady) throw (new Exception("Connection not Ready"));
            if (!this.isLoged) throw (new Exception("Connection isnt Logged"));
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
                return response;


            }
            catch (Exception E)
            {
                return null;
            }
        }
        

        public  static void getFile(string path,byte[] fileBytes)
        {
            string[] pathList = path.Split('/');
            path = pathList[pathList.Length - 1];
            File.Create(path);
            File.WriteAllBytes(path, fileBytes);

        }
        public void LoadDataBases()
        {
            dataBases = new Dictionary<string, DataBase>();
            if (!isLoged) return;
            string[] databaseNames = GetDBs().Data[0];
            foreach(string databasename in databaseNames)
            {
                DataBase dataBase = new DataBase(this);
                dataBase.Name = databasename;
                dataBase.LoadTables();
                dataBases.Add(dataBase.Name,dataBase);

            }
        }
        public bool addDataBase(DataBase db)
        {
            if (db.Name == null || db.Name == "") return false;
            NewDb(db.Name);
            foreach(Table table in db.Tables.Values)
            {
                db.AddTable(table);
            }

            return true;
        }
        public DataBase createDataBase(string name)
        {
            DataBase db = new DataBase(this);
            db.Name = name;
            dataBases.Add(db.Name,db);
            return db;
        }

    }
}
