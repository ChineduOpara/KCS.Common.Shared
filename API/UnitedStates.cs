using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCS.Common.Shared
{
    public class UnitedStates : List<UnitedState>
    {
        private static UnitedStates _instance = null;
        private static object _lock = new object();

        /// <summary>
        /// Static instance.
        /// </summary>
        public static UnitedStates Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new UnitedStates();
                    }
                }
                return _instance;
            }
        }

        public UnitedState this[string abbreviation]
        {
            get
            {
                return this.Where(x => string.Compare(x.Abbreviation, abbreviation, true) == 0).FirstOrDefault();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private UnitedStates()
        {
            Initialize();
        }

        private void Initialize()
        {
            Add("AL", "Alabama");
            Add("AK", "Alaska");
            Add("AZ", "Arizona");
            Add("AR", "Arkansas");
            Add("CA", "California");
            Add("CO", "Colorado");
            Add("CT", "Connecticut");
            Add("DE", "Delaware");
            Add("DC", "District of Columbia");
            Add("FL", "Florida");
            Add("GA", "Georgia");
            Add("HI", "Hawaii");
            Add("ID", "Idaho");
            Add("IL", "Illinois");
            Add("IN", "Indiana");
            Add("IA", "Iowa");
            Add("KS", "Kansas");
            Add("KY", "Kentucky");
            Add("LA", "Louisiana");
            Add("ME", "Maine");
            Add("MD", "Maryland");
            Add("MA", "Massachusetts");
            Add("MI", "Michigan");
            Add("MN", "Minnesota");
            Add("MS", "Mississippi");
            Add("MO", "Missouri");
            Add("MT", "Montana");
            Add("NE", "Nebraska");
            Add("NV", "Nevada");
            Add("NH", "New Hampshire");
            Add("NJ", "New Jersey");
            Add("NM", "New Mexico");
            Add("NY", "New York");
            Add("NC", "North Carolina");
            Add("ND", "North Dakota");
            Add("OH", "Ohio");
            Add("OK", "Oklahoma");
            Add("OR", "Oregon");
            Add("PA", "Pennsylvania");
            Add("RI", "Rhode Island");
            Add("SC", "South Carolina");
            Add("SD", "South Dakota");
            Add("TN", "Tennessee");
            Add("TX", "Texas");
            Add("UT", "Utah");
            Add("VT", "Vermont");
            Add("VA", "Virginia");
            Add("WA", "Washington");
            Add("WV", "West Virginia");
            Add("WI", "Wisconsin");
            Add("WY", "Wyoming");
        }

        private void Add(string abbreviation, string name)
        {
            var state = new UnitedState(abbreviation, name);
            base.Add(state);
        }                
    }

    /// <summary>
    /// A single state.
    /// </summary>
    public class UnitedState
    {
        public string Abbreviation { get; set; }
        public string Name { get; set; }

        public UnitedState(string abbreviation, string name)
        {
            Abbreviation = abbreviation;
            Name = name;
        }
    }
}
