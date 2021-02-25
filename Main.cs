using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Gtk;
// using Gdk;

namespace RaindropsCustomAssist
{
    partial class MainPage
    {
        string musicPath = "";
        string jsonPath = "";
        private void openMusicButton_opened(object o,  EventArgs e)
        {
            string filename = openMusicButton.Filename;
            if (filename == "") return;
            musicPath = filename;
            if(jsonPath != "") doneButton.Sensitive = true;
        }
        private void openJsonButton_opened(object o, EventArgs e)
        {
            string filename = openJsonButton.Filename;
            if (filename == "") return;
            jsonPath = filename;
            if(musicPath != "") doneButton.Sensitive = true;
        }
    }
}