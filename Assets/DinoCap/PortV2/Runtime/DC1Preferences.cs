using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DinoCap.PortV2
{
    // Platform storage adapter. Gameplay owners retain their native checkpoint
    // boundaries; setting a value does not commit it until Save is called.
    public static partial class DC1Preferences
    {
        [Serializable] sealed class Entry { public string key; public int kind, integer; public float real; public string text; }
        [Serializable] sealed class Document { public int version=1; public bool legacyImported=true; public List<Entry> entries=new List<Entry>(); }
        static readonly Dictionary<string,Entry> values=new Dictionary<string,Entry>();
        static string directory;
        static bool loaded,dirty;
        static string directoryOverride;
        public static string SaveFilePath { get { EnsureLoaded();return Path.Combine(directory,"dinocap-save.json"); } }
        static string ResolveDirectory()
        {
            if(directoryOverride!=null)return directoryOverride;
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i+1<args.Length;i++)if(args[i]=="-dc1-save-directory")return Path.GetFullPath(args[i+1]);
#if UNITY_EDITOR
            // Automated tests never import or mutate the user's live campaign.
            if(Array.IndexOf(args,"-runTests")>=0)
                return Path.GetFullPath(Path.Combine(Application.dataPath,"../../Dev/Operator/DC1-MASTER/TestSaves",System.Diagnostics.Process.GetCurrentProcess().Id.ToString()));
#endif
            return Application.persistentDataPath;
        }
        static bool ReadDocument(string path)
        {
            if(!File.Exists(path))return false;
            try
            {
                var doc=JsonUtility.FromJson<Document>(File.ReadAllText(path));
                if(doc==null || doc.version!=1 || doc.entries==null || !doc.legacyImported)return false;
                values.Clear();
                foreach(var entry in doc.entries)
                {
                    if(entry==null || string.IsNullOrEmpty(entry.key) || entry.kind<0 || entry.kind>2)return false;
                    values[entry.key]=entry;
                }
                return true;
            }
            catch(ArgumentException){return false;}
        }
        static void EnsureLoaded()
        {
            if(loaded)return;
            directory=ResolveDirectory();
            string path=Path.Combine(directory,"dinocap-save.json");
            loaded=true;
            if(ReadDocument(path))return;
            if(ReadDocument(path+".bak")){File.Copy(path+".bak",path,true);dirty=false;return;}
            values.Clear();
            if(File.Exists(path) || File.Exists(path+".bak"))
            {
                loaded=false;
                throw new InvalidDataException("DC1 save and backup could not be read; existing files were preserved.");
            }
            var args=Environment.GetCommandLineArgs();
            bool isolated=directoryOverride!=null || Array.IndexOf(args,"-dc1-save-directory")>=0;
#if UNITY_EDITOR
            isolated|=Array.IndexOf(args,"-runTests")>=0;
#endif
            if(!isolated)ImportLegacy();
            dirty=true;Save();
        }
        static void ImportLegacy()
        {
            // Read only once, before the first file save. Registry/shared-prefs
            // are never written and never resurrected after ResetAll.
            foreach(string key in LegacyKeys)
            {
                if(!UnityEngine.PlayerPrefs.HasKey(key))continue;
                bool real=key==CityGrindStartPreset.HealthPrefKey || key.EndsWith("_MoveSpeed",StringComparison.Ordinal) || key=="DC1_Debug_Jungle_TimerSpeed";
                values[key]=real?new Entry {key=key,kind=1,real=UnityEngine.PlayerPrefs.GetFloat(key)}:
                    new Entry {key=key,kind=0,integer=UnityEngine.PlayerPrefs.GetInt(key)};
            }
        }
        public static bool HasKey(string key){EnsureLoaded();return values.ContainsKey(key);}
        public static int GetInt(string key,int fallback=0){EnsureLoaded();return values.TryGetValue(key,out var e)&&e.kind==0?e.integer:fallback;}
        public static float GetFloat(string key,float fallback=0){EnsureLoaded();return values.TryGetValue(key,out var e)&&e.kind==1?e.real:fallback;}
        public static string GetString(string key,string fallback=""){EnsureLoaded();return values.TryGetValue(key,out var e)&&e.kind==2?e.text:fallback;}
        public static void SetInt(string key,int value){EnsureLoaded();values[key]=new Entry {key=key,kind=0,integer=value};dirty=true;}
        public static void SetFloat(string key,float value){EnsureLoaded();if(float.IsNaN(value)||float.IsInfinity(value))throw new ArgumentOutOfRangeException(nameof(value));values[key]=new Entry {key=key,kind=1,real=value};dirty=true;}
        public static void SetString(string key,string value){EnsureLoaded();values[key]=new Entry {key=key,kind=2,text=value};dirty=true;}
        public static void DeleteKey(string key){EnsureLoaded();dirty|=values.Remove(key);}
        public static void DeleteAll(){EnsureLoaded();values.Clear();dirty=true;}
        public static void Save()
        {
            EnsureLoaded();if(!dirty)return;
            Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"dinocap-save.json"),temporary=path+".tmp",backup=path+".bak";
            var doc=new Document();
            foreach(var key in new SortedSet<string>(values.Keys,StringComparer.Ordinal))doc.entries.Add(values[key]);
            using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None))
            using(var writer=new StreamWriter(stream)) {writer.Write(JsonUtility.ToJson(doc,true));writer.Flush();stream.Flush(true);}
            if(File.Exists(path))File.Replace(temporary,path,backup);
            else File.Move(temporary,path);
            dirty=false;
        }
        public static void ResetAll()
        {
            DeleteAll();Save();
            // Reset backups too: recovery must not restore the pre-reset campaign.
            string backup=SaveFilePath+".bak";
            if(File.Exists(backup))File.Delete(backup);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal static void TestHarness_Reload(){values.Clear();loaded=false;dirty=false;}
        internal static IDisposable TestHarness_UseDirectory(string path)=>new StorageScope(path);
        sealed class StorageScope:IDisposable
        {
            readonly string oldOverride,oldDirectory;
            readonly bool oldLoaded,oldDirty;
            readonly Dictionary<string,Entry> previous;
            public StorageScope(string path)
            {
                oldOverride=directoryOverride;oldDirectory=directory;oldLoaded=loaded;oldDirty=dirty;
                previous=new Dictionary<string,Entry>(values);
                directoryOverride=Path.GetFullPath(path);TestHarness_Reload();
            }
            public void Dispose()
            {
                values.Clear();foreach(var pair in previous)values[pair.Key]=pair.Value;
                directoryOverride=oldOverride;directory=oldDirectory;loaded=oldLoaded;dirty=oldDirty;
            }
        }
#endif
    }
}
