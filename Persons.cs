using System;
using System.Windows.Media.Media3D;

namespace URodziny
{
    internal class Persons
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDay { get; set; }
        public int Year { get; set; }
        public TimeSpan diference { get; set; }

        public Persons(String FirstName, string LastName, DateTime BirthDay, DateTime StartDate, int Year) { 
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.BirthDay = BirthDay;
            this.Year = Year;
            var BirthDay1 = DateTime.Parse(BirthDay.Day + "." + BirthDay.Month + "." + Year);
            diference = BirthDay1.Subtract(StartDate);
            
        }

        public string print()
        {
            return (this.BirthDay.Day + "." + this.BirthDay.Month + " ("+ this.diference.TotalDays +" Dzień) - " + this.FirstName + " " + this.LastName);
        }
    }
}