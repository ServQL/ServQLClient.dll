using System;
using System.Linq;
using System.Collections.Generic;

namespace ServQLClient { 

	public class Cache
	{

		public class Session
		{
			Owner owner { get; set; }

			public String DataBase ;




			public Session(string UserName,string Hash)
			{


				owner = new Owner(Hash)
				{
					Name = UserName
				};

			}
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
}
