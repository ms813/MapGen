using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ReSource
{
    public class WorldCalendar
    {       
        //current time
        private int currentTick = 0;

        [JsonProperty]
        public int DayOfMonth { get; private set; }

        [JsonProperty]
        public int CurrentMonth { get; private set; }

        [JsonProperty]
        public int CurrentYear { get; private set; }

        //calendar definitions
        public readonly int TicksInHour = 1000;

        [JsonProperty]
        public int HoursInDay { get; private set; }

        [JsonProperty]
        public List<Month> Months { get; private set; }

        [JsonProperty]
        public List<Season> Seasons { get;  private set; }

        public void Update(float dt)
        {
            currentTick += (int)Math.Floor(dt * 1000);

            //jump to the next day
            if(currentTick > TicksInHour)
            {
                currentTick = 0;
                DayOfMonth++;               

                //jump to the next month
                if (DayOfMonth > Months[CurrentMonth].days)
                {
                    CurrentMonth++;
                    DayOfMonth = 1;

                    //jump to the next year
                    if(CurrentMonth >= Months.Count)
                    {
                        CurrentYear++;
                        CurrentMonth = 0;
                    }
                }
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("Date: {0} {1}, {2}", DayOfMonth, Months[CurrentMonth].name, CurrentYear);
            }            
        }      

        public Season CurrentSeason()
        {
            foreach(Season s in Seasons)
            {
                if(s.isInSeason(CurrentMonth))
                {
                    return s;
                }
            }
            throw new Exception("Month " + Months[CurrentMonth].name +  " not found in a season!");
        }

        //populates Season.Months from Season.MonthIds
        public void Unpack()
        {
            foreach (Season s in Seasons)
            {
                foreach (int i in s.monthIds)
                {
                    s.months.Add(this.Months[i]);
                }
            }
        }
    }

    public class Month
    {
        [JsonProperty]
        public string name { get; private set; }

        [JsonProperty]
        public int days { get; private set; } //in days

        //protect this from being instantiated elsewhere
        internal Month() { }        
    }

    public class Season
    {
        [JsonProperty]
        public string name { get; private set; }

        [JsonIgnore]
        //populate this with actual month names/lengths using WorldCalender.Unpack()
        public List<Month> months { get; internal set; }

        [JsonProperty]
        //the months in each season are stored as integers in the config file
        //(e.g. 0 is the first month in the year)
        internal List<int> monthIds;

        //protect this from being instantiated elsewhere
        internal Season() {
            //instantiate this here as we are not loading directly from config file
            months = new List<Month>();
        }

        internal bool isInSeason(int month)
        {
            return monthIds.Contains(month);
        }
    }
}
