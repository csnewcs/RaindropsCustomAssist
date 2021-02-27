using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Gtk;
// using Gdk;

namespace RaindropsCustomAssist
{
    partial class MainPage
    {
        Chabo chabo;
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
                
                jsonFile.CopyTo($"{customDirectoryEntry.Text}/{realName}.json");
                setFraction(0.5);
                musicFile.CopyTo($"{customDirectoryEntry.Text}/{musicFile.Name}");
                setFraction(1);

                showDialog(new MessageDialog(null, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, false, "작업을 완료했습니다!"));
            });
            thread.Start();
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
    }
}