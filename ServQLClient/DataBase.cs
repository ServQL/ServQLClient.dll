using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServQLClient
{
    public class DataBase
    {
        private Client client;
        public string Name;
        public Dictionary<String,Table> tables;


        public void LoadTables()
        {
            client.CloseDb();
            client.OpenDb(Name);
            string[][] tableNames = client.GetTables().Data.Skip(1).ToArray();
            foreach(string[] tableName in tableNames)
            {
                Table table = new Table(this.client);
                table.Name = tableName[0];
                table.dataBase = this.Name;
                table.updateData(10);
                tables.Add(table.Name,table);

                

            }
            client.CloseDb();
        }

        public void AddTable(Table table) {
            client.CloseDb();
            client.OpenDb(Name);
            String order = $"create table {table.Name} (";
            foreach(string value in table.ColumnNames.Keys)
            {
                order += $"{value} {table.ColumnNames[value]},";
            }
            order.Remove(order.Length - 1, 1);
            order += ");";
            client.Query(order);
            foreach(List<String> row in table.Data)
            {
                order = $"inser into {Name} values (";
                foreach(string item in row)
                {
                    order += $"\"{item}\",";
                }
                order.Remove(order.Length - 1, 1);
                order += ");";
                client.Query(order);
            }
            client.CloseDb();

        }

        public DataBase(Client client)
        {
            this.client = client;
            tables = new Dictionary<string, Table>();
        }

    }
}
