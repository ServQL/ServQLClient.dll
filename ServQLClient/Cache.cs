using System;
using System.Linq;
using System.Collections.Generic;

public class Cache
{


	public class Session
	{
		Owner owner { get; set; }
		Dictionary<String,Database> Databases = new Dictionary<string, Database>();
		public String DataBase ;


		void AddDataBase(string dbName)
		{
			if (!Databases.ContainsKey(dbName))
			{
				Database database = new Database()
				{
					owner = owner,
					Name = dbName
				};

				Databases.Add(dbName, database);
			}


		}

		Session(int id,string UserName,string Hash)
		{


			owner = new Owner(Hash)
			{
				ID = id,
				Name = UserName
			};

		}
	}

		
	[Serializable]
	public class Database
	{
		public Owner owner { get; set; }
		public string Name { get; set; }
		public List<string> Tables { get; set; }
	}

	[Serializable]
	public class Table
	{
		List<string> Titles { get; set; }
		List<List<string>> Data { get; set; }
	}

	[Serializable]
	public class Owner
	{
		public int ID;
		public string Name;
		private string Hash;

		public Owner(string _Hash)
		{
			Hash = _Hash;
		}
	}
}
