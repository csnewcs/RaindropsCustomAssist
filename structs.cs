using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using SixLabors.ImageSharp;

namespace RaindropsCustomAssist
{
    struct Chabo
    {
        FileInfo _musicFile;
        FileInfo _jsonFile;
        List<Note> _notes;
        List<Event> _events;
        Music _music;
        string _title;
        int _bpm;
        
        
        public FileInfo musicFile
        {
            get {return _musicFile;}
        }
        public FileInfo jsonFile
        {
            get {return _jsonFile;}
        }
        public int bpm
        {
            get {return _bpm;}
        }
        public string title
        {
            get {return _title;}
        }
        public Music music
        {
            get {return _music;}
        }
        public List<Note> notes
        {
            get {return _notes;}
        }
        public List<Event> events
        {
            get {return _events;}
        }
        

        public Chabo(string musicPath, string jsonPath)
        {
            FileInfo m = new FileInfo(musicPath);
            FileInfo j = new FileInfo(jsonPath);
            if(m.Exists && j.Exists)
            {
                _musicFile = m;
                _jsonFile = j;
            }
            else
            {
                throw new Exception("one or many files not exist");
            }
            _notes = new List<Note>();
            _events = new List<Event>();
            _music = new Music(m.FullName);

            JObject json = JObject.Parse(File.ReadAllText(j.FullName));
            var noteJson = json["Notes"];
            var eventJson = json["Events"];
            _bpm = (int)json["bpm_for_custom"];
            _title = json["title_for_custom"].ToString();

            int index = 1;
            foreach(var note in noteJson)
            {
                NoteType noteType = NoteType.Click; //need_input으로 결정, 0, 1이면  클릭 / 2, 3이면 휠 / 4, 5면 캐치
                From from = From.Right; //need_input으로 결정, 짝수는 오른쪽, 홀수는 왼쪽
                int needInput = (int)note["need_input"];
                int vx = (int)note["vx"];
                switch(needInput)
                {
                    case 2 or 3:
                        noteType = NoteType.Wheel;
                        break;
                    case 4 or 5:
                        noteType = NoteType.Catch;
                        break;
                }
                if (vx < 0)
                {
                    from = From.Left;
                }
                _notes.Add(new Note(index, (double)note["time"], noteType, from, (double)note["y"], (double)note["dy"], (double)note["length"], (double)note["vx"]));
                index++;
            }
            for(int i = 0; i < 10; i++)
            {
                Console.WriteLine("{0}, {1}, {2}", _notes[i].time, _notes[i].noteType, _notes[i].from);
            }
            foreach(var oneEvent in eventJson)
            {
                _events.Add(new Event((double)oneEvent["time"], (double)oneEvent["judge_height"], (double)oneEvent["bpm"], (double)oneEvent["far_modifier"], (double)oneEvent["color"]));
            }
        }
        public int[] getNoteCounts()
        {
            int[] notes = new int[4]; //0번: 모든 노트, 1번: 클릭 노트, 2번: 휠 노트, 3번: 캐치 노트
            notes[0] = _notes.Count;
            foreach(Note note in _notes)
            {
                if(note.noteType == NoteType.Click) notes[1]++;
                else if(note.noteType == NoteType.Wheel) notes[2]++;
                else notes[3]++;
            }
            return notes;
        }
    }
    struct Music
    {
        uint _bpm;
        TimeSpan _duration;
        string[]  _artists;
        string _title;

        public uint bpm
        {
            get {return _bpm;}
        }
        public TimeSpan duration
        {
            get {return _duration;}
        }
        public string[] artists
        {
            get {return _artists;}
        }
        public string title
        {
            get {return _title;}
        }
        public Music(string path)
        {
            var file = TagLib.File.Create(path);
            TimeSpan duration = file.Properties.Duration;
            _bpm = file.Tag.BeatsPerMinute;
            _duration = duration;
            _artists = file.Tag.Performers;
            _title = file.Tag.Title;
        }
    }
    struct Event
    {
        double _time;
        double _yJudge;
        double _speed;
        double _color;
        double _far;

        public Event(double time, double yJudge, double speed, double far, double color)
        {
            _time = time;
            _yJudge = yJudge;
            _speed = speed;
            _far = far;
            _color = color;
        }
    }
    struct Note
    {
        double _time;
        double _yPoint;
        double _length;
        double _endYPoint;
        double _speed;
        NoteType _type;
        From _from;
        int _index;
        public double time
        {
            get {return _time;}
        }
        public NoteType noteType
        {
            get {return _type;}
        }
        public From from
        {
            get {return _from;}
        }
        public int index
        {
            get {return _index;}
        }
        public Note(int index, double time, NoteType noteType, From from, double yPoint, double endYPoint, double length, double noteSpeed)
        {
            _index = index;
            _time = time;
            _type = noteType;
            _yPoint = yPoint;
            _length = length;
            _endYPoint = endYPoint;
            _from = from;
            _speed = noteSpeed;
        }        
    }
    enum NoteType
    {
        Click, Wheel, Catch
    }
    enum From
    {
        Right, Left
    }
    class Setting
    {
        double _offset;
        public double offset
        {
            get { return _offset;}
            set {_offset = value;}
        }
        public Setting()
        {
            string path = "config.json";
            try
            {
                string fileOpen = File.ReadAllText(path);
                JObject settingJson = JObject.Parse(fileOpen);
                Console.WriteLine(settingJson);
                _offset = (double)settingJson["offset"];
            }
            catch
            {
                Console.WriteLine("catch");
                _offset = 0;
                saveSetting(path);
            }
        }
        public void loadSetting(string path = "config.json")
        {
            try
            {
                string fileOpen = File.ReadAllText(path);
                JObject settingJson = JObject.Parse(fileOpen);
                _offset = (double)settingJson["offset"];
            }
            catch
            {
                _offset = 0;
                saveSetting(path);
            }
        }
        public void saveSetting(string path = "config.json")
        {
            JObject settingJson = new JObject();
            settingJson.Add("offset", _offset);

            File.WriteAllText(path, settingJson.ToString());
        }
    }
}
