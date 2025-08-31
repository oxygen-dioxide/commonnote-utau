
using System.Globalization;
using System.Text;
using System.Reflection;
using utauPlugin;
using Commonnote;

Console.WriteLine($"CommonnoteUtauPaste {Assembly.GetEntryAssembly()?.GetName().Version}");
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

var commonnoteData = Commonnote.Commonnote.LoadFromClipboard();

//preprocess
var cNotes = commonnoteData.notes
    .OrderBy(n => n.start)
    .ThenBy(n => -n.pitch)
    .ToList();

if (cNotes.Count == 0)
{
    return;
}

var currentNote = cNotes[0];

List<CommonnoteNote> FixOverlap(List<CommonnoteNote> cNotes)
{
    var cNotesFixed = new List<CommonnoteNote>();
    foreach (var note in cNotes.Skip(1))
    {
        if (note.start == currentNote.start)
        {
            continue;
        }
        if (note.start < currentNote.start + currentNote.length)
        {
            currentNote.length = note.start - currentNote.start;
        }
        cNotesFixed.Add(currentNote);
        currentNote = note;
    }
    cNotesFixed.Add(currentNote);
    return cNotesFixed;
}

cNotes = FixOverlap(cNotes);

int resolution = commonnoteData.header.resolution > 0 ? commonnoteData.header.resolution : 480;

string defaultLyric = "a";

for (int i = 0; i < cNotes.Count; i++)
{
    var note = cNotes[i];
    var start = (int)(note.start * 480 / resolution);
    var length = (int)((note.start + note.length) * 480 / resolution - start);
    note.start = start;
    note.length = length;
    if (String.IsNullOrEmpty(note.label))
    {
        note.label = defaultLyric;
    }
}

void InsertPluginNote(
    int duration, 
    int tone, 
    string lyric, 
    string flags,
    UtauPlugin plugin, 
    int insertLocation)
{
    plugin.InsertNote(insertLocation);
    plugin.note[insertLocation].SetLength(duration);
    plugin.note[insertLocation].SetNoteNum(tone);
    plugin.note[insertLocation].SetLyric(lyric);
    plugin.note[insertLocation].SetFlags(flags);
}

void InsertPluginRest(
    int duration,
    UtauPlugin plugin,
    int insertLocation)
{
    InsertPluginNote(
        duration,
        60,
        "R",
        "",
        plugin,
        insertLocation);
}

int currentTick = cNotes[0].start;
bool hasPrev = plugin.note.Count > 0 && plugin.note[0].GetNum() == "PREV";
bool hasNext = plugin.note.Count > 0 && plugin.note[^1].GetNum() == "NEXT";
int insertLocation = hasPrev ? 1 : 0;

foreach (var note in cNotes)
{
    if (note.start > currentTick)
    {
        InsertPluginRest(note.start - currentTick, plugin, insertLocation);
        insertLocation++;
    }
    InsertPluginNote(
        note.length,
        note.pitch,
        note.label,
        "",
        plugin,
        insertLocation);
    insertLocation++;
    currentTick = note.start + note.length;
}

plugin.Output();