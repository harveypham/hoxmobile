// Horse piece and its moving rule.
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
    public class HorsePiece : Piece
    {
        public HorsePiece(PieceColor c, Position p) : base(PieceType.Horse, c, p) { }

        public override Piece Clone()
        {
            return new HorsePiece(m_color, m_position.Clone());
        }

        public override bool CanMoveTo(Position newPos)
        {
            /* Is a 2-1-rectangle move? */

            // Make sure there is no piece that hinders the move if the move
            // start from one of the four 'neighbors' below.
            //
            //          top
            //           ^
            //           |
            //  left <-- +  ---> right
            //           |
            //           v
            //         bottom
            // 

            Position neighbor;

            // If the new position is on TOP.
            if ((m_position.Y - 2) == newPos.Y && Math.Abs(newPos.X - m_position.X) == 1)
            {
                neighbor = new Position(m_position.X, m_position.Y - 1);
            }
            // If the new position is at the BOTTOM.
            else if ((m_position.Y + 2) == newPos.Y && Math.Abs(newPos.X - m_position.X) == 1)
            {
                neighbor = new Position(m_position.X, m_position.Y + 1);
            }
            // If the new position is on the RIGHT.
            else if ((m_position.X + 2) == newPos.X && Math.Abs(newPos.Y - m_position.Y) == 1)
            {
                neighbor = new Position(m_position.X + 1, m_position.Y);
            }
            // If the new position is on the LEFT.
            else if ((m_position.X - 2) == newPos.X && Math.Abs(newPos.Y - m_position.Y) == 1)
            {
                neighbor = new Position(m_position.X - 1, m_position.Y);
            }
            else
            {
                return false;
            }

            // If the neighbor exists, then the move is invalid.
            if (m_referee.HasPieceAt(neighbor))
                return false;

            // NOTE: The caller will check if the captured piece (if any) is allowed.

            return true;
        }

        public override List<Position> GetPotentialNextPositions()
        {
            List<Position> positions = new List<Position>();

            // ... Check for the 8 possible positions.
            positions.Add(new Position(m_position.X - 1, m_position.Y - 2));
            positions.Add(new Position(m_position.X - 1, m_position.Y + 2));
            positions.Add(new Position(m_position.X - 2, m_position.Y - 1));
            positions.Add(new Position(m_position.X - 2, m_position.Y + 1));
            positions.Add(new Position(m_position.X + 1, m_position.Y - 2));
            positions.Add(new Position(m_position.X + 1, m_position.Y + 2));
            positions.Add(new Position(m_position.X + 2, m_position.Y - 1));
            positions.Add(new Position(m_position.X + 2, m_position.Y + 1));
            return positions;
        }

    }
}
