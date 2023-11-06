using WorkdayNet.Models;

namespace WorkdayNet;

public class WorkdayCalendar : IWorkdayCalendar
{
    private WorkdayTimeControl WorkdayStart;
    private WorkdayTimeControl WorkdayEnd;
    private readonly HashSet<DateTime> _holidays = new HashSet<DateTime>();
    private readonly List<(int month, int day)> _recurringHolidays = new List<(int month, int day)>();

    public void SetHoliday(DateTime date)
    {
        _holidays.Add(date.Date);
    }

    public void SetRecurringHoliday(int month, int day)
    {
        _recurringHolidays.Add((month, day));
    }

    public void SetWorkdayStartAndStop(int startHours, int startMinutes, int stopHours, int stopMinutes)
    {
        WorkdayStart = new WorkdayTimeControl(startHours, startMinutes);
        WorkdayEnd = new WorkdayTimeControl(stopHours, stopMinutes);
    }

    public DateTime GetWorkdayIncrement(DateTime startDate, decimal incrementInWorkdays)
    {
        if (incrementInWorkdays == 0)
        {
            return startDate;
        }
        incrementInWorkdays = Math.Round(incrementInWorkdays, 6);
        int multiplier = incrementInWorkdays > 0 ? 1 : -1;

        int wholeDaysToAdd = (int)Math.Floor(Math.Abs(incrementInWorkdays)) * multiplier;
        decimal fractionOfDay = incrementInWorkdays % 1;

        DateTime adjustedDate = startDate;
        for (int i = 0; i < Math.Abs(wholeDaysToAdd); i++)
        {
            adjustedDate = GetValidWorkday(adjustedDate, multiplier);
        }

        if (fractionOfDay != 0)
        {
            var startDay = new DateTime(adjustedDate.Year, adjustedDate.Month, adjustedDate.Day, WorkdayStart.Hour, WorkdayStart.Minutes, 0);
            var endDay = new DateTime(adjustedDate.Year, adjustedDate.Month, adjustedDate.Day, WorkdayEnd.Hour, WorkdayEnd.Minutes, 0);
            adjustedDate = AddWorkdayTime(adjustedDate, fractionOfDay, multiplier, startDay, endDay);
        }

        return adjustedDate;
    }

    private DateTime AddWorkdayTime(DateTime date, decimal fractionOfDay, int multiplier, DateTime startDay, DateTime endDay)
    {
        var totalDailyJourneyInSeconds = (decimal)(endDay - startDay).TotalSeconds;
        decimal secondsToAdd = totalDailyJourneyInSeconds * fractionOfDay;
        DateTime result = date;
        if (date > endDay)
        {
            if(multiplier > 0)
            {
                date = GetValidWorkday(date, multiplier);
                return new DateTime(date.Year, date.Month, date.Day, startDay.Hour, startDay.Minute, 0).AddSeconds((double)secondsToAdd);
            }
            return new DateTime(date.Year, date.Month, date.Day, endDay.Hour, endDay.Minute, 0).AddSeconds((double)secondsToAdd);
        }

        if (date < startDay)
        {
            if (multiplier < 0)
            {
                date = GetValidWorkday(date, multiplier);
                return new DateTime(date.Year, date.Month, date.Day, endDay.Hour, endDay.Minute, 0).AddSeconds((double)secondsToAdd);
            }
            return new DateTime(date.Year, date.Month, date.Day, startDay.Hour, startDay.Minute, 0).AddSeconds((double)secondsToAdd);
        }

        var availableTime = (endDay - date).TotalSeconds;
        var timeUsed = (endDay - startDay).TotalSeconds - availableTime;
        if (multiplier > 0)
        {
            if ((decimal)availableTime < secondsToAdd)
            {
                date = GetValidWorkday(date, multiplier);
                return new DateTime(date.Year, date.Month, date.Day, startDay.Hour, startDay.Minute, 0).AddSeconds((double)secondsToAdd - availableTime);
            }
            return date.AddSeconds((double)secondsToAdd);
        }
        
        if ((decimal)timeUsed < secondsToAdd)
        {
            date = GetValidWorkday(date, multiplier);
        }
        return new DateTime(date.Year, date.Month, date.Day, endDay.Hour, endDay.Minute, 0).AddSeconds(Math.Round((double)secondsToAdd - timeUsed));
    }

    private DateTime GetValidWorkday(DateTime date, int direction)
    {
        do
        {
            date = date.AddDays(direction);
        } while (!IsWorkday(date));
        return date;
    }

    private bool IsWorkday(DateTime date)
    {
        return !_holidays.Contains(date.Date) &&
               !_recurringHolidays.Contains((date.Month, date.Day)) &&
               date.DayOfWeek != DayOfWeek.Saturday &&
               date.DayOfWeek != DayOfWeek.Sunday;
    }
}