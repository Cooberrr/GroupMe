namespace GroupMe;

using GroupMe.domain;
using GroupMe.services;

public class ConsoleUI
{
    private CalendarService _calendar;
    private NoteService _noteService;
    private MeetingResourceService _fileService;
    private Group? _currentGroup;
    private GroupMember? _currentUser;
    private Group _group;
    private GroupLeader _leader;
    public ConsoleUI()
    {
        _calendar = new CalendarService();
        _noteService = new NoteService();
        _fileService = new MeetingResourceService();
        _currentGroup = new Group(1, "Demo Group");
        _currentUser = new GroupMember(1, "Cooper", "cooper@email.com", Role.Member);
        _currentUser.JoinGroup(_currentGroup);
    }

    public void ShowMenu()
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n=== GroupMe Main Menu ===");
            Console.WriteLine($"Current User: {(_currentUser != null ? _currentUser.Name : "None")}");
            Console.WriteLine($"Current Group: {(_currentGroup != null ? _currentGroup.GroupName : "None")}");
            Console.WriteLine();
            Console.WriteLine("1. Setup Group");
            Console.WriteLine("2. Schedule a Meeting");
            Console.WriteLine("3. View Meetings");
            Console.WriteLine("4. Create a Note");
            Console.WriteLine("5. View Notes");
            Console.WriteLine("6. Upload a File");
            Console.WriteLine("7. View Files");
            Console.WriteLine("8. Join Group");
            Console.WriteLine("9. Leave Group");
            Console.WriteLine("0. Exit");
            Console.Write("Enter choice: ");

            int choice = ReadChoice();

