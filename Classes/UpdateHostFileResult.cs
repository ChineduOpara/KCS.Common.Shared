using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class UpdateHostFileResult
    {
        private List<DnsHostEntry> _added;
        private List<DnsHostEntry> _removed;
        private List<DnsHostEntry> _updated;
        private List<Exception> _exceptions;

        public DnsHostEntry[] Added { get { return _added.ToArray(); } }
        public DnsHostEntry[] Removed { get { return _removed.ToArray(); } }
        public DnsHostEntry[] Updated { get { return _updated.ToArray(); } }

        public Exception[] Exceptions { get { return _exceptions.ToArray(); } }
        public bool HasExceptions { get { return Exceptions.Any(); } }
        public bool HasChanges { get { return (_added.Count + _removed.Count + _updated.Count) > 0; } }

        public UpdateHostFileResult()
        {
            _added = new List<DnsHostEntry>();
            _removed = new List<DnsHostEntry>();
            _updated = new List<DnsHostEntry>();
            _exceptions = new List<Exception>();
        }

        public void AddAdded(DnsHostEntry entry)
        {
            _added.Add(entry);
        }

        public void AddDeleted(DnsHostEntry entry)
        {
            _removed.Add(entry);
        }

        public void AddModified(DnsHostEntry entry)
        {
            _updated.Add(entry);
        }

        public void AddException(Exception ex)
        {
            string message = "Error while updating Hosts file.";
            var newEx = new Exception(message, ex);
            _exceptions.Add(newEx);
        }

        public string GetErrorMessages()
        {
            return string.Join("\r\n\r\n", _exceptions.Select(x => x.GetAllExceptionsString()).ToArray());
        }

        public override string ToString()
        {
            string message = string.Empty;
            if (HasExceptions)
            {
                message = string.Format("{0} entries added, {1} removed, {2} updated, {3} exceptions.", _added.Count, _removed.Count, _updated.Count, _exceptions.Count);
            }
            else
            {
                message = string.Format("{0} entries added, {1} removed, {2} updated, no exceptions.", _added.Count, _removed.Count, _updated.Count);
            }
            return message;
        }
    }
}
