// Properties and Windows Form Designer generated code
//
// Copyright (C) 2009 Harvey Pham (harveypham@playxiangqi.com)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Drawing;

namespace HoxMobile
{
    partial class HoxChessForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu = new System.Windows.Forms.MainMenu();
            this.miStart = new System.Windows.Forms.MenuItem();
            this.miMain = new System.Windows.Forms.MenuItem();
            this.miGameOptions = new System.Windows.Forms.MenuItem();
            this.miQuit = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.Add(this.miStart);
            this.mainMenu.MenuItems.Add(this.miMain);
            // 
            // miStart
            // 
            this.miStart.Text = "Start";
            this.miStart.Click += new System.EventHandler(this.miStart_Click);
            // 
            // miMain
            // 
            this.miMain.MenuItems.Add(this.miGameOptions);
            this.miMain.MenuItems.Add(this.miQuit);
            this.miMain.Text = "Menu";
            // 
            // miGameOptions
            // 
            this.miGameOptions.Text = "Game Options...";
            this.miGameOptions.Click += new System.EventHandler(this.miGameOptions_Click);
            // 
            // miQuit
            // 
            this.miQuit.Text = "Quit";
            this.miQuit.Click += new System.EventHandler(this.miQuit_Click);
            // 
            // HoxChessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(176, 180);
            this.Menu = this.mainMenu;
            this.Name = "HoxChessForm";
            this.Text = "HoxMobile";
            this.ResumeLayout(false);

        }

        #endregion


        private XiangqiSettings mSettings = new XiangqiSettings();
        private BoardSize mBoardSize = new BoardSize(false);

        private const int VertCellCount = 9;
        private const int HorzCellCount = 8;

        private static readonly int[,] mSeatLocations = new int[14, 2] {
                    { 1, 2 }, { 7, 2 },
                    { 0, 3 }, { 2, 3 }, { 4, 3 }, { 6, 3 }, { 8, 3 },
                    { 0, 6 }, { 2, 6 }, { 4, 6 }, { 6, 6 }, { 8, 6 },
                    { 1, 7 }, { 7, 7 },
                };

        private Pen linePen;
        private Position mLastMove;
        private bool mGameOver = false;
        private ChessReferee mReferee;
        private PieceImage mImages = new PieceImage();

        private System.Windows.Forms.MenuItem miStart;
        private System.Windows.Forms.MenuItem miMain;
        private System.Windows.Forms.MenuItem miQuit;
        private System.Windows.Forms.MenuItem miGameOptions;
        private System.Windows.Forms.MainMenu mainMenu;
    }

}

