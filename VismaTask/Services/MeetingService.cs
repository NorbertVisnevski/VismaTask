using System.ComponentModel.DataAnnotations;
using VismaTask.Common.Exceptions;
using VismaTask.Data;
using VismaTask.Models;

namespace VismaTask.Services;

public class MeetingService
{
    private readonly DbContext _context;
    public MeetingService(DbContext context)
    {
        _context = context;
    }
    public Meeting AddMeeting(List<string> args)
    {
        string name = args[1];

        if (_context.Meetings.Find(x => x.Name.Equals(name)) != null)
        {
            throw new ValidationException("Meeting name should be unique");
        }

        string responsiblePerson = args[2];
        string description = args[3];
        Category category = Enum.Parse<Category>(args[4]);
        Type type = Enum.Parse<Type>(args[5]);
        DateTime start = DateTime.Parse(args[6]);
        DateTime end = DateTime.Parse(args[7]);

        if (end < start)
        {
            throw new ValidationException("Start date can not be smaller than end date");
        }

        if (_context.Meetings.Any(meeting => meeting.Attendees.Any(attendee =>
            attendee.PersonName.Equals(responsiblePerson) &&
            attendee.AttendsFrom < end && start < meeting.EndDate)))
        {
            throw new ValidationException("Meeting time intersects");
        }

        var newMeeting = new Meeting(name, responsiblePerson, description, category, type, start, end);
        newMeeting.Attendees.Add(new Attendee(responsiblePerson, start));
        _context.Meetings.Add(newMeeting);
        return newMeeting;
    }
    public void RemoveMeeting(List<string> args)
    {
        string name = args[1];
        string person = args[2];
        var count = _context.Meetings.RemoveAll(meeting => meeting.Name.Equals(name) && meeting.ResponsiblePerson.Equals(person));
        if (count == 0)
        {
            throw new NotFoundException("Couldn't find meeting or user is not allowed to remove this meeting");
        }
    }
    public Meeting AddToMeeting(List<string> args)
    {
        string name = args[1];
        string person = args[2];
        DateTime attendsFrom = DateTime.Parse(args[3]);
        var meeting = _context.Meetings.Find(meeting => meeting.Name.Equals(name)) ?? throw new Exception($"Meeting with name {name} not found");

        if (meeting.StartDate > attendsFrom || meeting.EndDate < attendsFrom)
        {
            throw new ValidationException("Person can only join during meeting");
        }

        if (meeting.Attendees.Any(attendee => attendee.PersonName.Equals(person)))
        {
            throw new ValidationException("Person is already in meeting");
        }

        if (_context.Meetings.Any(m => m.Attendees.Any(attendee =>
            attendee.PersonName.Equals(person) &&
            attendee.AttendsFrom < meeting.EndDate && meeting.StartDate < m.EndDate)))
        {
            throw new ValidationException("Meeting time intersects");
        }

        meeting.Attendees.Add(new Attendee(person, attendsFrom));
        return meeting;
    }
    public void RemoveFromMeeting(List<string> args)
    {
        string name = args[1];
        string person = args[2];
        var meeting = _context.Meetings.Find(meeting => meeting.Name.Equals(name)) ?? throw new Exception($"Meeting with name {name} not found");
        var count = meeting.Attendees.RemoveAll(attendee => attendee.PersonName.Equals(person) && !meeting.ResponsiblePerson.Equals(person));

        if (count == 0)
        {
            throw new NotFoundException("Couldn't find meeting or user can not be removed");
        }
    }
    public List<Meeting> ListMeetings(List<string> args)
    {
        var query = _context.Meetings.AsQueryable();
        for (int i = 1; i < args.Count - 1; i++)
        {
            var tmp = i + 1;
            switch (args[i])
            {
                case "-d":
                    query = query.Where(x => x.Description.Contains(args[tmp]));
                    break;
                case "-c":
                    query = query.Where(x => x.Category == Enum.Parse<Category>(args[tmp]));
                    break;
                case "-t":
                    query = query.Where(x => x.Type == Enum.Parse<Type>(args[tmp]));
                    break;
                case "-u":
                    query = query.Where(x => x.ResponsiblePerson.Equals(args[tmp]));
                    break;
                case "-s":
                    query = query.Where(x => x.StartDate >= DateTime.Parse(args[tmp]));
                    break;
                case "-e":
                    query = query.Where(x => x.EndDate <= DateTime.Parse(args[tmp]));
                    break;
                case "-aa":
                    query = query.Where(x => x.Attendees.Count > int.Parse(args[tmp]));
                    break;
                case "-ab":
                    query = query.Where(x => x.Attendees.Count < int.Parse(args[tmp]));
                    break;
                case "-ai":
                    query = query.Where(x => x.Attendees.Any(attendee => attendee.PersonName.Equals(args[tmp])));
                    break;
            }
        }
        return query.ToList();

    }
    public void SaveChanges()
    {
        _context.SaveChanges();
    }
}
