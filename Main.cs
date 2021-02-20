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
        private void OpenMusicClicked(object o, EventArgs e)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("음악 파일(.mp3 혹은.wav)를 선택하세요", null, FileChooserAction.Open, "불러오기", ResponseType.Accept);
            FileFilter musicFileFilter = new FileFilter(); musicFileFilter.AddPattern("*.mp3"); musicFileFilter.AddPattern("*.wav");
            fileChooser.Filter = musicFileFilter;
            fileChooser.Run();
            string fileName = fileChooser.Filename;
            Console.WriteLine(fileName);
            musicPath.Text = fileName;
            fileChooser.Dispose();
        }
        private void OpenJsonClicked(object o, EventArgs e)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("채보 파일(.json)을 선택하세요", null, FileChooserAction.Open, "불러오기", ResponseType.Accept);
            FileFilter jsonFileFilter = new FileFilter(); jsonFileFilter.AddPattern("*.json");
            fileChooser.Filter = jsonFileFilter;
            fileChooser.Run();
            string fileName = fileChooser.Filename;
            Console.WriteLine(fileName);
            jsonPath.Text = fileName;
            fileChooser.Dispose();
        }
    }
}