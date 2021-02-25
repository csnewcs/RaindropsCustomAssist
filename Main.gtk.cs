using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Gtk;
using Gdk;

using Newtonsoft.Json;

namespace RaindropsCustomAssist
{
    partial class MainPage : Gtk.Window
    {
        Button doneButton = new Button("시작");
        FileChooserButton openMusicButton = new FileChooserButton("음악 파일 열기", FileChooserAction.Open);
        FileChooserButton openJsonButton = new FileChooserButton("Json 파일 열기", FileChooserAction.Open);

        public MainPage() : base("방울비 커스텀 보조 프로그램")
        {
            DeleteEvent += delegate {Application.Quit();};
            SetDefaultSize(350, 220);
            Resizable = false;

            Grid mainGrid = new Grid(); //모든 위젯들을 담는 메인 표
            Grid firstGrid = getFirstGrid(); //프로그램 켰을 때 뜨는 표, 곡 선택 등
            List<Grid> editGrids = new List<Grid>(); //채보를 수정하는 기능들을 담는 표들의 묶음
            List<Grid> settingGrids = new List<Grid>(); //쓸진 모르겠지만 설정

            Add(firstGrid);
            Show();
            firstGrid.ShowAll();
        }
        private Grid getFirstGrid()
        {
            Grid grid = new Grid();
            Label openMusicLable = new Label("음악 파일 열기");
            Label openJsonLable = new Label("JSON 파일 열기");
            doneButton.Sensitive = false;

            openMusicButton.FileSet += openMusicButton_opened;
            openJsonButton.FileSet += openJsonButton_opened;
            
            FileFilter musicFileFilter = new FileFilter(); musicFileFilter.AddPattern("*.mp3"); musicFileFilter.AddPattern("*.wav");
            FileFilter jsonFileFilter = new FileFilter(); jsonFileFilter.AddPattern("*.json");
            openMusicButton.AddFilter(musicFileFilter);
            openJsonButton.AddFilter(jsonFileFilter);

            grid.Attach(openMusicLable, 1, 1, 1, 1);
            grid.Attach(openJsonLable, 1, 2, 1,1);
            grid.Attach(openMusicButton, 2, 1, 1, 1);
            grid.Attach(openJsonButton, 2, 2, 1, 1);
            grid.Attach(doneButton, 3, 1, 1, 2);
            grid.RowHomogeneous = true;
            grid.ColumnHomogeneous = true;
            grid.RowSpacing = 15;
            grid.ColumnSpacing = 15;
            grid.Margin = 20;

            //CSS를 위한 이름 및 클래스 설정
            openMusicLable.Name = "OpenMusicLable";
            openMusicLable.StyleContext.AddClass("OpenFileLable");
            openMusicButton.Name = "OpenMusicButton";
            openMusicButton.StyleContext.AddClass("OpenFileButton");
            openJsonLable.Name = "OpenJsonLable";
            openJsonLable.StyleContext.AddClass("OpenFileLable");
            openJsonButton.Name = "OpenJsonButton";
            openJsonButton.StyleContext.AddClass("OpenFileButton");
            doneButton.Name = "DoneButton";

            return grid;
        }
    }
}