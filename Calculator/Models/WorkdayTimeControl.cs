namespace WorkdayNet.Models;

public class WorkdayTimeControl
{
    public int Hour { get; set; }
    public int Minutes { get; set; }

    public WorkdayTimeControl(int hour, int minutes)
    {
        Hour = hour;
        Minutes = minutes;
    }
}