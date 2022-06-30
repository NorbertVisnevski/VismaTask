using System.Text.Json;
using System.Text.Json.Serialization;
using VismaTask.Data;
using VismaTask.Services;

var dbContext = new DbContext("data.json");

var meetingService = new MeetingService(dbContext);

JsonSerializerOptions options = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters =
    {
        new JsonStringEnumConverter()
    }
};

if (args.Length == 0)
{
    HandleInteractive();
}

else if (args.Length == 1 && args[0].Equals("--help",StringComparison.OrdinalIgnoreCase))
{
    HandleHelp();
}

else
{
    try
    {
        HandleCommands(args.ToList());
        meetingService.SaveChanges();
    }
    catch(Exception e)
    {
        Console.WriteLine(e.Message);
    }
}

void HandleInteractive()
{
    Console.WriteLine("Please input username");
    var username = Console.ReadLine()!;
    while (true)
    {
        try
        {
            Console.WriteLine("Please input command (type --help for help) or exit");
            var command = Console.ReadLine()!;
            List<string> arguments = new List<string>();
            arguments.Add(command);
            switch (command)
            {
                case "exit":
                    return;

                case "--help":
                    HandleHelp();
                    break;

                case "add-meeting":
                    Console.Write("meeting name: ");
                    arguments.Add(Console.ReadLine()!);
                    arguments.Add(username);
                    Console.Write("description: ");
                    arguments.Add(Console.ReadLine()!);
                    PrintEnumOptions<Category>();
                    Console.Write("category: ");
                    arguments.Add(Console.ReadLine()!);
                    PrintEnumOptions<Type>();
                    Console.Write("type: ");
                    arguments.Add(Console.ReadLine()!);
                    Console.Write("start date: ");
                    arguments.Add(Console.ReadLine()!);
                    Console.Write("end date: ");
                    arguments.Add(Console.ReadLine()!);
                    HandleCommands(arguments);
                    meetingService.SaveChanges();
                    break;

                case "remove-meeting":
                    Console.Write("meeting name: ");
                    arguments.Add(Console.ReadLine()!);
                    arguments.Add(username);
                    HandleCommands(arguments);
                    meetingService.SaveChanges();
                    break;

                case "add-to-meeting":
                    Console.Write("meeting name: ");
                    arguments.Add(Console.ReadLine()!);
                    Console.Write("person: ");
                    arguments.Add(Console.ReadLine()!);
                    Console.Write("attendint start date: ");
                    arguments.Add(Console.ReadLine()!);
                    HandleCommands(arguments);
                    meetingService.SaveChanges();
                    break;

                case "remove-from-meeting":
                    Console.Write("meeting name: ");
                    arguments.Add(Console.ReadLine()!);
                    Console.Write("person: ");
                    arguments.Add(Console.ReadLine()!);
                    HandleCommands(arguments);
                    meetingService.SaveChanges();
                    break;

                case "list-meetings":
                    Console.Write("input any combinations of available filters: ");
                    arguments.AddRange(Console.ReadLine()!.Split(' '));
                    HandleCommands(arguments);
                    meetingService.SaveChanges();
                    break;

                default:
                    Console.WriteLine("unknown command");
                    break;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

void HandleHelp()
{
    Console.WriteLine(@"
    Available commands:

    add-meeting [name] [responsible person] [description] [category] [type] [start date] [end date]
    remove-meeting [name] [responsible person]
    add-to-meeting [name] [person] [attendint start date]
    remove-from-meeting [name] [person]
    list-meetings -d [description] -c [category] -t [type] -u [responsible person] -s [start date] -e [end date] -aa [attending above count] -ab [attending below count] -ai [attending includes user]
        
    required parameters are not needed in Interactive Mode
    ");
}

void HandleCommands(List<string> arguments)
{
    switch (arguments[0])
    {
        case "add-meeting":
            var newMeeting = meetingService.AddMeeting(arguments);
            Console.WriteLine(JsonSerializer.Serialize(newMeeting, options));
            break;

        case "remove-meeting":
            meetingService.RemoveMeeting(arguments);
            break;

        case "add-to-meeting":
            var meeting = meetingService.AddToMeeting(arguments);
            Console.WriteLine(JsonSerializer.Serialize(meeting, options));
            break;

        case "remove-from-meeting":
            meetingService.RemoveFromMeeting(arguments);
            break;

        case "list-meetings":
            var meetings = meetingService.ListMeetings(arguments);
            Console.WriteLine(JsonSerializer.Serialize(meetings, options));
            break;

        default:
            HandleHelp();
            break;
    }
}


void PrintEnumOptions<T>()
{
    Console.Write("[ ");
    foreach (var i in Enum.GetNames(typeof(T)))
    {
        Console.Write($"{i} ");
    }
    Console.WriteLine("]");
}

