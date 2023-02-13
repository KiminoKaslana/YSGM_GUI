using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace YSGM_GUI
{
    public class SQLManager
    {
        public static SQLManager Instance = new();
        private SQLManager()
        {
            
        }

        public XmlNode? Parse(string xml)
        {
            XmlDocument doc = new();
            doc.LoadXml(xml);
            return doc.SelectSingleNode("resultset");
        }

        private string Query(string db, string query)
        {
            return SSHManager.Instance.Execute($"mysql -p {db} -e \"{query}\" --binary-as-hex --xml");
        }

        public XmlNode Execute(string db, string query)
        {
            return Parse(Query(db, query))!;
        }
    }
}
