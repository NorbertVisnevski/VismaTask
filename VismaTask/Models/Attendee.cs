namespace VismaTask.Models;

public class Attendee
{
    public string PersonName { get; set; }
    public DateTime AttendsFrom { get; set; }
    public Attendee(string personName, DateTime attendsFrom)
    {
        PersonName = personName;
        AttendsFrom = attendsFrom;
    }
}
