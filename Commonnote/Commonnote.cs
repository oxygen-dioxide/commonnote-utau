using System.Collections.Generic;
using TextCopy;
using Newtonsoft.Json;

//Commonnote format definition: https://github.com/ExpressiveLabs/commonnote
namespace Commonnote
{
    public struct CommonnoteNote
    {
        public int start;
        public int length;
        public string label;
        public int pitch;
    }

    public struct CommonnoteHeader
    {
        public int resolution;
        public string origin;
    }

    public struct CommonnoteData
    {
        public static string identifier = "commonnote";
        public CommonnoteHeader header;
        public List<CommonnoteNote> notes;
    }

    public static class Commonnote
    {
        public static string Dumps(CommonnoteData data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static CommonnoteData Loads(string text)
        {
            return JsonConvert.DeserializeObject<CommonnoteData>(text);
        }

        public static void CopyToClipboard(CommonnoteData data)
        {
            var text = Dumps(data);
            ClipboardService.SetText(text);
        }

        public static CommonnoteData LoadFromClipboard()
        {
            var text = ClipboardService.GetText();
            return Loads(text);
        }
    }
}