using NUnit.Framework.Internal;
using WorkdayNet;

namespace Calculator.UnitTests;

public class WorkdayCalendarTests
{
    private WorkdayCalendar _workdayCalendar;

    [SetUp]
    public void SetUp()
    {
        _workdayCalendar = new WorkdayCalendar();
        _workdayCalendar.SetWorkdayStartAndStop(8, 0, 16, 0);
        _workdayCalendar.SetRecurringHoliday(5, 17);
        _workdayCalendar.SetHoliday(new DateTime(2004, 5, 27));
    }

    [Test, TestCaseSource(nameof(GetWorkDayChallengeCases))]
    public void GetWorkDay_Should_Return_Positive_Days((DateTime, decimal, DateTime) dayCase)
    {
        //Arrage
        string format = "dd-MM-yyyy HH:mm";

        //Act
        var incrementedDate = _workdayCalendar.GetWorkdayIncrement(dayCase.Item1, dayCase.Item2);

        //Assert
        Assert.That(incrementedDate.ToString(format), Is.EqualTo(dayCase.Item3.ToString(format)));
    }

    private static IEnumerable<(DateTime startday, decimal increment, DateTime result)> GetWorkDayChallengeCases()
    {
        return new[]
        {
            (new DateTime(2004, 5, 24, 18, 5, 0), -5.5m, new DateTime(2004, 5, 14, 12, 0, 0)),
            (new DateTime(2004, 5, 24, 19, 3, 0), 44.723656m, new DateTime(2004, 7, 27, 13, 47, 0)),
            (new DateTime(2004, 5, 24, 18, 3, 0), -6.7470217m, new DateTime(2004, 5, 13, 10, 2, 0)),
            (new DateTime(2004, 5, 24, 8, 3, 0), 12.782709m, new DateTime(2004, 6, 10, 14, 18, 0)),
            (new DateTime(2004, 5, 24, 7, 3, 0), 8.276628m, new DateTime(2004, 6, 4, 10, 12, 0)),
        };
    }
}