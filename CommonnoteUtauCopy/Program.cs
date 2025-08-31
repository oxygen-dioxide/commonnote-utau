
using System.Globalization;
using System.Text;
using System.Reflection;
using utauPlugin;
using Commonnote;

static bool isRest(Note note)
{
    var lyric = note.GetLyric();
    return lyric == "R" || lyric == "r";
}

Console.WriteLine($"CommonnoteUtauCopy {Assembly.GetEntryAssembly()?.GetName().Version}");
Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var arg = Environment.GetCommandLineArgs();

//Load tmp ust file
if (arg.Length < 2)
{
    Console.WriteLine("Please launch this plugin from utau editor");
    Console.ReadLine();
    return;
}
UtauPlugin plugin = new UtauPlugin(arg[1]);
plugin.Input();

var CommonnoteNotes = new List<CommonnoteNote>();

var currentPosition = 0;
foreach (var note in plugin.note)
{
    if (note.GetNum() == "PREV" || note.GetNum() == "NEXT")
    {
        continue;
    }
    var duration = note.GetLength();

    if (!isRest(note))
    {
        CommonnoteNotes.Add(new CommonnoteNote
        {
            start = currentPosition,
            length = duration,
            pitch = note.GetNoteNum(),
            label = note.GetLyric()
        });
    }
    currentPosition += duration;
}

var commonnoteData = new CommonnoteData
{
    identifier = "commonnote",
    header = new CommonnoteHeader
    {
        resolution = 480,
        origin = "utau",
    },
    notes = CommonnoteNotes,
};

Commonnote.Commonnote.CopyToClipboard(commonnoteData);