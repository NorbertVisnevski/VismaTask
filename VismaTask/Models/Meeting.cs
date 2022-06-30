namespace VismaTask.Models;

public class Meeting
{
    public string Name { get; set; }
    public string ResponsiblePerson { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public Type Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Attendee> Attendees { get; set; }

    public Meeting(string name, string responsiblePerson, string description, Category category, Type type, DateTime startDate, DateTime endDate)
    {
        Name = name;
        ResponsiblePerson = responsiblePerson;
        Description = description;
        Category = category;
        Type = type;
        StartDate = startDate;
        EndDate = endDate;
        Attendees = new List<Attendee>();
    }
}
