// Main form of the application
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace HoxMobile
{

    public partial class HoxChessForm : Form, IOpponentCallback
    {
        public HoxChessForm()
        {
            InitializeComponent();
            //mBoardSize = new BoardSize(false);
            linePen = new Pen(mSettings.mForeground, mSettings.mLineWeight);
            mLastMove = new Position(Position.OffBoard);
            mReferee = new ChessReferee( PieceColor.Red, m_gameOptions.Mode == GameOptionsDialog.GameMode.Machine, m_gameOptions.Level);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (mGameOver || !mReferee.PlayerTurn())
                return;

            if (e.Button == MouseButtons.Left)
            {
                Graphics g = CreateGraphics();

                Position pos = mBoardSize.GetPosition(new Point(e.X, e.Y));
                if (!pos.IsValid())
                    return;

                Piece selected = mReferee.Selected;
                if (Piece.IsValidPiece(selected))
                {
                    Position oldPosition = new Position(selected.Position);
                    //Position lastMove = new Position(getReferee().GetLastMove());
                    MoveResult result = mReferee.Move(pos, this);
                    switch (result)
                    {
                        case MoveResult.Invalid:
                            break;

                        case MoveResult.ChangeSelect:
                            RedrawPiece(g, selected);
                            SelectPiece(g, pos);
                            break;

                        case MoveResult.Move:
                        case MoveResult.Winning:
                            ErasePiece(g, oldPosition);

                            if (mLastMove.IsValid() && pos != mLastMove)
                            { // We don't move to the last move position, 
                                //  so we need to restore it
                                Piece lastPiece = mReferee.GetPieceAt(mLastMove);
                                if (lastPiece != null)
                                    RedrawPiece(g, lastPiece);
                            }
                            mLastMove.Set(pos);

                            // Draw the current piece to the next piece.
                            DrawPiece(g, selected);
                            HighlightLastMove(g);
                            if (result == MoveResult.Winning)
                                SetGameOver();
                            break;
                    }
                }
                else
                {
                    if (mReferee.Select(pos))
                    {
                        SelectPiece(g, pos);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //mBoardSize.Resize(Size);

            Graphics g = e.Graphics;
            DrawBoard(g);
            DrawPieces(g);
            if (mGameOver)
                DrawGameOver( g );
        }

        #region Helpers to draw the board
        private void DrawBoard( Graphics g )
        {
            OnDrawBackground(g);

            using (Pen borderPen = new Pen(mSettings.mForeground, mSettings.mBorderWeight))
            using (SolidBrush labelBrush = new SolidBrush( mSettings.mForeground))
            {
                // Draw vertical lines
                g.DrawLine(linePen, mBoardSize.Left, mBoardSize.Top, mBoardSize.Left, mBoardSize.Bottom);
                for (int vLine = 1; vLine <= Position.MaxX-1; vLine++)
                {
                    int x1 = mBoardSize.Left + vLine * mBoardSize.CellSize;
                    int y1 = mBoardSize.Top + 4 * mBoardSize.CellSize;
                    int y2 = mBoardSize.Top + 5 * mBoardSize.CellSize;
                    g.DrawLine(linePen, x1, mBoardSize.Top, x1, y1);
                    g.DrawLine(linePen, x1, y2, x1, mBoardSize.Bottom);
                }
                g.DrawLine(linePen, mBoardSize.Right, mBoardSize.Top, mBoardSize.Right, mBoardSize.Bottom);

                // Draw horizontal line
                for (int hLine = 0; hLine <= Position.MaxY; hLine++)
                {
                    int y = mBoardSize.Top + hLine * mBoardSize.CellSize;
                    g.DrawLine(linePen, mBoardSize.Left, y, mBoardSize.Right, y);
                }

                DrawPalace(g);

                using (Pen seatPen = new Pen(mSettings.mSeatColor))
                {
                    int lBound = mSeatLocations.GetLowerBound(0);
                    int uBound = mSeatLocations.GetUpperBound(0);
                    for (int i = lBound; i <= uBound; i++)
                    {
                        DrawSeat(g, mSeatLocations[i, 0], mSeatLocations[i, 1]);
                    }
                }
            }

        }

        private void DrawPieces(Graphics g)
        {
            List<Piece> activePieces = mReferee.ActivePieces;
            foreach (Piece piece in activePieces)
            {
                DrawPiece(g, piece);
            }
            HighlightLastMove(g);
            Piece selected = mReferee.Selected;
            if (selected != null)
                SelectPiece(g, selected.Position);
        }

        private void OnDrawBackground(Graphics g)
        {
            g.Clear(BackColor);
        }

        private void DrawPalace(Graphics g)
        {
            // Drawing palaces
            int palaceX1 = mBoardSize.Left + 3 * mBoardSize.CellSize;
            int palaceX2 = mBoardSize.Left + 5 * mBoardSize.CellSize;
            int palaceY2 = mBoardSize.Top + 2 * mBoardSize.CellSize;
            int palaceY3 = mBoardSize.Top + 7 * mBoardSize.CellSize;
            g.DrawLine(linePen, palaceX1, mBoardSize.Top,
                palaceX2, palaceY2);
            g.DrawLine(linePen, palaceX1, palaceY2,
                palaceX2, mBoardSize.Top);
            g.DrawLine(linePen, palaceX1, palaceY3,
                palaceX2, mBoardSize.Bottom);
            g.DrawLine(linePen, palaceX1, mBoardSize.Bottom,
                palaceX2, palaceY3);
        }

        void DrawSeat(Graphics g, int xPos, int yPos)
        {
            using (Pen pen = new Pen(mSettings.mSeatColor))
            {
                int x = mBoardSize.X + xPos * mBoardSize.CellSize;
                int y = mBoardSize.Y + yPos * mBoardSize.CellSize;
                int seatSize = mBoardSize.CellSize / mSettings.mSeatSizeScale;
                if (xPos != 0)
                {
                    Point[] upper = new Point[3]
                    {
                        new Point(x - mSettings.mSeatSpace, y - mSettings.mSeatSpace - seatSize ),
                        new Point(x - mSettings.mSeatSpace, y - mSettings.mSeatSpace ),
                        new Point(x - mSettings.mSeatSpace - seatSize, y - mSettings.mSeatSpace)
                    };
                    g.DrawLines(pen, upper);

                    Point[] lower = new Point[3]
                    {
                        new Point(x - mSettings.mSeatSpace, y + mSettings.mSeatSpace + seatSize ),
                        new Point(x - mSettings.mSeatSpace, y + mSettings.mSeatSpace ),
                        new Point(x - mSettings.mSeatSpace - seatSize, y + mSettings.mSeatSpace)
                    };
                    g.DrawLines(pen, lower);
                }

                if (xPos < Position.MaxX)
                {
                    Point[] upper = new Point[3]
                    {
                        new Point(x + mSettings.mSeatSpace, y - mSettings.mSeatSpace - seatSize ),
                        new Point(x + mSettings.mSeatSpace, y - mSettings.mSeatSpace ),
                        new Point(x + mSettings.mSeatSpace + seatSize, y - mSettings.mSeatSpace)
                    };
                    g.DrawLines(pen, upper);

                    Point[] lower = new Point[3]
                    {
                        new Point(x + mSettings.mSeatSpace, y + mSettings.mSeatSpace + seatSize ),
                        new Point(x + mSettings.mSeatSpace, y + mSettings.mSeatSpace ),
                        new Point(x + mSettings.mSeatSpace + seatSize, y + mSettings.mSeatSpace)
                    };
                    g.DrawLines(pen, lower);
                }
            }
        }

        private void DrawPiece(Graphics g, Piece piece)
        {
            Position pos = new Position(piece.Position);
            //pos.Transform(mInverted);

            Bitmap im = mImages.GetImage(piece.Type, piece.Color);
            ImageAttributes imageAttrib = new ImageAttributes();
            Color transparentColor = im.GetPixel(0, 0);
            imageAttrib.SetColorKey(transparentColor, transparentColor);

            Rectangle rect = mBoardSize.GetRectangle(pos);

            g.DrawImage(im, rect, 0, 0, im.Width, im.Height, GraphicsUnit.Pixel, imageAttrib);
        }

        private void ErasePiece(Graphics g, Position pos)
        {
            Rectangle rect = mBoardSize.GetRectangle(pos);
            //rect.Width += 1 + mSettings.mSelectWeigth;
            //rect.Height += 1 + mSettings.mSelectWeigth;
            //if ( pos.getX() < Position.MaxX )
                rect.Width += mSettings.mSelectWeigth;
            //if ( pos.getY() < Position.MaxY )
                rect.Height += mSettings.mSelectWeigth;
            g.Clip = new Region(rect);
            DrawBoard(g);
            g.ResetClip();
        }

        private void RedrawPiece(Graphics g, Piece piece)
        {
            ErasePiece(g, piece.Position);
            DrawPiece(g, piece);
        }

        private void SelectPiece(Graphics g, Position pos)
        {
            if (!pos.IsValid())
                return;

            using (Pen selectPen = new Pen(mSettings.mSelectColor, mSettings.mSelectWeigth))
            {
                Highlight(g, pos, selectPen);
            }
        }

        private void HighlightLastMove(Graphics g)
        {
            if (!mLastMove.IsValid())
                return;

            using (Pen highlight = new Pen(mSettings.mHighlightColor, mSettings.mHighlightWeight))
            {
                Highlight(g, mLastMove, highlight);
            }
        }

        private void Highlight( Graphics g, Position pos, Pen pen )
        {
            if (!pos.IsValid())
                return;

            Rectangle rect = mBoardSize.GetRectangle(pos);
            g.DrawRectangle(pen, rect);
        }

        private void DrawGameOver(Graphics g)
        {
            Font f = new Font( FontFamily.GenericSansSerif, 24, FontStyle.Italic);
            SolidBrush brush = new SolidBrush(Color.Red);
            Rectangle rect = new Rectangle(mBoardSize.X, mBoardSize.Y + 4 * mBoardSize.CellSize,
                mBoardSize.Width, mBoardSize.CellSize);
            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            g.DrawString("Game Over", f, brush, rect, sf);
        }

        private void SetGameOver()
        {
            mGameOver = true;
            Graphics g = CreateGraphics();
            DrawGameOver(g);
        }

        #endregion

        #region IAiCallback Members

        delegate void OpponentMoveDelegate(ChessMove move, MoveResult result);
        public void OnOpponentMove( ChessMove move, MoveResult result )
        {
            if (InvokeRequired)
            {
                OpponentMoveDelegate opponentMoveDelegate = new OpponentMoveDelegate(OnOpponentMove);
                BeginInvoke(opponentMoveDelegate, new object[] { move, result });
            }
            else
            {
                Position from = move.From;
                Position to = move.To;
                Graphics g = CreateGraphics();
                ErasePiece(g, from);
                RedrawPiece(g, mReferee.GetPieceAt(to));
                if (mLastMove.IsValid() && !mLastMove.Equals(to))
                {
                    Piece lastMovePiece = mReferee.GetPieceAt(mLastMove);
                    RedrawPiece(g, lastMovePiece);
                }
                mLastMove.Set(to);
                HighlightLastMove(g);
                if (result == MoveResult.Winning)
                    SetGameOver();
                mReferee.AcknowledgeOpponentMove();
            }
        }

        #endregion

        private void miStart_Click(object sender, EventArgs e)
        {
            // Start of the game, there is no need to reset
            //if (!mLastMove.IsValid())
            //    return;

            if (mGameOver || mReferee.PlayerTurn())
            {
                mGameOver = false;
                mLastMove.Set(Position.OffBoard);
                mReferee.StartNewGame(PieceColor.Red, m_gameOptions.Mode == GameOptionsDialog.GameMode.Machine, m_gameOptions.Level);
                Invalidate();
                Update();
            }
        }

        private void miQuit_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private GameOptionsDialog.GameOptions m_gameOptions = new GameOptionsDialog.GameOptions();

        private void miGameOptions_Click(object sender, EventArgs e)
        {
            GameOptionsDialog dialog = new GameOptionsDialog();
            dialog.Options.Set(m_gameOptions);
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                //if ( dialog.Options.Mode == GameOptionsDialog.GameMode.Machine && 
                m_gameOptions.Set(dialog.Options);
            }
        }
    }

}