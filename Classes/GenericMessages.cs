using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.ComponentModel.DataAnnotations;

namespace KCS.Common.Shared
{
    public interface IGenericMessage
    {
        string Source { get; set; }
        Enumerations.MessageType MessageType { get; set; }
        string MessageBody { get; set; }
        DateTime Created { get; set; }
    }

    /// <summary>
    /// A generic message class.
    /// </summary>
    public class GenericMessage : IGenericMessage
    {
        public virtual string Source { get; set; }
        public virtual Enumerations.MessageType MessageType { get; set; }
        public string MessageBody { get; set; }
        public DateTime Created { get; set; }

        public GenericMessage(string source, Enumerations.MessageType messageType, string message)
        {
            this.Source = source;
            this.MessageType = messageType;
            this.MessageBody = message;
            this.Created = DateTime.Now;
        }

        public GenericMessage(Enumerations.MessageType messageType, string message) : this(string.Empty, messageType, message)
        {
        }

        public GenericMessage(string message) : this(string.Empty, Enumerations.MessageType.Information, message)
        {
        }
    }

    public class GenericMessageQueue : Queue<GenericMessage>
    {
    }

    public class GenericMessageList : List<GenericMessage>
    {
        /// <summary>
        /// Removes the first message matching the body and returns it to the caller.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public GenericMessage Remove(string message)
        {
            var match = this.Where(x => string.Compare(x.MessageBody, message, true) == 0).FirstOrDefault();
            if (match != null)
            {
                this.Remove(match);
            }
            return match;
        }

        /// <summary>
        /// Removes the next message and returns it to the caller.
        /// </summary>
        /// <returns></returns>
        public GenericMessage Dequeue()
        {
            var match = this.First();
            this.Remove(match);
            return match;
        }

        public bool Contains(string message)
        {
            var count = this.Where(x => string.Compare(x.MessageBody, message, true) == 0).Count();
            return count > 0;
        }
    }

    ///// <summary>
    ///// List of generic messages.
    ///// </summary>
    //public class GenericMessages : List<GenericMessage>, IList<GenericMessage>
    //{
    //    /// <summary>
    //    /// Gets or sets the source of messages being added to this list.
    //    /// </summary>
    //    public string Source { get; set; }

    //    /// <summary>
    //    /// Constructor.
    //    /// </summary>
    //    /// <param name="source">Sender of the messages.</param>
    //    public GenericMessages(string source = "")
    //    {
    //        this.Source = source;
    //    }

    //    int IList<GenericMessage>.IndexOf(GenericMessage item)
    //    {
    //        return base.IndexOf(item);
    //    }

    //    void IList<GenericMessage>.Insert(int index, GenericMessage item)
    //    {
    //        base.Insert(index, item);
    //    }

    //    void IList<GenericMessage>.RemoveAt(int index)
    //    {
    //        base.RemoveAt(index);
    //    }

    //    GenericMessage IList<GenericMessage>.this[int index]
    //    {
    //        get
    //        {
    //            return base[index];
    //        }
    //        set
    //        {
    //            base[index] = value;
    //        }
    //    }

    //    void ICollection<GenericMessage>.Add(GenericMessage item)
    //    {
    //        if (string.IsNullOrEmpty(item.Source))
    //        {
    //            item.Source = this.Source;
    //        }
    //        base.Add(item);
    //    }

    //    void ICollection<GenericMessage>.Clear()
    //    {
    //        base.Clear();
    //    }

    //    bool ICollection<GenericMessage>.Contains(GenericMessage item)
    //    {
    //        return base.Contains(item);
    //    }

    //    void ICollection<GenericMessage>.CopyTo(GenericMessage[] array, int arrayIndex)
    //    {
    //        base.CopyTo(array, arrayIndex);
    //    }

    //    int ICollection<GenericMessage>.Count
    //    {
    //        get { return base.Count; }
    //    }

    //    bool ICollection<GenericMessage>.IsReadOnly
    //    {
    //        get { return false; }
    //    }

    //    bool ICollection<GenericMessage>.Remove(GenericMessage item)
    //    {
    //        return base.Remove(item);
    //    }

    //    IEnumerator<GenericMessage> IEnumerable<GenericMessage>.GetEnumerator()
    //    {
    //        return base.GetEnumerator();
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return base.GetEnumerator();
    //    }
    //}
}
