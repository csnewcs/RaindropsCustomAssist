using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using Newtonsoft.Json;
using NetCoreAudio;
using Gtk;

// using Gdk;

namespace RaindropsCustomAssist
{
    partial class MainPage
    {
        Chabo chabo;
        string musicPath = "";
        string jsonPath = "";
        Player player = new Player();
        bool played = false;
        double time = 0;
        int index = 0;
        long offset = 0;
        Stopwatch sw = new Stopwatch();
        
        private string timeToString(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes}:{time.Seconds}";
        }
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
        private void doneButton_clicked(object o, EventArgs e)
        {
            chabo = new Chabo(musicPath, jsonPath);
            mainGrid.Attach(getMainGrid(), 1, 1, 1, 1);
            Remove(firstGrid);
            Add(mainGrid);
            mainGrid.ShowAll();
        }
        private void startCopyButton_clicked(object o, EventArgs e)
        {
            Thread thread = new Thread(() => {
                // string jsonName = chabo.jsonFile.Name;
                var jsonFile = chabo.jsonFile;
                var musicFile = chabo.musicFile;

                string name = musicFile.Name;
                string realName = name.Substring(0, musicFile.Name.Length - 4);
                
                jsonPath = $"{customDirectoryEntry.Text}/{realName}.json";
                musicPath = $"{customDirectoryEntry.Text}/{musicFile.Name}";
                
                jsonFile.CopyTo(jsonPath);
                setFraction(0.5);
                musicFile.CopyTo(musicPath);
                setFraction(1);

                showDialog(new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, "작업을 완료했습니다!"));
            });
            thread.Start();
        }
        private void timeScale_valueChanged(object o, EventArgs e)
        {
            timeLabel.Text = $"{((int)timeScale.Value) / 60}:{((int)timeScale.Value) % 60} / {timeToString(chabo.music.duration)}";
        }
        private void playPauseButton_clicked(object o, EventArgs e)
        {
            double[] noteTimes = new double[chabo.getNoteCounts()[1]];
            int i = 0;
            foreach(var note in chabo.notes)
            {
                if(note.noteType == NoteType.Click)
                {
                    noteTimes[i] = note.time;
                    i++;
                }
            }
            
            Thread noteSoundThread = new Thread(() => noteSoundPlay(noteTimes));
            if(playPauseButton.Label == "▶️")
            {
                playPauseButton.Label = "⏸";
                if(played)
                {
                    player.Resume();
                    sw.Start();
                }
                else
                {
                    player.Play(musicPath);
                    played = true;
                    sw.Restart();
                }
                noteSoundThread.Start();
            }
            else
            {
                playPauseButton.Label = "▶️";
                player.Pause();
                sw.Stop();
            }
            // player.
        }
        private void showDialog(MessageDialog dialog)
        {
            Application.Invoke(delegate {dialog.Run(); dialog.Show(); dialog.Dispose();});
        }
        private void addFraction(double add)
        {
            Application.Invoke(delegate {
                if(doingProgressBar.Fraction + add > 1)
                {
                    doingProgressBar.Fraction = 1;
                }
                else if(doingProgressBar.Fraction + add < 0)
                {
                    doingProgressBar.Fraction = 0;
                }
                else
                {
                    doingProgressBar.Fraction += add;
                }
            });
        }
        private void setFraction(double set)
        {
            Application.Invoke(delegate {
                if(set > 1)
                {
                    doingProgressBar.Fraction = 1;
                }
                else if(set < 0)
                {
                    doingProgressBar.Fraction = 0;
                }
                else
                {
                    doingProgressBar.Fraction = set;
                }
            });
        }
        private void noteSoundPlay(double[] noteTimes)
        {
            while(this.player.Playing)
            {
                if((double)(sw.ElapsedTicks - offset) / 1000000000 >= noteTimes[index])
                {
                    Player player = new Player();
                    player.Play("note.mp3");
                    Console.WriteLine("{0}번째 노트, {1}틱", index, sw.ElapsedTicks);
                    index++;
                }
            }
        }
    }
}