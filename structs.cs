using System;

namespace RaindropsCustomAssist
{
    struct Music
    {
        string _path;
        string path
        {
            get {return _path;}
        }
        public Music(string path)
        {
            _path = path;
        }
    }
    struct Event
    {
        double _time;
    }
    struct Note
    {
        double _time;
        NoteType _type;
        public double time
        {
            get {return _time;}
        }
        public NoteType noteType
        {
            get {return _type;}
        }
        public Note(double time, NoteType noteType)
        {
            _time = time;
            _type = noteType;
        }
    }
    enum NoteType
    {
        Click, Wheel, Catch
    }
}