using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using NetCoreAudio;
using Gtk;

// using Gdk;

namespace RaindropsCustomAssist
{
    partial class MainPage
    {
        Chabo chabo;
        Setting setting;
        string musicPath = "";
        string jsonPath = "";
        Player player = new Player();
        bool played = false;
        double time = 0;
        int index = 0;
        long offset = 0;
        Stopwatch sw = new Stopwatch();
        double sink = 0;
        int sinkLooped = 0;

        
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
            double[] noteTimes = new double[chabo.getNoteCounts()[0]];
            int i = 0;
            foreach(var note in chabo.notes)
            {
                // if(note.noteType == NoteType.Click)
                // {
                    noteTimes[i] = note.time;
                    i++;
                // }
            }
            
            Thread noteSoundThread = new Thread(() => noteSoundPlay(noteTimes));
            Thread syncMusicAndScaleThread = new Thread(syncMusicAndScale);
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
                syncMusicAndScaleThread.Start();
            }
            else
            {
                playPauseButton.Label = "▶️";
                player.Pause();
                sw.Stop();
            }
            // player.
        }
        private void setSinkButton_clicked(object o, EventArgs e)
        {
            if(sinkLooped == 0)
            {
                sw.Reset();
                sinkLabel.Text = "방울 소리가 들리면 버튼을 누르세요";
                Thread soundPlayThread = new Thread(() => {
                    Player player = new Player();
                    while(sinkLooped < 11)
                    {
                        // sw.Reset();
                        Thread.Sleep(1000);
                        sw.Reset();
                        // if(sinks[sinkLooped - 1] == 0)
                        sw.Start();
                        player.Play("note.mp3");
                        sinkLooped++;
                        Console.WriteLine("looped!");
                    }
                });
                soundPlayThread.Start();
            }
            else
            {
                sink = ((double)sw.ElapsedTicks) / 1000000000;
                sinkLabel.Text = $"{sink}초";
                sinkLooped++;
                if(sinkLooped >= 11)
                {
                    sinkLooped = 0;
                    setting.offset = sink;
                    setting.saveSetting();
                    sinkLabel.Text = $"싱크 설정 완료! 설정된 싱크: {sink}초";
                }
            }
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
            double offset = setting.offset;
            while(this.player.Playing && index < chabo.notes.Count)
            {
                if(((double)sw.ElapsedTicks) / 1000000000 + setting.offset >= noteTimes[index])
                {
                    Player player = new Player();
                    player.Play("note.mp3");
                    int now = index;
                    Note note = chabo.notes[now];
                    Application.Invoke(delegate {
                        if(note.from == From.Left)
                        {
                            leftNoteLabel.Text = $"<big><b>현재 노트</b></big>\n{now + 1}번째 노트\n노트 종류: {note.noteType}\n시간: {note.time}";
                            leftNoteLabel.UseMarkup = true;
                        }
                        else
                        {
                            rightNoteLabel.Text = $"<big><b>현재 노트</b></big>\n{now + 1}번째 노트\n노트 종류: {note.noteType}\n시간: {note.time}";
                            rightNoteLabel.UseMarkup = true;
                        }
                    });
                    // Console.WriteLine("{0}번째 노트, {1}초, 오프셋 {2}", index, noteTimes[index], setting.offset);
                    index++;
                }
            }
        }
        private void syncMusicAndScale()
        {
            while(player.Playing)
            {
                Thread.Sleep(100);
                Application.Invoke(delegate {
                    timeScale.Value = sw.Elapsed.TotalSeconds;
                    timeLabel.Text = $"{((int)timeScale.Value) / 60}:{((int)timeScale.Value) % 60} / {timeToString(chabo.music.duration)}";
                });
            }
        }
    }
}