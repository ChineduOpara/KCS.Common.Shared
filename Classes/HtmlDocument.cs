using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCS.Common.Shared
{
    public class MinimalHtmlDocument
    {
        public string Text { get; private set; }
        public string Title { get; set; }
        public string Body { get; set; }
        private List<string> _styles = new List<string>();
        private List<string> _scripts = new List<string>();

        public MinimalHtmlDocument()
        {
            Text = Properties.Settings.Default.MinimalHtmlDocument;
        }

        public void AddScriptLink()
        {
            throw new NotImplementedException();
        }

        public void AddScript(string script)
        {
            _scripts.Add(script);
        }

        public void AddStylesheetLink()
        {
            throw new NotImplementedException();
        }


        public void AddStyle(string prefix, string content)
        {
            _styles.Add(string.Format("{0} {{{1}}}", prefix, content));
        }

        public override string ToString()
        {
            string scripts = string.Empty;
            string styles = string.Empty;

            if (_scripts.Any())
            {
                scripts = string.Format("<script language='javascript' type='text/javascript'>{0}</script>", string.Join("\r\n", _scripts));
            }
            if (_styles.Any())
            {
                styles = string.Format("<style type='text/css'>{0}</style>", string.Join("\r\n", _styles));
            }

            Text = Text.Replace("{scripts}", scripts);
            Text = Text.Replace("{styles}", styles);
            Text = Text.Replace("{title}", Title);
            Text = Text.Replace("{body}", Body);
            return Text;
        }
    }
}
