// A simple Horizontal TrackBar
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

    public partial class HTrackBar : UserControl
    {
        private int m_distance;
        private int m_barDistance;
        private static readonly Pen s_controlPen = new Pen(Color.Black);

        public HTrackBar()
        {
            InitializeComponent();
            NormalizeValue();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            RecalMetrics();
            g.DrawRectangle(s_controlPen, 7, 8, m_barDistance-1, 3);
            g.DrawLine(s_controlPen, 11, 21, 11, 25);
            for (int i = 1; i < Range - 1; i++)
            {
                int x = 11 + i * m_distance;
                g.DrawLine(s_controlPen, x, 21, x, 24);
            }
            int lastX = 11 + ( Range - 1 ) * m_distance;
            g.DrawLine(s_controlPen, lastX, 21, lastX, 25);
            DrawMarker(g);

            if (this.Focused)
            {
                DrawFocus(g, true);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            DrawFocus(true);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            DrawFocus(false);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                int curX = 11 + (m_value - m_min) * m_distance;
                if (e.X < curX - 4)
                {
                    Value = m_value - 1;
                }
                else if (e.X > curX + 4)
                {
                    Value = m_value + 1;
                }
                Focus();
            }
        }

        private void DrawMarker(Graphics g)
        {
            int x = 11 + (m_value - m_min) * m_distance;
            Point[] points = new Point[5];
            points[0] = new Point(x - 4, 2);
            points[1] = new Point(x + 4, 2);
            points[2] = new Point(x + 4, 15);
            points[3] = new Point(x, 19);
            points[4] = new Point(x - 4, 15);
            g.DrawPolygon(s_controlPen, points);
            g.DrawRectangle(new Pen(BackColor), x - 3, 8, 6, 3);
        }

        private void EraseMarker(Graphics g)
        {
            int x = 11 + (m_value - m_min) * m_distance;
            Point[] points = new Point[5];
            points[0] = new Point(x - 4, 2);
            points[1] = new Point(x + 4, 2);
            points[2] = new Point(x + 4, 15);
            points[3] = new Point(x, 19);
            points[4] = new Point(x - 4, 15);
            using (Pen erasePen = new Pen(BackColor))
            {
                g.DrawPolygon(erasePen, points);
            }
            if (m_value == m_min)
            {
                g.DrawLine(s_controlPen, 7, 8, 16, 8);
                g.DrawLine(s_controlPen, 7, 11, 16, 11);
                g.DrawLine(s_controlPen, 7, 8, 7, 11);
            }
            else if (m_value == m_max)
            {
                g.DrawLine(s_controlPen, m_barDistance - 3, 8, 6 + m_barDistance, 8);
                g.DrawLine(s_controlPen, m_barDistance - 3, 11, 6 + m_barDistance, 11);
                g.DrawLine(s_controlPen, 6 + m_barDistance, 8, 6 + m_barDistance, 11);
            }
            else
            {
                g.DrawLine(s_controlPen, x - 4, 8, x + 4, 8);
                g.DrawLine(s_controlPen, x - 4, 11, x + 4, 11);
            }
        }

        private void DrawFocus(bool focus)
        {
            using (Graphics g = CreateGraphics())
            {
                DrawFocus(g, focus);
            }
        }

        private void DrawFocus( Graphics g, bool focus)
        {
            using (Pen pen = new Pen(focus ? Color.Blue : BackColor))
            {
                g.DrawRectangle(pen, 0, 0, m_barDistance + 13, 29);
            }
        }

        private int NormalizeValue(int value)
        {
            if (value < m_min)
            {
                return m_min;
            }
            else if (value > m_max)
            {
                return m_max;
            }
            return value;
        }

        private void NormalizeValue()
        {
            m_value = NormalizeValue(m_value);
        }

        public int Max
        {
            get { return m_max; }
            set { m_max = value; }
        }

        public int Min
        {
            get { return m_min; }
            set { m_min = value; }
        }

        private int Range
        {
            get { return m_max - m_min + 1; }
        }

        public int Value
        {
            get { return m_value; }
            set
            {
                value = NormalizeValue(value);
                if (m_value != value)
                {
                    using (Graphics g = CreateGraphics())
                    {
                        EraseMarker(g);
                        m_value = value;
                        DrawMarker(g);
                    }
                }
            }
        }

        private void RecalMetrics()
        {
            if (Range == 1)
            {
                m_distance = 0;
                m_barDistance = 0;
            }
            else
            {
                m_distance = (Size.Width - 23) / (Range - 1);
                m_barDistance = m_distance * (Range - 1) + 9;
            }
        }
   

    }

}
