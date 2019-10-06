using System;
using System.Collections.Generic;
using System.Linq;

namespace GoryMoon.StreamEngineer.Data
{
    public abstract class BaseAction
    {
        public string Message { get; set; }
        public List<Condition> Conditions { get; set; }
        
        public abstract void Execute(Data data);

        protected int GetEventValue(string property, int defaultVal, Data data)
        {
            if ("$event$".Equals(property))
            {
                return data.Amount;
            }

            if (property != null)
            {
                try
                {
                    return Convert.ToInt32(property);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            return defaultVal;
        }
        
        public bool Test(Data eventData)
        {
            return Conditions.Count <= 0 || Conditions.Select(c => c.Test(eventData)).Aggregate((b, b1) => b || b1);
        }
        
        public override string ToString()
        {
            return $"{nameof(Message)}: {Message}, {nameof(Conditions)}: {string.Join(",", Conditions)}";
        }
    }
}