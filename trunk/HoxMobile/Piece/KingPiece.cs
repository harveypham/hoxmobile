// King piece and its moving rule.
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
    public class KingPiece : Piece
    {
        public KingPiece(PieceColor c, Position p) : base(PieceType.King, c, p) { }

        public override Piece Clone()
        {
            return new KingPiece(m_color, m_position.Clone());
        }

        public override bool CanMoveTo(Position newPos)
        {

            // Within the palace...?
            if (!newPos.IsInsidePalace(m_color))
                return false;

            // Is a 1-cell horizontal or vertical move?
            if (!   (  (Math.Abs(newPos.X - m_position.X) == 1 && newPos.Y == m_position.Y)
                    || (newPos.X == m_position.X && Math.Abs(newPos.Y - m_position.Y) == 1)))
            {
                return false;
            }

            // NOTE: The caller will check if the captured piece (if any) is allowed.

            return true;
        }

        public override List<Position> GetPotentialNextPositions()
        {
            List<Position> positions = new List<Position>();

            // ... Simply use the 4 possible positions.
            positions.Add(new Position(m_position.X, (m_position.Y - 1)));
            positions.Add(new Position(m_position.X, m_position.Y + 1));
            positions.Add(new Position(m_position.X - 1, m_position.Y));
            positions.Add(new Position(m_position.X + 1, m_position.Y));

            return positions;
        }
    }
}