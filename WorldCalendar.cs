using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ReSource
{
    class WorldCalendar
    {       
        //current time
        [JsonIgnore]
        private int currentTick = 0;

        [JsonProperty]
        public double CurrentHour { get; private set; }

        [JsonIgnore]
        public DayPhase DayPhase{ get; private set; }

        [JsonProperty]
        public int DayOfMonth { get; private set; }

        [JsonProperty]
        public int CurrentMonth { get; private set; }

        [JsonProperty]
        public int CurrentYear { get; private set; }

        [JsonIgnore]
        public Season CurrentSeason
        {
            get
            {
                foreach (Season s in Seasons)
                {
                    if (s.isInSeason(CurrentMonth))
                    {
                        return s;
                    }
                }
                throw new Exception("Month " + Months[CurrentMonth].name + " not found in a season!");
            }
        }

        //calendar definitions
        [JsonIgnore]
        public readonly int TicksInHour = 50;

        [JsonProperty]
        public int HoursInDay { get; private set; }

        [JsonProperty]
        public List<Month> Months { get; private set; }

        [JsonProperty]
        public List<Season> Seasons { get;  private set; }

        public void Update(float dt)
        {
            currentTick += (int)Math.Floor(dt * 1000);
                        
            //jump to the next hour
            if(currentTick >= TicksInHour)
            {
                currentTick = 0;
                CurrentHour++;

                int dayPhaseEndHour = CurrentSeason.DayPhaseEndHours[DayPhase];
                if(DayPhase != DayPhase.Night)
                {
                    if (CurrentHour > dayPhaseEndHour)
                    {
                        DayPhase++;
                    }
                }
                else
                {
                    if(CurrentHour >= CurrentSeason.DayPhaseEndHours[DayPhase.Night] 
                        && CurrentHour <= CurrentSeason.DayPhaseEndHours[DayPhase.Morning])
                    {
                        DayPhase = DayPhase.Morning;
                    }
                }
             
                //jump to next day
                if(CurrentHour >= HoursInDay)
                {
                    CurrentHour = 0;
                    DayOfMonth++;
                    //jump to the next month
                    if (DayOfMonth > Months[CurrentMonth].days)
                    {
                        CurrentMonth++;
                        DayOfMonth = 1;

                        //jump to the next year
                        if (CurrentMonth >= Months.Count)
                        {
                            CurrentYear++;
                            CurrentMonth = 0;
                        }
                    }
                }
                
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write("Date: {0}:00 ({5}) on {1} {2} ({3}), Year: {4}", Math.Floor(CurrentHour), DayOfMonth, Months[CurrentMonth].name, CurrentSeason.Name, CurrentYear, DayPhase.ToString());
            }            
        }               

        //populates Season.Months from Season.MonthIds
        public void Unpack()
        {
            foreach (Season s in Seasons)
            {
                foreach (int i in s.monthIds)
                {
                    s.Months.Add(this.Months[i]);
                }
            }
        }

        public double GetCurrentHour(MapTile t)
        {
            double longitude = t.GlobalIndex.X / t.ParentMap.MapSize.X;
            return (CurrentHour + currentTick/TicksInHour)* (1 + longitude);
        }
    }

    class Month
    {
        [JsonProperty]
        public string name { get; private set; }

        [JsonProperty]
        public int days { get; private set; } //in days

        //protect this from being instantiated elsewhere
        internal Month() { }        
    }

    class Season
    {
        [JsonProperty]
        public string Name { get; private set; }

        [JsonIgnore]
        //populate this with actual month names/lengths using WorldCalender.Unpack()
        public List<Month> Months { get; internal set; }

        [JsonProperty]
        public Dictionary<DayPhase, int> DayPhaseEndHours { get; private set; }

        [JsonProperty]
        //the months in each season are stored as integers in the config file
        //(e.g. 0 is the first month in the year)
        internal List<int> monthIds;

        //protect this from being instantiated elsewhere
        internal Season() {
            //instantiate this here as we are not loading directly from config file
            Months = new List<Month>();
        }        

        internal bool isInSeason(int month)
        {
            return monthIds.Contains(month);
        }
    }

    enum DayPhase
    {
        Morning, Day, Dusk, Night
    }
}
