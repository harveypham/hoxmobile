namespace HoxMobile
{
    partial class GameOptionsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu;

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
            this.menuOK = new System.Windows.Forms.MenuItem();
            this.menuCancel = new System.Windows.Forms.MenuItem();
            this.lblLevel = new System.Windows.Forms.Label();
            this.trackLevel = new HoxMobile.Forms.HTrackBar();
            this.choiceHuman = new HoxMobile.Forms.RadioButton();
            this.choiceAI = new HoxMobile.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.Add(this.menuOK);
            this.mainMenu.MenuItems.Add(this.menuCancel);
            // 
            // menuOK
            // 
            this.menuOK.Text = "OK";
            this.menuOK.Click += new System.EventHandler(this.menuOK_Click);
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Cancel";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // lblLevel
            // 
            this.lblLevel.Location = new System.Drawing.Point(36, 26);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(97, 15);
            this.lblLevel.Text = "Level";
            // 
            // trackLevel
            // 
            this.trackLevel.BackColor = System.Drawing.SystemColors.Window;
            this.trackLevel.Location = new System.Drawing.Point(31, 43);
            this.trackLevel.Max = 5;
            this.trackLevel.Min = 1;
            this.trackLevel.Name = "trackLevel";
            this.trackLevel.Size = new System.Drawing.Size(142, 39);
            this.trackLevel.TabIndex = 2;
            this.trackLevel.Value = 3;
            // 
            // choiceHuman
            // 
            this.choiceHuman.BackColor = System.Drawing.SystemColors.Window;
            this.choiceHuman.Checked = false;
            this.choiceHuman.Label = "Play with human";
            this.choiceHuman.Location = new System.Drawing.Point(4, 96);
            this.choiceHuman.Name = "choiceHuman";
            this.choiceHuman.Size = new System.Drawing.Size(124, 21);
            this.choiceHuman.TabIndex = 3;
            // 
            // choiceAI
            // 
            this.choiceAI.BackColor = System.Drawing.SystemColors.Window;
            this.choiceAI.Checked = true;
            this.choiceAI.Label = "Play with AI";
            this.choiceAI.Location = new System.Drawing.Point(4, 5);
            this.choiceAI.Name = "choiceAI";
            this.choiceAI.Size = new System.Drawing.Size(124, 21);
            this.choiceAI.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(0, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 48);
            this.label1.Text = "NOTE: Settings take effect on next game.";
            // 
            // GameOptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(176, 180);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackLevel);
            this.Controls.Add(this.choiceHuman);
            this.Controls.Add(this.choiceAI);
            this.Controls.Add(this.lblLevel);
            this.KeyPreview = true;
            this.Menu = this.mainMenu;
            this.Name = "GameOptionsDialog";
            this.Text = "Game Options";
            this.ResumeLayout(false);

        }

        #endregion

        private HoxMobile.Forms.RadioButton choiceAI;
        private System.Windows.Forms.Label lblLevel;
        private HoxMobile.Forms.HTrackBar trackLevel;
        private System.Windows.Forms.MenuItem menuOK;
        private System.Windows.Forms.MenuItem menuCancel;
        private HoxMobile.Forms.RadioButton choiceHuman;
        private System.Windows.Forms.Label label1;

    }

}