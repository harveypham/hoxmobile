// Implements a simple RadioButton class
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace HoxMobile.Forms
{
    public partial class RadioButton : UserControl
    {
        public RadioButton()
        {
            InitializeComponent();
        }

        public bool Checked
        {
            get { return m_checked; }
            set
            {
                if (m_checked != value)
                {
                    // Clear the value of the other guys.
                    if (value)
                    {
                        DisableSibling();
                    }
                    m_checked = value;
                    DrawButton();
                }
            }
        }

        public string Label
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            DrawButton(g);
            DrawText(g);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button != MouseButtons.Left)
                return;

            if (Focused)
                return;

            DisableSibling();
            m_checked = true;
            // If we didn't steal the focus from sibling, switch the focus
            // because user clicked on this.
            if (!Focused)
                Focus();
            DrawButton();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            using (Graphics g = CreateGraphics())
            {
                g.Clip = new Region(new Rectangle(16, 0, Width - 16, Height));
                g.Clear(BackColor);
                DrawText(g);
                g.ResetClip();
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DrawButton();
        }

        private void DisableSibling()
        {
            if (Parent != null && Parent.Controls != null)
            {
                foreach (Control ctl in Parent.Controls)
                {
                    // Skipping this control
                    if (ctl == this)
                        continue;

                    // Skipping controls that are not RadioButton
                    if (!(ctl is RadioButton))
                        continue;

                    RadioButton sibling = (RadioButton)ctl;
                    if (!sibling.Checked)
                        continue;

                    sibling.m_checked = false;
                    if (!sibling.Focused)
                    {
                        sibling.DrawButton();
                    }
                    else
                    {  
                        // Steal focus from sibling
                        Focus();
                        // sibling is redrawn by OnLostFocus;
                    }
                }
            }
        }

        private void DrawButton()
        {
            using (Graphics g = CreateGraphics())
            {
                DrawButton(g);
            }
        }
       
        private void DrawButton(Graphics g)
        {
            Bitmap bitmap = new Bitmap(16, 16);
            using (Graphics gOff = Graphics.FromImage(bitmap))
            {
                Icon icon;
                if (m_checked)
                {
                    if (Focused)
                    {
                        icon = Properties.Resources.RadioButton_Focused;
                    }
                    else
                    {
                        icon = Properties.Resources.RadioButton_Checked;
                    }
                }
                else
                {
                    icon = Properties.Resources.RadioButton_Unchecked;
                }
                gOff.Clear(BackColor);
                gOff.DrawIcon(icon, 0, 0);
            }
            g.DrawImage(bitmap, 0, 0);
        }

        private void DrawText(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                g.DrawString(this.Text, this.Font, brush, 22, 0);
            }
        }

    }

}
