using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ServQLClient
{
    public class Table
    {
        private static  class Valuetypes{
            public static String Null = "null";
            public static String Integer = "int";
            public static String Real = "real";
            public static String Text = "text";
            public static String Blob = "blob";

        }


        public string dataBase;
        private Client client;
        public String Name;
        public Dictionary<string, string> ColumnNames; //name, type
        public ObservableCollection<List<String>> Data;
       
        public bool autoUpdate = false;

        public void updateData(int max = -1)
        {
            autoUpdate = false;
            if (dataBase != "" && dataBase != null)
            {
                client.CloseDb();
                client.OpenDb(dataBase);

                Package.Response response = client.Query($"select * from {Name} { ((max == -1) ? "" : $"limit {max}")}");
                string[][] columnNames = client.Query($"pragma table_info({Name});").Data;
                ColumnNames.Clear();
                bool first = true;
                foreach (string[] columnName in columnNames)
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    ColumnNames.Add(columnName[1], columnName[2]);
                }
                if (response.Data.Length != 0 && response.Data != null)
                {

                    if (response.Data.Length >= 2)
                    {
                        Data.Clear();
                        for (int i = 1; i <= response.Data.Length - 1; i++)
                        {
                            List<string> workingList = response.Data[i].ToList();
                            Data.Add(workingList);

                        }

                    }

                }
                client.CloseDb();
            }
            autoUpdate = true;
        }

        public Table(Client client)
        {
            this.client = client;
            ColumnNames = new Dictionary<string, string>();
            Data = new ObservableCollection<List<string>>();
            Data.CollectionChanged += colectionChangedCallback;
               

        }

        private void colectionChangedCallback(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!autoUpdate) return;
            if (dataBase != "" && dataBase != null)
            {
                client.CloseDb();
                client.OpenDb(dataBase);
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (IList<string> row in e.NewItems)
                    {
                        bool first = true;
                        string order = $"insert into  {Name} values (";
                        foreach(string item in row)
                        {
                            order += (first? "" : ",") + $"\"{item}\"";
                            first = false;
                        }
                        order += ");";
                        client.Query(order);
                    }

                }
                else if(e.Action == NotifyCollectionChangedAction.Replace || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    string order = $"update {Name} set";
                    bool first = true;
                    foreach (IList<string> row in e.NewItems)
                    {
                        for (int i = 0; i <= ColumnNames.Count - 1; i ++)
                        {
                            order += $" {(first ? "" : ",")} {ColumnNames.Keys.ToArray()[i]} = \"{row[i]}\"";
                            first = false;
                        }

                    }
                    order += $" where rowid = {e.OldStartingIndex + 1} ;";
                    client.Query(order);

                   
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    client.Query($"delete from {Name} where rowid = {e.OldStartingIndex + 1}");
                    client.Query($"update  {Name} set rowid = rowid - 1 where rowid > {e.OldStartingIndex + 1};");
                }
            }
        }
    }
}
