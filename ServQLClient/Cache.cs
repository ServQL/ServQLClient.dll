using System;
using System.Linq;
using System.Collections.Generic;

namespace ServQLClient { 

	public class Cache
	{
		private List<String> cacheStorage { get; set; }
		private string key { get; set; }
		public string DataBase { get; internal set; }

		public void AddCache(DataBase dataBase) { }
		public void AddCache(Table table) {
			AddCache(table.dataBase, table.Data.ToList());
		}
		public void AddCache(string tableName,string[][] data) { 
			
		}
		public void AddCache(string tableName, List<List<String>> data)
		{

		}



		public Cache(string user,string password)
		{

		}

	}
}
