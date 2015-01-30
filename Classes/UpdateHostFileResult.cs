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
        private List<Exception> _exceptions;

        public Exception[] Exceptions { get { return _exceptions.ToArray(); } }
        public bool HasExceptions { get { return Exceptions.Any(); } }
        public uint Written { get; internal set; }
        public uint Enabled { get; internal set; }

        public UpdateHostFileResult()
        {
            Written = 0;
            _exceptions = new List<Exception>();
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
                message = string.Format("{0} entries written, {1} exceptions.", Written, _exceptions.Count);
            }
            else
            {
                message = string.Format("{0} entries added, no exceptions.", Written);
            }
            return message;
        }
    }
}
