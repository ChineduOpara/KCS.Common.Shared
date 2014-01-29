using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.ComponentModel;

namespace KCS.Common.Shared
{
    public class CalendarEvent : DateRange
    {
        public enum PrivacyEnum
        {
            [Description("DEFAULT")]
            Default,
            [Description("PRIVATE")]
            Private,
            [Description("PUBLIC")]
            Public
        }

        public string Id { get; private set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public Uri Uri { get; set; }
        public MailAddress OrganizerMailAddress { get; set; }
        public bool Cancelled { get; set; }
        public PrivacyEnum Privacy { get; set; }

        /// <summary>
        /// Constructor accepts the ID as an int.
        /// </summary>
        /// <param name="id"></param>
        public CalendarEvent(int id) : base()
        {
            Id = id.ToString();
        }

        public CalendarEvent(string id, DateTime start, DateTime end, string summary, string description, string location) : base(start, end)
        {
            this.Id = id;
            this.Summary = summary;
            this.Description = description;
            this.Location = location;
        }
    }

    public class Calendar : List<CalendarEvent>
    {
        public string SourceID { get; private set; }
        public string DateFormat{get; private set;}

        /// <summary>
        /// Constructor.
        /// </summary>
        public Calendar(string sourceID)
        {
            SourceID = sourceID;
            //DateFormat = "yyyyMMddTHHmmssZ";
            DateFormat = "yyyyMMddTHHmmss";
        }

        /// <summary>
        /// Gets the calendar as a MemoryStream.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();

            // Create new StreamWriter to write iCalendar file.
            using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
            {
                // Write the opening line of iCalendar.
                sw.WriteLine("BEGIN:VCALENDAR");
                sw.WriteLine("VERSION:2.0");
                if (this.Where(x => x.Cancelled).Count() == 0)
                {
                    sw.WriteLine("METHOD:PUBLISH");
                }
                else
                {
                    sw.WriteLine("METHOD:CANCEL");
                }

                // Loop through rows in data source to write each event.
                foreach (CalendarEvent item in this)
                {
                    //string startString = item.Start.ToUniversalTime().ToString(DateFormat);
                    //string endString = item.End.ToUniversalTime().ToString(DateFormat);

                    string startString = item.Start.ToString(DateFormat);
                    string endString = item.End.ToString(DateFormat);

                    // Write the event start.
                    sw.WriteLine("BEGIN:VEVENT");
                    sw.WriteLine("ORGANIZER:MAILTO:" + item.OrganizerMailAddress.Address);
                    sw.WriteLine("SUMMARY:" + item.Summary);
                    sw.WriteLine("DESCRIPTION:" + item.Description);
                    sw.WriteLine("PRIORITY:5");
                    sw.WriteLine("DTSTART:" + startString);
                    sw.WriteLine("DTEND:" + endString);
                    sw.WriteLine("LOCATION:" + item.Location);
                    if (item.Uri != null)
                    {
                        sw.WriteLine("URL:" + item.Uri.ToString());
                    }

                    // Status
                    if (item.Cancelled)
                    {
                        sw.WriteLine("STATUS:CANCELLED");
                    }

                    // Unique IDs
                    sw.WriteLine("UID:" + item.Id);
                    sw.WriteLine("DTSTAMP:" + DateTime.Now.ToUniversalTime().ToString(DateFormat));

                    // Privacy setting
                    if (item.Privacy != CalendarEvent.PrivacyEnum.Default)
                    {
                        sw.WriteLine("CLASS:" + item.Privacy.GetDescription());
                    }

                    // Write the event end.                    
                    sw.WriteLine("END:VEVENT");
                }

                // Write the end line of iCalendar.
                sw.WriteLine("END:VCALENDAR");
                sw.Flush();
                return ms.ToArray();
            }
        }
    }
}
