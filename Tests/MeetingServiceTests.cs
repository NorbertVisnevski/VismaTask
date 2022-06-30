using System.ComponentModel.DataAnnotations;
using VismaTask.Common.Exceptions;
using VismaTask.Data;
using VismaTask.Models;
using VismaTask.Services;

namespace VismaTask.Test;

public class MeetingServiceTests
{
    private readonly DbContext _dbContext;
    private readonly MeetingService _meetingService;
    private readonly List<string> _baseArgs;

    public MeetingServiceTests()
    {
        _dbContext = new DbContext("PLACEHOLDER");
        _meetingService = new MeetingService(_dbContext);
        _baseArgs = new List<string>(new[] { "add-meeting", "meeting-name", "responsible-person", "description", "Short", "inPerson", "2023-02-10", "2023-02-20" });
    }

    [Fact]
    public void ServiceShouldAddMeeting()
    {
        var meeting = _meetingService.AddMeeting(_baseArgs);
        Assert.True(meeting.Name.Equals("meeting-name"));
        Assert.True(meeting.Category == Category.Short);
        Assert.False(meeting.Type == Type.Live);
        Assert.True(meeting.StartDate.Equals(new DateTime(2023, 2, 10)));
        Assert.True(_meetingService.ListMeetings(_baseArgs).Count == 1);
    }

    [Fact]
    public void ServiceShouldNotAddDuplicateMeeting()
    {
        _meetingService.AddMeeting(_baseArgs);
        var exception = Assert.Throws<ValidationException>(() => _meetingService.AddMeeting(_baseArgs));
        Assert.Equal("Meeting name should be unique", exception.Message);
        Assert.True(_meetingService.ListMeetings(_baseArgs).Count == 1);
    }

    [Fact]
    public void ServiceShouldDeleteMeeting()
    {
        _meetingService.AddMeeting(_baseArgs);
        Assert.Throws<NotFoundException>(() => _meetingService.RemoveMeeting(new List<string>(new[] { "remove-meeting", "meeting-name", "other person" })));

        _meetingService.RemoveMeeting(new List<string>(new[] { "remove-meeting", "meeting-name", "responsible-person" }));
        Assert.True(_meetingService.ListMeetings(_baseArgs).Count == 0);
    }

    [Fact]
    public void ServiceShouldAddAttendeeIfNoTimeInterception()
    {
        _meetingService.AddMeeting(_baseArgs);
        var meeting = _meetingService.AddToMeeting(new List<string>(new[] { "add-to-meeting", "meeting-name", "other person", "2023-02-10T10:10:00.0" }));
        Assert.Throws<ValidationException>(() => _meetingService.AddToMeeting(new List<string>(new[] { "add-to-meeting", "meeting-name", "other person", "2023-02-10T10:10:00.0" })));
        Assert.True(meeting.Attendees.Count == 2);

        _baseArgs[1] = "meeting-name2";
        _baseArgs[2] = "another person";
        _baseArgs[6] = "2023-02-15";
        _baseArgs[7] = "2023-02-25";
        _meetingService.AddMeeting(_baseArgs);
        Assert.Throws<ValidationException>(() => _meetingService.AddToMeeting(new List<string>(new[] { "add-to-meeting", "meeting-name2", "other person", "2023-02-16T10:10:00.0" })));
    }

    [Fact]
    public void ServiceShouldAddAndRemoveAttendeeFromMeeting()
    {
        _meetingService.AddMeeting(_baseArgs);
        var meeting = _meetingService.AddToMeeting(new List<string>(new[] { "add-to-meeting", "meeting-name", "other person", "2023-02-10T10:10:00.0" }));
        Assert.True(meeting.Attendees.Count == 2);

        Assert.Throws<NotFoundException>(() => _meetingService.RemoveFromMeeting(new List<string>(new[] { "remove-from-meeting", "meeting-name", "responsible-person" })));

        _meetingService.RemoveFromMeeting(new List<string>(new[] { "remove-from-meeting", "meeting-name", "other person"}));
        Assert.True(meeting.Attendees.Count == 1);
    }
    [Fact]
    public void ServiceShouldFilter()
    {
        _dbContext.Meetings.Add(new Meeting("name", "person", "super maga awesome description", Category.CodeMonkey, Type.Live, new DateTime(2022, 1, 1), new DateTime(2022, 1, 1)));
        _dbContext.Meetings.Add(new Meeting(".NET", "person2", "super maga awesome description for .NET meeting", Category.Short, Type.Live, new DateTime(2022, 2, 2), new DateTime(2022, 2, 2)));
        _dbContext.Meetings[1].Attendees.AddRange(new[]
        {
            new Attendee("person4",new DateTime(2022, 2, 2)),
            new Attendee("person5",new DateTime(2022, 2, 2)),
            new Attendee("person6",new DateTime(2022, 2, 2)),
            new Attendee("person7",new DateTime(2022, 2, 2)),
        });
        _dbContext.Meetings.Add(new Meeting(".NET", "person2", "super maga awesome description for .NET meeting", Category.Short, Type.Live, new DateTime(2022, 7, 2), new DateTime(2022, 7, 2)));
        _dbContext.Meetings.Add(new Meeting("name3", "person", "super maga awesome description", Category.CodeMonkey, Type.inPerson, new DateTime(2022, 3, 1), new DateTime(2022, 3, 1)));

        var meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings"}));
        Assert.True(meetings.Count == 4);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-d", "unknown string" }));
        Assert.Empty(meetings);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-d", ".NET" }));
        Assert.True(meetings.Count == 2);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-s", "2021-01-01", "-e", "2022-03-03" }));
        Assert.True(meetings.Count == 3);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-t", "inPerson", "-n", "name3" }));
        Assert.True(meetings.Count == 1);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-aa", "1" }));
        Assert.True(meetings.Count == 1);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-ab", "2" }));
        Assert.True(meetings.Count == 3);

        meetings = _meetingService.ListMeetings(new List<string>(new[] { "list-meetings", "-u", "person" }));
        Assert.True(meetings.Count == 2);
        Assert.Equal("person", meetings[0].ResponsiblePerson);
    }
}
