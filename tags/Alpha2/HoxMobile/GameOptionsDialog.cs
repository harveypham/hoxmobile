using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HoxMobile
{
    public partial class GameOptionsDialog : Form
    {
        public enum GameMode
        {
            Human,
            Machine,
        }

        public class GameOptions
        {
            private GameMode m_mode = GameMode.Machine;
            private int m_level = 3;

            public GameOptions()
            {
            }

            public void Set( GameOptions options )
            {
                this.m_mode = options.m_mode;
                this.m_level = options.m_level;
            }

            public GameMode Mode
            {
                get { return m_mode; }
                set { m_mode = value; }
            }

            public int Level
            {
                get { return m_level; }
                set { m_level = value; }
            }
        }

        public GameOptionsDialog()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (m_gameOptions.Mode == GameMode.Human)
                choiceHuman.Checked = true;
            trackLevel.Value = m_gameOptions.Level;
        }

        private void menuOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            m_gameOptions.Mode = (this.choiceAI.Checked ? GameMode.Machine : GameMode.Human);
            m_gameOptions.Level = trackLevel.Value;
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private GameOptions m_gameOptions = new GameOptions();
        public GameOptions Options
        {
            get { return m_gameOptions; }
            set
            {
                m_gameOptions.Set(value);
                if (m_gameOptions.Mode == GameMode.Human)
                    choiceHuman.Checked = true;
                trackLevel.Value = m_gameOptions.Level;
            }
        }

    }

}