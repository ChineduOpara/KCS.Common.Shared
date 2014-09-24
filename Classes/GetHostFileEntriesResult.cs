using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class GetHostFileEntriesResult
    {
        private List<DnsHostEntry> _entries;
        private List<Exception> _exceptions;

        public DnsHostEntry[] Entries { get { return _entries.ToArray(); } }
        public Exception[] Exceptions { get { return _exceptions.ToArray(); } }        

        public GetHostFileEntriesResult()
        {
            _entries = new List<DnsHostEntry>();
            _exceptions = new List<Exception>();
        }

        public void AddEntry(DnsHostEntry entry)
        {
            _entries.Add(entry);
        }

        public void AddException(Exception ex)
        {
            _exceptions.Add(ex);
        }
    }
}
