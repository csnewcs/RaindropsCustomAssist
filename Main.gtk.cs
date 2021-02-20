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
        Entry musicPath = new Entry();
        Entry jsonPath = new Entry();
        public MainPage() : base("방울비 커스텀 보조 프로그램")
        {
            DeleteEvent += delegate {Application.Quit();};
            SetDefaultSize(750, 400);

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
            Button openMusic = new Button("음악 파일 열기");
            Button openJson = new Button("채보 파일 열기");
            openMusic.Clicked += OpenMusicClicked;
            openJson.Clicked += OpenJsonClicked;
            grid.Attach(musicPath, 1, 1, 2, 1);
            grid.Attach(openMusic, 3, 1, 1, 1);
            grid.Attach(jsonPath, 1, 2, 2, 1);
            grid.Attach(openJson, 3, 2, 1, 1);
            return grid;
        }
    }
}