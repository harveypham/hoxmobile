// Getting resource image by piece name and color
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

    class PieceImage
    {

        public PieceImage()
        {
        }

        public Bitmap GetImage(PieceType type, PieceColor role)
        {
            if (role == PieceColor.Red)
                return GetRedImage(type);
            else
                return GetBlackImage(type);
        }

        private Bitmap GetBlackImage(PieceType type)
        {
            switch (type)
            {
                case PieceType.King:
                    return Properties.Resources.bking;

                case PieceType.Advisor:
                    return Properties.Resources.badvisor;

                case PieceType.Elephant:
                    return Properties.Resources.belephant;

                case PieceType.Chariot:
                    return Properties.Resources.bchariot;

                case PieceType.Cannon:
                    return Properties.Resources.bcannon;

                case PieceType.Horse:
                    return Properties.Resources.bhorse;

                case PieceType.Pawn:
                    return Properties.Resources.bpawn;
            }
            return Properties.Resources.bking;
        }

        private Bitmap GetRedImage(PieceType type)
        {
            switch (type)
            {
                case PieceType.King:
                    return Properties.Resources.rking;

                case PieceType.Advisor:
                    return Properties.Resources.radvisor;

                case PieceType.Elephant:
                    return Properties.Resources.relephant;

                case PieceType.Chariot:
                    return Properties.Resources.rchariot;

                case PieceType.Cannon:
                    return Properties.Resources.rcannon;

                case PieceType.Horse:
                    return Properties.Resources.rhorse;

                case PieceType.Pawn:
                    return Properties.Resources.rpawn;
            }
            return Properties.Resources.rking;
        }

    }

}
