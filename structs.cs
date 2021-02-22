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
        double _yJudge;
        double _speed;
        double _color;

        public Event(double time, double yJudge, double speed, double color)
        {
            _time = time;
            _yJudge = yJudge;
            _speed = speed;
            _color = color;
        }
    }
    struct Note
    {
        double _time;
        double _xPoint;
        double _length;
        double _endXPoint;
        NoteType _type;
        public double time
        {
            get {return _time;}
        }
        public NoteType noteType
        {
            get {return _type;}
        }
        public Note(double time, NoteType noteType, double xPoint, double length, double endXPoint)
        {
            _time = time;
            _type = noteType;
            _xPoint = xPoint;
            _length = length;
            _endXPoint = endXPoint;
        }
        public Note(double time, NoteType noteType, double xPoint)
        {
            _time = time;
            _type = noteType;
            _xPoint = xPoint;
            _length = 0;
            _endXPoint = xPoint;
        }
    }
    enum NoteType
    {
        Click, Wheel, Catch
    }
}
