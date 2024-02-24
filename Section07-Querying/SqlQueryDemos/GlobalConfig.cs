//using System.Collections.Generic;
//using System.Configuration;

//namespace JsonSqlQuery
//{
//	public static class GlobalConfig
//    {
//        public static IDictionary<string, string> ConnectionStrings { get; set; }

//        static GlobalConfig()
//        {
//            ConnectionStrings = new Dictionary<string, string>();
//            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
//            {
//                if (css.Name.ToUpper() == "LOCALSQLSERVER")
//                {
//                    continue;
//                }
//                ConnectionStrings.Add(css.Name, css.ConnectionString);
//            }
//        }

//    }
//}
