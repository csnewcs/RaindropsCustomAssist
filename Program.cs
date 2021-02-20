using System;
using Gtk;
using Gdk;

using Newtonsoft.Json.Linq;


namespace RaindropsCustomAssist
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            new MainPage();
            Application.Run();
            // Console.WriteLine("Hello World!");
        }
    }
}
