
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace KCS.Common.Shared
{
    public class Countries :  List<Country>
    {
        private static Countries _instance = null;
        private static object _lock = new object();

        /// <summary>
        /// Static instance.
        /// </summary>
        public static Countries Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Countries();
                    }
                }
                return _instance;
            }
        }

        public Country this[string iso3166Code]
        {
            get
            {
                return this.Where(x => string.Compare(x.ISO3166Code, iso3166Code, true) == 0).FirstOrDefault();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private Countries()
        {
            Initialize();
        }

        private void Initialize()
        {
            var ass = Assembly.GetExecutingAssembly();
            var types = ass.GetManifestResourceNames();
            using (Stream stream = ass.GetManifestResourceStream("KCS.Common.Shared.API.Countries.xml"))
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                var nodes = doc.SelectNodes("//country");
                foreach (XmlNode xn in nodes)
                {
                    XmlNode nameNode = xn.SelectSingleNode("name");
                    XmlNode iso3166Node = xn.SelectSingleNode("iso3166Code");
                    Add(nameNode.InnerText, iso3166Node.InnerText);
                }                
            }
        }

        private void Add(string name, string iso3166Code)
        {
            var c = new Country(name, iso3166Code);
            base.Add(c);
        }        
    }

    public class Country
    {
        public string ISO3166Code { get; set; }
        public string Name { get; set; }

        public Country(string name, string iso3166Code)
        {
            ISO3166Code = iso3166Code;
            Name = name;
        }
    }
}
