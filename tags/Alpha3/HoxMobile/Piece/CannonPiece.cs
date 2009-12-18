// Cannon piece and its moving rule.
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
    public class CannonPiece : Piece
    {
        public CannonPiece(PieceColor c, Position p) : base(PieceType.Cannon, c, p) { }

        public override Piece Clone()
        {
            return new CannonPiece( m_color, m_position.Clone());
        }

        public override bool CanMoveTo(Position newPos)
        {
            // Is a horizontal or vertical move?
            if (!((newPos.X != m_position.X && newPos.Y == m_position.Y)
                  || (newPos.X == m_position.X && newPos.Y != m_position.Y)))
            {
                return false;
            }

            // Make sure there is no piece that hinders the move from the current
            // position to the new.
            //
            //          top
            //           ^
            //           |
            //  left <-- +  ---> right
            //           |
            //           v
            //         bottom
            // 

            List<Position> middlePieces = new List<Position>();
            Position pPos;
            int i;

            // If the new position is on TOP.
            if (newPos.Y < m_position.Y)
            {
                for (i = newPos.Y + 1; i < m_position.Y; ++i)
                {
                    pPos = new Position(m_position.X, i);
                    middlePieces.Add(pPos);
                }
            }
            // If the new position is on the RIGHT.
            else if (newPos.X > m_position.X)
            {
                for (i = m_position.X + 1; i < newPos.X; ++i)
                {
                    pPos = new Position(i, m_position.Y);
                    middlePieces.Add(pPos);
                }
            }
            // If the new position is at the BOTTOM.
            else if (newPos.Y > m_position.Y)
            {
                for (i = m_position.Y + 1; i < newPos.Y; ++i)
                {
                    pPos = new Position(m_position.X, i);
                    middlePieces.Add(pPos);
                }
            }
            // If the new position is on the LEFT.
            else if (newPos.X < m_position.X)
            {
                for (i = newPos.X + 1; i < m_position.X; ++i)
                {
                    pPos = new Position(i, m_position.Y);
                    middlePieces.Add(pPos);
                }
            }

            // Check to see how many middle pieces exist from the 
            // new to the current position.
            int numMiddle = 0;
            foreach (Position pos in middlePieces)
            {
                if (m_referee.HasPieceAt(pos))
                    ++numMiddle;
            }

            // If there are more than 1 middle piece, return 'invalid'.
            if (numMiddle > 1)
            {
                return false;
            }
            // If there is exactly 1 middle pieces, this must be a Capture Move.
            else if (numMiddle == 1)
            {
                Piece capturedPiece = m_referee.GetPieceAt(newPos);
                if (capturedPiece == null || capturedPiece.HasColor(m_color))
                {
                    return false;
                }
            }
            // If there is no middle piece, make sure that no piece is captured.
            else   // numMiddle = 0
            {
                if (m_referee.HasPieceAt(newPos))
                {
                    return false;
                }
            }

            return true;
        }

        public override List<Position> GetPotentialNextPositions()
        {
            List<Position> positions = new List<Position>();

            // ... Horizontally.
            for (int x = 0; x <= 8; ++x)
            {
                if (x == m_position.X) continue;
                positions.Add(new Position(x, m_position.Y));

            }

            // ... Vertically
            for (int y = 0; y <= 9; ++y)
            {
                if (y == m_position.Y) continue;
                positions.Add(new Position(m_position.X, y));

            }
            return positions;
        }

    }
}