            switch (choice)
            {
                case 1:
                    SetupGroup();
                    break;
                case 2:
                    ScheduleMeeting();
                    break;
                case 3:
                    ViewMeetings();
                    break;
                case 4:
                    CreateNote();
                    break;
                case 5:
                    ViewNotes();
                    break;
                case 6:
                    UploadFile();
                    break;
                case 7:
                    ViewFiles();
                    break;
                case 8:
                    JoinGroup();
                    break;
                case 9:
                    LeaveGroup();
                    break;
                case 0:
                    running = false;
                    DisplayMessage("Goodbye!");
                    break;
                default:
                    DisplayMessage("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    public int ReadChoice()
    {
        string input = Console.ReadLine() ?? "0";
        if (int.TryParse(input, out int choice))
        {
            return choice;
        }
        return -1;
    }

    public void DisplayMessage(string message)
    {
        Console.WriteLine($"\n{message}");
    }

    private void SetupGroup()
    {
        Console.Write("Enter group name: ");
        string groupName = Console.ReadLine() ?? "My Group";

        _group = new Group(1, groupName);
        Console.Write("Enter leader name: ");
        string leaderName = Console.ReadLine() ?? "Leader";

        Console.Write("Enter leader email: ");
        string leaderEmail = Console.ReadLine() ?? "leader@email.com";

        _leader = new GroupLeader(1, leaderName, leaderEmail);
        _leader.JoinGroup(_group);
        DisplayMessage($"Group '{groupName}' created with leader {leaderName}.");
    }

    private void ScheduleMeeting()
    {
        if (_currentGroup == null)
        {
            Console.WriteLine("You need to setup or join a group before scheduling a meeting.");
        return;
        }

        Console.Write("Enter meeting title: ");
        string title = Console.ReadLine();

        Console.Write("Enter meeting date and time (example: 5/6/2026 3:30 PM): ");
        string dateInput = Console.ReadLine();

        DateTime meetingDate;

        if (!DateTime.TryParse(dateInput, out meetingDate))
        {
            Console.WriteLine("Invalid date/time. Meeting was not scheduled.");
            return;
        }

        Console.WriteLine("Choose meeting type:");
        Console.WriteLine("1. Online");
        Console.WriteLine("2. In Person");
        Console.WriteLine("3. Hybrid");
        Console.WriteLine("4. Call");

        Console.Write("Enter choice: ");
        string typeInput = Console.ReadLine();

        MeetingType type;

        switch (typeInput)
        {
            case "1":
                type = MeetingType.Online;
                break;
            case "2":
                type = MeetingType.InPerson;
                break;
            case "3":
                type = MeetingType.Hybrid;
                break;
            case "4":
                type = MeetingType.Call;
                break;
            default:
                Console.WriteLine("Invalid meeting type. Meeting was not scheduled.");
                return;
        }

        MeetingDetails details = new MeetingDetails(title, meetingDate, type);

        if (type == MeetingType.Online)
        {
            Console.Write("Enter meeting link: ");
            string link = Console.ReadLine();

            _calendar.CreateAndAddMeeting(type, details, link: link);
        }
        else if (type == MeetingType.InPerson)
        {
            Console.Write("Enter meeting location: ");
            string location = Console.ReadLine();

            _calendar.CreateAndAddMeeting(type, details, location: location);
        }
        else if (type == MeetingType.Hybrid)
        {
            Console.Write("Enter meeting link: ");
            string link = Console.ReadLine();

            Console.Write("Enter meeting location: ");
            string location = Console.ReadLine();

            _calendar.CreateAndAddMeeting(type, details, link: link, location: location);
        }
        else if (type == MeetingType.Call)
        {
            Console.Write("Enter conference number: ");
            string numberInput = Console.ReadLine();

            int conferenceNum;

            if (!int.TryParse(numberInput, out conferenceNum))
            {
                Console.WriteLine("Invalid conference number. Meeting was not scheduled.");
                return;
            }

            _calendar.CreateAndAddMeeting(type, details, conferenceNum: conferenceNum);
        }

        Console.WriteLine($"Meeting '{title}' scheduled for {meetingDate}.");
    }
    private void ViewMeetings()
    {
        List<Meeting> meetings = _calendar.GetMeetings();

        if (meetings.Count == 0)
        {
            Console.WriteLine("No meetings scheduled.");
            return;
        }

        foreach (Meeting meeting in meetings)
        {
            MeetingDetails details = meeting.GetDetails();

            Console.WriteLine($"{details.Title} - {details.Date} - {details.Type}");
        }
    }

    private void CreateNote()
    {
        Console.Write("Enter note title: ");
        string title = Console.ReadLine() ?? "Note";

        Console.Write("Enter note content: ");
        string content = Console.ReadLine() ?? "";

        Console.Write("Public or Private? (1 = Public, 2 = Private): ");
        int noteType = ReadChoice();

        Note note;

        if (noteType == 2 && _leader != null)
        {
            note = new PrivateNote(_noteService.GetNotes().Count + 1, title, content, _leader);
        }
        else
        {
            note = new PublicNote(_noteService.GetNotes().Count + 1, title, content);
        }

        _noteService.CreateNotes(note);
    }
    private void ViewNotes()
    {
        List<Note> notes = _noteService.GetNotes();

        if (notes.Count == 0)
        {
            DisplayMessage("No notes found.");
            return;
        }

        Console.WriteLine("\n=== Notes ===");
        foreach (Note n in notes)
        {
            Console.WriteLine($"- [{n.GetType().Name}] {n.Title}: {n.GetContent()}");
        }
    }

    private void UploadFile()
    {
        Console.Write("Enter file name: ");
        string fileName = Console.ReadLine() ?? "file.txt";

        Console.Write("Enter file content: ");
        string content = Console.ReadLine() ?? "";

        _fileService.UploadFile(fileName, content);
    }

    private void ViewFiles()
    {
        List<string> files = _fileService.ListFiles();

        if (files.Count == 0)
        {
            DisplayMessage("No files uploaded.");
            return;
        }
        Console.WriteLine("\n=== Uploaded Files ===");
        foreach (string f in files)
        {
            Console.WriteLine($"-{f}");
        }
    }
    private void JoinGroup()
    {
        Console.Write("Enter group name to join: ");
        string groupName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(groupName))
        {
            Console.WriteLine("Group name cannot be empty.");
            return;
        }

        if (_currentUser == null)
        {
            Console.Write("Enter your name: ");
            string name = Console.ReadLine();

            Console.Write("Enter your email: ");
            string email = Console.ReadLine();

            _currentUser = new GroupMember(1, name, email, Role.Member);
        }

        _currentGroup = new Group(1, groupName);
        _currentUser.JoinGroup(_currentGroup);

        Console.WriteLine($"{_currentUser.Name} joined {_currentGroup.GroupName}.");
    }

    private void LeaveGroup()
    {
        if (_currentGroup == null || _currentUser == null)
        {
            Console.WriteLine("No current group or user selected.");
            return;
        }

        string groupName = _currentGroup.GroupName;

        _currentGroup.RemoveMember(_currentUser);
        _currentGroup = null;

        Console.WriteLine($"{_currentUser.Name} left {groupName}.");
    }
}