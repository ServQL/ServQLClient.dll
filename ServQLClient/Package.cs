using System;
using System.Collections.Generic;
using System.Text;

namespace ServQLClient
{
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
            public String[][] Data { get; set; }
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
