using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using NetCoreAudio;

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
        //====================듣기==========================
        Label  timeLabel;
        Label leftNoteLabel = new Label();
        Label rightNoteLabel = new Label();
        Scale timeScale;
        Button playPauseButton;
        CheckButton musicSound = new CheckButton("음악 소리");
        CheckButton noteSound = new CheckButton("노트 소리");
            CheckButton wheelNotes = new CheckButton("휠");
            CheckButton catchNotes = new CheckButton("캐치");
            CheckButton clickNotes = new CheckButton("클릭");
        //======================메타데이터 설정====================
        Image chaboImage = new Image();
        SpinButton levelSpinButton = new SpinButton(new Adjustment(1, 1, 21, 1, 1, 1), 1, 0); //레벨 선택 SpinButton(최대 20)
        Entry titleEntry = new Entry("");
        SpinButton bpmSpinButton = new SpinButton(new Adjustment(150, 1, 5001, 1, 1, 1), 1, 0);
        //==========================설정==========================
        Label sinkLabel;

        //==========================계속 아래쪽에 있을 위젯====================
        ProgressBar doingProgressBar = new ProgressBar();
        

        public MainPage() : base("방울비 커스텀 보조 프로그램")
        {
            DeleteEvent += delegate {Application.Quit(); player.Stop();};
            SetDefaultSize(350, 220);
            Resizable = false;
            mainGrid.RowHomogeneous = true;
            mainGrid.ColumnHomogeneous = true;

            CssProvider cssProvider = new CssProvider();
            cssProvider.LoadFromPath("design.css");
            StyleContext.AddProviderForScreen(this.Screen, cssProvider, 800);

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
            Label openMusicLabel = new Label("음악 파일 열기");
            Label openJsonLabel = new Label("JSON 파일 열기");
            doneButton.Sensitive = false;

            openMusicButton.FileSet += openMusicButton_opened;
            openJsonButton.FileSet += openJsonButton_opened;
            doneButton.Clicked += doneButton_clicked;
            
            FileFilter musicFileFilter = new FileFilter(); musicFileFilter.AddPattern("*.mp3"); musicFileFilter.AddPattern("*.wav");
            FileFilter jsonFileFilter = new FileFilter(); jsonFileFilter.AddPattern("*.json");
            openMusicButton.AddFilter(musicFileFilter);
            openJsonButton.AddFilter(jsonFileFilter);

            grid.Attach(openMusicLabel, 1, 1, 1, 1);
            grid.Attach(openJsonLabel, 1, 2, 1,1);
            grid.Attach(openMusicButton, 2, 1, 1, 1);
            grid.Attach(openJsonButton, 2, 2, 1, 1);
            grid.Attach(doneButton, 3, 1, 1, 2);
            grid.RowHomogeneous = true;
            grid.ColumnHomogeneous = true;
            grid.RowSpacing = 15;
            grid.ColumnSpacing = 15;
            grid.Margin = 20;

            //CSS를 위한 이름 및 클래스 설정
            openMusicLabel.Name = "OpenMusicLabel";
            openMusicLabel.StyleContext.AddClass("OpenFileLabel");
            openMusicButton.Name = "OpenMusicButton";
            openMusicButton.StyleContext.AddClass("OpenFileButton");
            openJsonLabel.Name = "OpenJsonLable";
            openJsonLabel.StyleContext.AddClass("OpenFileLabel");
            openJsonButton.Name = "OpenJsonButton";
            openJsonButton.StyleContext.AddClass("OpenFileButton");
            doneButton.Name = "DoneButton";

            return grid;
        }
        private Grid getMainGrid()
        {
            setting = new Setting();
            Resizable = true;
            SetSizeRequest(500, 400);
            Grid grid = new Grid();
            Stack stack = new Stack();
            stack.AddTitled(getCopyFileGrid(), "커스텀 파일 복사하기", "커스텀 파일 복사하기");
            stack.AddTitled(getListenGrid(), "듣기", "듣기");
            stack.AddTitled(getSetMetaDataGrid(), "데이터 수정", "데이터 수정");
            stack.AddTitled(getSettingGrid(), "설정", "설정");
            StackSidebar stackSidebar = new StackSidebar();
            
            doingProgressBar.Valign = Align.End;
            stackSidebar.Stack = stack;
            grid.RowHomogeneous = true;
            grid.ColumnHomogeneous = true;
            grid.Attach(stackSidebar, 1, 1, 1, 6);
            grid.Attach(stack, 2, 1, 4, 6);
            grid.Attach(doingProgressBar, 1, 7, 5, 1);

            doingProgressBar.Name = "DoingProgressBar";
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
        private Grid getListenGrid()
        {
            setOffset();

            Grid grid = new Grid();
            TimeSpan musicDuration = chabo.music.duration;
            timeScale = new Scale(Orientation.Horizontal, new Adjustment(0, 0, (int)musicDuration.TotalSeconds, 0, 1, 0));
                timeScale.Digits = 0;
                timeScale.DrawValue = false;
                timeScale.ChangeValue += timeScale_valueChanged;
            timeLabel = new Label($"0:00 / {timeToString(musicDuration)}");
            leftNoteLabel.Halign = Align.Start;
            rightNoteLabel.Halign = Align.Start;
            playPauseButton = new Button("▶️"); //▶️ ⏸️ ⏸
            playPauseButton.Clicked  += playPauseButton_clicked;

            Grid time = new Grid();
                time.ColumnHomogeneous = true;
                time.Attach(timeScale, 1, 1, 1, 1);
                time.Attach(timeLabel, 1 ,2 ,1, 1);
            Grid listenTo = new Grid();
                Grid selectNotes = new Grid();           
                    selectNotes.Attach(clickNotes, 1, 1, 1, 1);
                    selectNotes.Attach(wheelNotes, 1, 2, 1, 1);
                    selectNotes.Attach(catchNotes, 1, 3, 1, 1);
                listenTo.Attach(musicSound, 1, 1, 2, 1);
                listenTo.Attach(noteSound, 1, 2, 2, 1);
                listenTo.Attach(selectNotes, 2, 3, 1, 1);
                listenTo.Margin = 15;
            ScrolledWindow listenToScroll = new ScrolledWindow();
            listenToScroll.Add(listenTo);            
            Frame listenToFrame = new Frame("들을 소리");
            listenToFrame.Add(listenToScroll);

            grid.Margin = 20;
            grid.RowSpacing = 20;
            grid.ColumnSpacing = 20;
            grid.ColumnHomogeneous = true;
            grid.RowHomogeneous = true;

            // grid.Attach(timeScale, 1, 1, 3, 1);
            // grid.Attach(timeLabel, 1, 2, 3, 1);
            grid.Attach(time, 1, 1, 6, 1);
            grid.Attach(leftNoteLabel, 1, 2, 3, 1);
            grid.Attach(rightNoteLabel, 4, 2, 3, 1);
            grid.Attach(playPauseButton, 1, 3, 3, 1);
            grid.Attach(listenToFrame, 7, 1, 3, 3);
            return grid;
        }
        private Grid getSettingGrid()
        {
            Grid grid = new Grid();
                Frame sinkFrame = new Frame("싱크 설정");
                    Grid sinkGrid = new Grid();
                        sinkLabel = new Label("싱크를 설정하려면 버튼을 누르세요");
                        Button setSinkButton = new Button("싱크 설정하기");
                        setSinkButton.Clicked += setSinkButton_clicked;
                    sinkGrid.Attach(sinkLabel, 1, 1, 3, 1);
                    sinkGrid.Attach(setSinkButton, 4, 1, 1, 1);
                sinkFrame.Add(sinkGrid);
            grid.Attach(sinkFrame, 1, 1, 1, 1);            
            return grid;
        }
        private Grid getSetMetaDataGrid()
        {
            Grid grid = new Grid();
            Label title = new Label("<big><bold>제목</bold></big>"); title.UseMarkup = true;
            grid.Attach(title, 1, 1, 1, 1);
            grid.Attach(titleEntry, 2, 1, 3, 1);
            Label bpm = new Label("bpm");
            grid.Attach(bpm, 1, 2, 1, 1);
            grid.Attach(bpmSpinButton, 2, 2, 3, 1);
            
            return grid;
        }
        
        private void setOffset()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Player player = new Player();
            player.Play("note.mp3");
            sw.Stop();
            player.Stop();
            offset = sw.ElapsedTicks;
            // Console.WriteLine("설정된 오프셋: {0}", offset);
        }
    }
}