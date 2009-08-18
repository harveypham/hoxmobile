// A game move which contains a piece, its current position, and its
// destination.
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
    public class ChessMove
    {
        Piece m_piece;            // The Piece that moves.
        Position m_to;         // position to move to.
        Position m_from;      // position to move from

        Piece m_captured;

        public ChessMove(Piece piece)
        {
            m_piece = piece;
            m_from = new Position(piece.Position);
        }

        public Piece Piece
        {
            get { return m_piece; }
        }

        public Position To
        {
            get { return m_to; }
            set { m_to = value; }
        }

        public Position From
        {
            get { return m_from; }
        }

        public Piece Captured
        {
            get { return m_captured; }
            set { m_captured = value; }
        }

    }

}
