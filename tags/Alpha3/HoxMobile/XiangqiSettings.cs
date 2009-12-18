// Trivial settings on how the Xiangqi board is drawn
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

    public class XiangqiSettings
    {
        //public Color mBackground;
        public Color mForeground;

        public int mBorderX;
        public int mBorderY;
        public int mBorderWeight;

        public int mLineWeight;

        public Color mSeatColor;
        public int mSeatSpace;
        public int mSeatSizeScale;

        public Color mHighlightColor;
        public int mHighlightWeight;

        public Color mSelectColor;
        public int mSelectWeigth;

        public XiangqiSettings()
        {
            //mBackground = Color.FromArgb( 255, 64, 64, 64 );
            //mForegournd = Color.FromArgb( 0, 255, 255, 255 );
            mForeground = Color.Black;

            mBorderX = 4;
            mBorderY = 4;
            mBorderWeight = 2;

            mLineWeight = 0;

            mSeatColor = Color.Black;
            mSeatSpace = 3;
            mSeatSizeScale = 7;

            mHighlightColor = Color.Green;
            mHighlightWeight = 1;

            mSelectColor = Color.Blue;
            mSelectWeigth = 1;

        }

    }

}
