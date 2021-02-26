using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Gtk;
using Gdk;

using Newtonsoft.Json;

namespace RaindropsCustomAssist
{
    partial class MainPage : Gtk.Window
    {
        Grid mainGrid = new Grid(); //모든 위젯들을 담는 메인 표
        // ================= 프로그램 시작시 보이는 곳====================
        Grid firstGrid;
        Button doneButton = new Button("시작");
        FileChooserButton openMusicButton = new FileChooserButton("음악 파일 열기", FileChooserAction.Open);
        FileChooserButton openJsonButton = new FileChooserButton("Json 파일 열기", FileChooserAction.Open);
        // ===================파일 복사하는 곳에서 보이는 위젯 =================
        Entry customDirectoryEntry = new Entry();
        Button startCopyButton = new Button("복사 시작!");
        //==========================계속 아래쪽에 있을 위젯====================
        ProgressBar doingProgressBar = new ProgressBar();
        

        public MainPage() : base("방울비 커스텀 보조 프로그램")
        {
            DeleteEvent += delegate {Application.Quit();};
            SetDefaultSize(350, 220);
            Resizable = false;
            mainGrid.RowHomogeneous = true;
            mainGrid.ColumnHomogeneous = true;

             firstGrid  = getFirstGrid();//프로그램 켰을 때 뜨는 표, 곡 선택 등
            // List<Grid> editGrids = new List<Grid>(); //채보를 수정하는 기능들을 담는 표들의 묶음
            // List<Grid> settingGrids = new List<Grid>(); //쓸진 모르겠지만 설정

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
            doneButton.Clicked += doneButton_clicked;
            
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
        private Grid getMainGrid()
        {
            Resizable = true;
            SetSizeRequest(500, 400);
            Grid grid = new Grid();
            Stack stack = new Stack();
            stack.AddTitled(getCopyFileGrid(), "커스텀 파일 복사하기", "커스텀 파일 복사하기");
            StackSidebar stackSidebar = new StackSidebar();
            
            stackSidebar.Stack = stack;
            grid.RowHomogeneous = true;
            grid.ColumnHomogeneous = true;
            grid.Attach(stackSidebar, 1, 4, 1, 1);
            grid.Attach(stack, 2, 4, 4, 1);
            // grid.Attach(doingProgressBar, 1, 5, 5, 1);
            return grid;
        }
        private Grid getCopyFileGrid()
        {
            Grid grid = new Grid();
            string username = Environment.UserName;
            string path = "";

            int[] noteCounts = chabo.getNoteCounts();
            Label aboutLabel = new Label($"<big>채보 정보</big>\n\t<b>제목</b>: {chabo.title}\n\t노트 수: {noteCounts[0]}\n\t\t<small>클릭: {noteCounts[1]}</small>\n\t\t<small>휠: {noteCounts[2]}</small>\n\t\t<small>캐치: {noteCounts[3]}</small>\n\t이벤트 수: {chabo.events.Count}\n\n<big>음악 정보</big>\n\t<b>제목</b>: {chabo.music.title}\n\t아티스트: {string.Join(", ", chabo.music.artists)}\n\t길이: {(int)chabo.music.duration.TotalMinutes}:{chabo.music.duration.Seconds}");
            aboutLabel.UseMarkup = true;
            // string summary = ;
            

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //윈도우이면 그냥 기본 경로
            {
                path = $"C:/Users/{username}/AppData/LocalLow/Poobool/Raindrops/Custom"; 
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) //리눅스이면 Proton쪽 경로
            {
                path = $"/home/{username}/.steam/steam/steamapps/compatdata/1268860/pfx/drive_c/users/steamuser/AppData/LocalLow/Poobool/Raindrops/Custom";
            }
            customDirectoryEntry.Text = path;

            grid.Margin = 20;
            grid.RowHomogeneous = true;
            grid.ColumnHomogeneous = true;
            grid.RowSpacing = 15;
            grid.ColumnSpacing = 15;

            customDirectoryEntry.Valign = Align.Start;
            startCopyButton.Valign = Align.Start;
            aboutLabel.Halign = Align.Start;
            
            startCopyButton.Clicked += startCopyButton_clicked;

            ScrolledWindow aboutScoll = new ScrolledWindow();
            aboutScoll.Add(aboutLabel);
            aboutLabel.Valign = Align.Start;

            grid.Attach(customDirectoryEntry, 1, 1, 2, 1);
            grid.Attach(startCopyButton, 3, 1, 1, 1);
            grid.Attach(aboutScoll, 1, 2, 3, 6);

            customDirectoryEntry.Name = "customDirectoryEntryEntry";
            startCopyButton.Name = "StartCopyButton";
            return grid;
        }

    }
}