// Generic game piece, template for how to interact with the board
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
    public abstract class Piece
    {
        protected PieceType m_type;
        protected PieceColor m_color;
        protected Position m_position;
        protected ChessReferee m_referee;

        public Piece(PieceType type, PieceColor color, Position position)
        {
            m_type = type;
            m_color = color;
            m_position = position;
        }

        public static bool IsValidPiece(Piece piece)
        {
            return piece != null && piece.Position.IsValid();
        }

        public PieceType Type
        {
            get { return m_type; }
        }

        public PieceColor Color
        {
            get { return m_color; }
        }

        public Position Position
        {
            get { return m_position; }
            set { m_position.Set(value); }
        }

        public bool HasColor(PieceColor color)
        {
            return m_color == color;
        }
        public bool HasType(PieceType t) { return m_type == t; }
        public bool HasSameColumnAs(Piece other)
        {
            return (m_position.X == other.Position.X);
        }

        public abstract bool CanMoveTo(Position newPos);
        public abstract List<Position> GetPotentialNextPositions();
        public abstract Piece Clone();

        //public void setPosition(Position pos) { position.set(pos); }
        public void SetReferee(ChessReferee board)
        {
            m_referee = board;
        }


        public bool IsValidMove(Position newPos)
        {
            if (!CanMoveTo(newPos))
                return false;

            /* If this is an Capture-Move, make sure the captured piece 
             * is an 'enemy'. 
             */
            Piece captured = m_referee.GetPieceAt(newPos);
            if (captured != null && captured.HasColor(m_color))
            {
                return false; // Capture your OWN piece! Not legal.
            }

            return true;
        }

        public bool CanMoveNext()
        {

            /* Generate all potential 'next' positions. */

            List<Position> positions = GetPotentialNextPositions();  // all potential 'next' positions.

            /* For each potential 'next' position, check if this piece can 
             * actually move there. 
             */

            ChessMove move = new ChessMove(this);


            foreach (Position position in positions)
            {
                if (!position.IsValid())
                    continue;

                move.To = position;

                /* Ask the Board to validate this Move in Simulation mode. */
                if (m_referee.Simulation_IsValidMove(move))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
