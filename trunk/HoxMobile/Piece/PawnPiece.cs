// Pawn piece and its moving rule.
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
    public class PawnPiece : Piece
    {
        public PawnPiece(PieceColor c, Position p) : base(PieceType.Pawn, c, p) { }

        public override Piece Clone()
        {
            return new PawnPiece(m_color, m_position.Clone());
        }

        public override bool CanMoveTo(Position newPos)
        {

            // Within the country...?
            if (newPos.IsInsideCountry(m_color))
            {
                // Can only move up.
                if (newPos.X == m_position.X &&
                   ((m_color == PieceColor.Black && newPos.Y == m_position.Y + 1)
                     || m_color == PieceColor.Red && newPos.Y == m_position.Y - 1))
                {
                    return true;
                }
            }
            // Outside the country (alread crossed the 'river')
            else
            {
                // Only horizontally (LEFT or RIGHT)
                // ... or 1-way-UP vertically.
                if ((newPos.Y == m_position.Y
                            && Math.Abs(newPos.X - m_position.X) == 1)
                    || (newPos.X == m_position.X
                          && (m_color == PieceColor.Black && newPos.Y == m_position.Y + 1
                            || m_color == PieceColor.Red && newPos.Y == m_position.Y - 1)))
                {
                    return true;
                }
            }

            return false;
        }

        public override List<Position> GetPotentialNextPositions()
        {
            List<Position> positions = new List<Position>();

            // ... Simply use the 4 possible positions.
            positions.Add(new Position(m_position.X, m_position.Y - 1));
            positions.Add(new Position(m_position.X, m_position.Y + 1));
            positions.Add(new Position(m_position.X - 1, m_position.Y));
            positions.Add(new Position(m_position.X + 1, m_position.Y));

            return positions;
        }

    }
}
