// Board metrics
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
using System.Text;
using System.Drawing;

namespace HoxMobile
{

    class BoardSize
    {
        private Size m_clientSize;
        private Rectangle m_board;
        private int m_cellSize;
        private Size m_pieceSize;

        private bool m_inverted = false;

        public BoardSize( bool inverted )
        {
            m_inverted = inverted;

            // Dimension for 240x320 sccreen: 240x266 (0,27)(240,293)
            m_clientSize = new Size(240, 266);
            m_board = new Rectangle(11, 11, 216, 243);
            m_cellSize = 27;
            m_pieceSize = new Size(25, 25);
        }

        public bool Inverted
        {
            get { return m_inverted; }
            set { m_inverted = value; }
        }


        public Size ClientSize
        {
            get { return m_clientSize; }
        }

        /// <summary>
        /// Get board rectangle
        /// </summary>
        public Rectangle Board
        {
            get { return m_board; }
        }

        /// <summary>
        /// Get x-coordinate of the upper left corner of the board
        /// </summary>
        public int X
        {
            get { return m_board.X; }
        }

        /// <summary>
        /// Get y-coordinate of the upper left corner of the board
        /// </summary>
        public int Y
        {
            get { return m_board.Y; }
        }

        public int Top
        {
            get { return m_board.Top; }
        }

        public int Bottom
        {
            get { return m_board.Bottom; }
        }

        public int Left
        {
            get { return m_board.Left; }
        }

        public int Right
        {
            get { return m_board.Right; }
        }

        /// <summary>
        /// Get the width of the board
        /// </summary>
        public int Width
        {
            get { return m_board.Width; }
        }

        /// <summary>
        /// Get the height of the board
        /// </summary>
        public int Height
        {
            get { return m_board.Height; }
        }

        /// <summary>
        /// Get cell size
        /// </summary>
        public int CellSize
        {
            get { return m_cellSize; }
        }

        /// <summary>
        /// Get or set the size of each game piece
        /// </summary>
        public Size PieceSize
        {
            get { return m_pieceSize; }
            set { m_pieceSize = value; }
        }

        /// <summary>
        /// Resize board size to new dimension
        /// </summary>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <returns>true/false: dimension has changed</returns>
        public bool Resize(int width, int height)
        {

            int maxX = width - 2 * m_board.X;
            int maxY = height - 2 * m_board.Y;

            // Cell is square
            int cellSize = Math.Min(maxX / Position.MaxX, maxY / Position.MaxY);
            if (m_cellSize == cellSize)
                return false;

            m_cellSize = cellSize;
            m_board.Width = m_cellSize * Position.MaxX;
            m_board.Height = m_cellSize * Position.MaxY;

            m_clientSize.Width = 2 * m_board.X + m_board.Width;
            m_clientSize.Height = 2 * m_board.Y + m_board.Height;

            return true;
        }

        /// <summary>
        /// Resize board size to new dimension
        /// </summary>
        /// <param name="size">New size</param>
        /// <returns>true/false: dimension has changed</returns>
        public bool Resize(Size size)
        {
            return Resize(size.Width, size.Height);
        }

        /// <summary>
        /// Get center point of a piece at Position
        /// </summary>
        /// <param name="pos">Position of game piece</param>
        /// <returns>Center point</returns>
        public Point GetPoint(Position pos)
        {
            Position screenPos = Transform(pos);
            return new Point(m_board.X + m_cellSize * screenPos.X, m_board.Y + m_cellSize * screenPos.Y);
        }

        public Rectangle GetRectangle(Position pos)
        {
            Point orig = GetPoint(pos);
            int halfPieceSizeWidth = m_pieceSize.Width / 2;
            int halfPieceSizeHeight = m_pieceSize.Height / 2;
            return new Rectangle(orig.X - halfPieceSizeWidth,
                orig.Y - halfPieceSizeHeight,
                m_pieceSize.Width,
                m_pieceSize.Height);
        }

        /// <summary>
        /// Get position of a clicked point
        /// </summary>
        /// <param name="p">A point on board</param>
        /// <returns>Position at this point</returns>
        public Position GetPosition(Point p)
        {
            int halfPieceSizeWidth = m_pieceSize.Width / 2;
            int halfPieceSizeHeight = m_pieceSize.Height / 2;

            if (p.X < m_board.Left - halfPieceSizeWidth
                || p.X > m_board.Right + halfPieceSizeWidth
                || p.Y < m_board.Top - halfPieceSizeHeight
                || p.Y > m_board.Bottom + halfPieceSizeHeight)
            {
                return Position.OffBoard;
            }

            int xPos;
            if (p.X < m_board.Left)
            {
                xPos = 0;
            }
            else if (p.X > m_board.Right)
            {
                xPos = Position.MaxX;
            }
            else
            {

                xPos = (p.X - m_board.Left) / m_cellSize;

                int x1 = m_board.Left + xPos * m_cellSize;
                int x4 = x1 + m_cellSize;
                int x2 = x1 + halfPieceSizeWidth;
                int x3 = x4 - halfPieceSizeWidth;

                if (p.X > x2 && p.X < x3)
                    return Position.OffBoard;

                if (p.X > x3)
                    xPos++;
            }

            int yPos;
            if (p.Y < m_board.Top)
            {
                yPos = 0;
            }
            else if (p.Y > m_board.Bottom)
            {
                yPos = Position.MaxY;
            }
            else
            {
                yPos = (p.Y - m_board.Top) / m_cellSize;

                int y1 = m_board.Top + yPos * m_cellSize;
                int y4 = y1 + m_cellSize;
                int y2 = y1 + halfPieceSizeHeight;
                int y3 = y4 - halfPieceSizeHeight;

                if (p.Y > y2 && p.Y < y3)
                    return Position.OffBoard;

                if (p.Y > y3)
                    yPos++;
            }

            return Transform(new Position(xPos, yPos));
        }

        protected Position Transform(Position pos)
        {
            if (!m_inverted)
                return pos;
            Position screenPos = new Position(pos);
            screenPos.Transform();
            return screenPos;
        }

    }

}
