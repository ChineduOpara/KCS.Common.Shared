using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class SaveIISWebsiteBindingsResult
    {
        public enum BindingAction
        {
            [Description("adding")]
            Adding,
            [Description("removing")]
            Removing,
            [Description("updating")]
            Updating
        }

        private List<Binding> _added;
        private List<Binding> _removed;
        private List<Binding> _updated;
        private List<Exception> _exceptions;

        public Binding[] Added { get { return _added.ToArray(); } }
        public Binding[] Removed { get { return _removed.ToArray(); } }
        public Binding[] Updated { get { return _updated.ToArray(); } }

        public Exception[] Exceptions { get { return _exceptions.ToArray(); } }
        public bool HasExceptions { get { return Exceptions.Any(); } }
        public bool HasChanges { get { return (_added.Count + _removed.Count + _updated.Count) > 0; } }

        public SaveIISWebsiteBindingsResult()
        {
            _added = new List<Binding>();
            _removed = new List<Binding>();
            _updated = new List<Binding>();
            _exceptions = new List<Exception>();
        }

        public void AddAdded(Binding binding)
        {
            _added.Add(binding);
        }

        public void AddRemoved(Binding binding)
        {
            _removed.Add(binding);
        }

        public void AddUpdated(Binding binding)
        {
            _updated.Add(binding);
        }

        public void AddException(Exception ex)
        {
            string message = "Error while committing changes to IIS";
            var newEx = new Exception(message, ex);
            _exceptions.Add(newEx);
        }

        public void AddException(Binding binding, BindingAction action, Exception ex)
        {
            string message = string.Format("Error while {0} binding \"{1}\" to website \"{2}\"", action, binding.Host, "[TODO: website]");
            var newEx = new Exception(message, ex);
            _exceptions.Add(newEx);
        }

        public void AddException(DnsHostEntry binding, BindingAction action, Exception ex)
        {
            string message = string.Format("Error while {0} binding \"{1}\" to website \"{2}\"", action, binding.DnsSafeDisplayString, binding.Website.Name);
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
                message = string.Format("{0} bindings added, {1} removed, {2} updated, {3} exceptions.", _added.Count, _removed.Count, _updated.Count, _exceptions.Count);
            }
            else
            {
                message = string.Format("{0} bindings added, {1} removed, {2} updated, no exceptions.", _added.Count, _removed.Count, _updated.Count);
            }
            return message;
        }
    }
}
