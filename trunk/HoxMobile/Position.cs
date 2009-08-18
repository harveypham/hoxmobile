// A position on the board.
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

namespace HoxMobile
{
    public enum PieceColor
    {
        None = 1,
        Red = 2,
        Black = 3
    }

    public enum PieceType
    {
        King = 0,               // King (or General)
        Advisor,                // Advisor (or Guard, or Mandarin)
        Elephant,               // Elephant (or Ministers)
        Chariot,                // Chariot ( Rook, or Car)
        Horse,                  // Horse ( Knight )
        Cannon,                 // Canon
        Pawn                    // Pawn (or Soldier)
    }

    public enum MoveResult
    {
        Invalid,
        ChangeSelect,
        Move,
        Winning,
        Draw
    }

    public class Position
    {
        public static readonly Position OffBoard = new Position(-1, -1);
        public const int MaxX = 8;
        public const int MaxY = 9;

        int m_x;
        int m_y;

        public Position(int x, int y)
        {
            m_x = x;
            m_y = y;
        }

        public Position(Position pos)
        {
            Set(pos);
        }

        public Position()
        {
            m_x = -1;
            m_y = -1;
        }

        public Position Clone()
        {
            return new Position(this);
        }

        public bool Equals(Position pos)
        {
            return m_x == pos.m_x
                && m_y == pos.m_y;
        }

        public void Set(Position pos)
        {
            m_x = pos.m_x;
            m_y = pos.m_y;
        }

        public void Set(int x, int y)
        {
            m_x = x;
            m_y = y;
        }

        public bool IsValid()
        {
            return (m_x >= 0 && m_x <= MaxX && m_y >= 0 && m_y <= MaxY);
        }

        public bool IsInsidePalace(PieceColor color)
        {
            switch( color )
            {
                case PieceColor.Black:
                    return (m_x >= 3 && m_x <= 5 && m_y >= 0 && m_y <= 2);

                case PieceColor.Red:
                    return (m_x >= 3 && m_x <= 5 && m_y >= 7 && m_y <= 9);

                default:
                    return false;
            }
        }

        public bool IsInsideCountry(PieceColor color)
        {
            if (color == PieceColor.Black)
            {
                return (m_y >= 0 && m_y <= 4);
            }
            else  // Red?
            {
                return (m_y >= 5 && m_y <= 9);
            }
        }

        public int X
        {
            get { return m_x; }
        }

        public int Y
        {
            get { return m_y; }
        }

        public void Transform()
        {
            m_x = MaxX - m_x;
            m_y = MaxY - m_y;
        }

    }

}
