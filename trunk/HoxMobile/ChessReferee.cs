// Board game data and game rules
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
using System.Threading;

namespace HoxMobile
{
    public interface IOpponentCallback
    {
        void OnOpponentMove( ChessMove move, MoveResult result );
    }

    public class ChessReferee
    {
        private bool m_playerVsMachine;

        // Game state for both side
        private bool m_humanTurn;                       // Waiting for human input?
        PieceColor m_color;                             // Which color is playing
        Piece[,] m_board = new Piece[Position.MaxX + 1, Position.MaxY + 1]; // The board
        List<Piece> m_pieces = new List<Piece>();       // Active pieces (on the board)
        List<Piece> m_deadPieces = new List<Piece>();   // Captured pieces

        // Tracking interactive user data
        private Piece m_select = null;
        private Position m_lastMove = Position.OffBoard;
        private Piece m_lastCaptured = null;

        // AI
        HoxMobile.AI.XQWLight m_xqwLight;

        public ChessReferee(PieceColor playerColor, bool playerVsMachine, int level )
        {
            m_color = playerColor;
            m_playerVsMachine = playerVsMachine;
            if (m_color == PieceColor.Red)
            {
                m_humanTurn = true;
            }

            if (m_playerVsMachine)
            {
                m_xqwLight = new HoxMobile.AI.XQWLight();
                m_xqwLight.init_engine(level);
                m_xqwLight.set_search_time(10);

                m_xqwLight.init_game(null, 'w');
            }

            ClearBoard();
            SetupPiecesForNewGame();
        }

        private void ClearBoard()
        {
            for (int i = 0; i <= Position.MaxX; i++)
            {
                for (int j = 0; j <= Position.MaxY; j++)
                    m_board[i, j] = null;
            }
            // Is it more cost effective just reconstruct the 2 dim array?
            //m_board = new Piece[Position.MaxX + 1, Position.MaxY + 1];
            m_pieces.Clear();
            m_deadPieces.Clear();
            m_select = null;
            m_lastMove = Position.OffBoard;
            m_lastCaptured = null;
        }

        public void SetupPiecesForNewGame()
        {
            //init pieces
            // init black pieces
            PieceColor color = PieceColor.Black;
            Piece piece;
            piece = new KingPiece(color, new Position(4, 0));
            AddPiece(piece);

            piece = new AdvisorPiece(color, new Position(3, 0));
            AddPiece(piece);

            piece = new AdvisorPiece(color, new Position(5, 0));
            AddPiece(piece);

            piece = new ElephantPiece(color, new Position(2, 0));
            AddPiece(piece);

            piece = new ElephantPiece(color, new Position(6, 0));
            AddPiece(piece);

            piece = new HorsePiece(color, new Position(1, 0));
            AddPiece(piece);

            piece = new HorsePiece(color, new Position(7, 0));
            AddPiece(piece);

            piece = new ChariotPiece(color, new Position(0, 0));
            AddPiece(piece);

            piece = new ChariotPiece(color, new Position(8, 0));
            AddPiece(piece);

            piece = new CannonPiece(color, new Position(1, 2));
            AddPiece(piece);

            piece = new CannonPiece(color, new Position(7, 2));
            AddPiece(piece);

            for (short i = 0; i < 10; i += 2) // 5 Pawns.
            {
                piece = new PawnPiece(color, new Position(i, 3));
                AddPiece(piece);
            }

            // init red pieces
            color = PieceColor.Red;
            piece = new KingPiece(color, new Position(4, 9));
            AddPiece(piece);

            piece = new AdvisorPiece(color, new Position(3, 9));
            AddPiece(piece);

            piece = new AdvisorPiece(color, new Position(5, 9));
            AddPiece(piece);

            piece = new ElephantPiece(color, new Position(2, 9));
            AddPiece(piece);

            piece = new ElephantPiece(color, new Position(6, 9));
            AddPiece(piece);

            piece = new HorsePiece(color, new Position(1, 9));
            AddPiece(piece);

            piece = new HorsePiece(color, new Position(7, 9));
            AddPiece(piece);

            piece = new ChariotPiece(color, new Position(0, 9));
            AddPiece(piece);

            piece = new ChariotPiece(color, new Position(8, 9));
            AddPiece(piece);

            piece = new CannonPiece(color, new Position(1, 7));
            AddPiece(piece);

            piece = new CannonPiece(color, new Position(7, 7));
            AddPiece(piece);

            for (short i = 0; i < 10; i += 2) // 5 Pawns.
            {
                piece = new PawnPiece(color, new Position(i, 6));
                AddPiece(piece);
            }
        }

        public void StartNewGame(PieceColor playerColor, bool playerVsMachine, int level)
        {
            m_color = playerColor;
            m_playerVsMachine = playerVsMachine;
            if (m_color == PieceColor.Red)
            {
                m_humanTurn = true;
            }

            if (m_playerVsMachine)
            {
                if (m_xqwLight != null)
                    m_xqwLight = new HoxMobile.AI.XQWLight();
                //m_xqwLight.init_engine(3);
                //m_xqwLight.set_search_time(10);
                m_xqwLight.init_engine(level);
                m_xqwLight.set_search_time(10);
                m_xqwLight.init_game(null, 'w');
            }
            else
            {
                m_xqwLight = null;
            }

            ClearBoard();
            SetupPiecesForNewGame();
        }

        public List<Piece> ActivePieces
        {
            get { return m_pieces; }
        }

        public bool PlayerTurn()
        {
            return (m_playerVsMachine == false || m_humanTurn == true);
        }

        public Piece Selected
        {
            get { return m_select; }
        }

        public bool Select(Position pos)
        {
            Piece piece = GetPieceAt(pos);
            if (piece != null && piece.HasColor(m_color))
            {
                m_select = piece;
                return true;
            }
            return false;
        }

        public MoveResult Move(Position pos, IOpponentCallback callback)
        {
            if (m_select == null)
                return MoveResult.Invalid;

            Piece piece = GetPieceAt(pos);
            if (piece != null && piece.HasColor(m_color))
            {
                m_select = piece;
                return MoveResult.ChangeSelect;
            }

            ChessMove moveAction = new ChessMove(m_select);
            moveAction.To = pos;
            MoveResult result = ValidateMove(moveAction);
            if (result == MoveResult.Invalid)
            {
                return result;
            }

            m_select = null;
            m_lastMove = pos;


            m_lastCaptured = moveAction.Captured;
            SwitchUser();
            if (result == MoveResult.Winning)
            {   // No need to start next player
                return result;
            }

            if (m_playerVsMachine)
            {
                string move = string.Format("{0}{1}{2}{3}",
                    moveAction.From.X,
                    moveAction.From.Y,
                    moveAction.To.X,
                    moveAction.To.Y);

                if (m_xqwLight != null)
                    m_xqwLight.on_human_move(move);

                bool background = System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(ExecuteXqwLight), callback);
            }

            return result;
        }

        public void AcknowledgeOpponentMove()
        {
            SwitchUser();
        }

        private void ExecuteXqwLight( object callback )
        {
            string s = m_xqwLight.generate_move();
            if (s == null || s.Length < 4)
            {
                return;
            }

            Position from = new Position( ToCoordinate( s[ 0 ] ), ToCoordinate( s[ 1 ] ));
            Position to = new Position(ToCoordinate(s[2]), ToCoordinate(s[3]));

            Piece selected = GetPieceAt(from);
            if (selected == null)
                return;

            ChessMove moveAction = new ChessMove( selected );
            moveAction.To = to;
            MoveResult moveResult = ValidateMove(moveAction);
            if (moveResult == MoveResult.Move || moveResult == MoveResult.Winning)
            {
                IOpponentCallback oppopnentCallback = (IOpponentCallback)callback;
                oppopnentCallback.OnOpponentMove(moveAction, moveResult);
            }
        }

        private int ToCoordinate(char c)
        {
            return Convert.ToInt32(c - '0');
        }

        private void SwitchUser()
        {
            m_color = (m_color == PieceColor.Red ? PieceColor.Black : PieceColor.Red);
            if (m_playerVsMachine)
            {
                m_humanTurn = !m_humanTurn;
            }
        }

        private void AddPiece(Piece piece)
        {
            piece.SetReferee(this);
            m_pieces.Add(piece);
            SetPiece(piece);
        }

        private void SetPiece(Piece piece)
        {
            Position pos = piece.Position;
            m_board[pos.X,pos.Y] = piece;
        }

        // Check if a King (of a given color) is in CHECK position (being "checked").
        // @return true if the King is being checked.
        //         false, otherwise.
        private bool IsKingBeingChecked(PieceColor color)
        {
            /* Check if this move results in one's own-checkmate.
             * This is done as follows:
             *  + For each piece of the 'enemy', check if the king's position
             *    is one of its valid move.
             */

            Piece king = GetKing(color);
            foreach (Piece piece in m_pieces)
            {
                if (!piece.HasColor(color)  // enemy?
                     && piece.IsValidMove(king.Position))
                {
                    return true;
                }
            }

            return false;  // Not in "checked" position.
        }

        // Get the King of a given color.
        private Piece GetKing(PieceColor color)
        {
            foreach (Piece piece in m_pieces)
            {
                if (piece.Type == PieceType.King && piece.HasColor(color))
                {
                    return piece;
                }
            }
            return null;
        }

        /**
 * Carry out the 'capture' action toward a given piece:
 *   + Unset the piece from the Board.
 *   + Move the piece from the ACTIVE list to the INACTIVE list.
 */
        private void CapturePiece(Piece piece)
        {
            UnsetPiece(piece);
            m_pieces.Remove(piece);
            m_deadPieces.Add(piece);
        }

        /**
 * Move an existing piece from its current position to a new position.
 * @side-affects: The piece's position is modified as well.
 */
        public void MovePieceTo(Piece piece, Position newPos)
        {

            Position curPos = piece.Position;  // The current position.

            UnsetPiece(piece);
            piece.Position = newPos;  // Adjust piece's position.
            SetPiece(piece);
        }

        /**
     * Record a valid Move.
     */
        public Piece RecordMove(ChessMove move)
        {
            // Remove captured piece, if any.
            Piece captured = GetPieceAt(move.To);
            if (captured != null)
            {
                CapturePiece(captured);
            }

            // Move the piece to the new position.
            Piece piece = GetPieceAt(move.Piece.Position);
            MovePieceTo(piece, move.To);

            return captured;
        }

        private void PutbackPiece(Piece piece)
        {
            if (m_deadPieces.Contains(piece))
            {
                m_deadPieces.Remove(piece);
                AddPiece(piece);
            }
        }

        private void UndoMove(ChessMove move, Piece captured)
        {
            // Return the piece from its NEW position to its ORIGINAL position.
            Piece piece = GetPieceAt(move.To);
            MovePieceTo(piece, move.From);

            // "Un-capture" the captured piece, if any.
            if (captured != null)
            {
                captured.Position = move.To;
                PutbackPiece(captured);
            }
        }

        // Check if one king is facing another.
        private bool IsKingFaceKing()
        {
            Piece blackKing = GetKing(PieceColor.Black);
            Piece redKing = GetKing(PieceColor.Red);


            if (!blackKing.HasSameColumnAs(redKing)) // not the same column.
                return false;  // Not facing

            // If they are in the same column, check if there is any piece in between.
            foreach (Piece piece in m_pieces)
            {
                if (piece.Equals(blackKing) || piece.Equals(redKing))
                    continue;

                if (piece.HasSameColumnAs(redKing))  // in between Kings?
                    return false;  // Not facing
            }
            return true;  // Facing
        }

        public MoveResult ValidateMove(ChessMove move)
        {
            /* Check for 'turn' */

            if (!move.Piece.HasColor(m_color))
            {
                return MoveResult.Invalid; // Error! Wrong turn.
            }

            /* Perform a basic validation */

            Piece piece = GetPieceAt(move.Piece.Position);
            if (piece == null)
            {
                return MoveResult.Invalid;
            }

            if (!piece.IsValidMove(move.To))
                return MoveResult.Invalid;

            /* At this point, the Move is valid.
             * Record this move (to validate future Moves).
             */

            Piece captured = RecordMove(move);

            /* If the Move results in its own check-mate OR
             * there is a KING-face-KING problem...
             * then it is invalid and must be undone.
             */
            if (IsKingBeingChecked(move.Piece.Color) || IsKingFaceKing())
            {
                UndoMove(move, captured);
                return MoveResult.Invalid;
            }

            /* Return the captured-piece, if any */
            move.Captured = captured;

            /* Check for end game:
             * ------------------
             *   Checking if this Move makes the Move's Player
             *   the winner of the game. The step is done by checking to see if the
             *   opponent can make ANY valid Move at all.
             *   If not, then the opponent has just lost the game.
             */

            if (!DoesNextMoveExist())
            {
                //return (m_color == Color.BLACK
                //          ? Status.RED_WIN
                //          : Status.BLACK_WIN);
                return MoveResult.Winning;
            }
            else
            {
                return MoveResult.Move;
            }
        }

        private bool DoesNextMoveExist()
        {
            /* Go through all Pieces of the 'next' color.
             * If any piece can move 'next', then Board can as well.
             */
            PieceColor otherColor = GetOtherColor();

            //Iterate through a copy of live pieces
            //This is needed as within each iteration, the pieces list will be modified.
            List<Piece> copy_m_pieces = new List<Piece>();
            foreach (Piece piece in m_pieces)
                copy_m_pieces.Add(piece);

            foreach (Piece piece in copy_m_pieces)
            {
                if (piece.HasColor(otherColor) && piece.CanMoveNext())
                {
                    return true;
                }
            }

            return false;
        }

        public bool Simulation_IsValidMove(ChessMove move)
        {
            Piece piece = GetPieceAt(move.Piece.Position);
            if (piece == null)
                return false;

            if (!piece.IsValidMove(move.To))
                return false;

            /* Simulate the Move. */

            Piece captured = RecordMove(move);

            /* If the Move ends up results in its own check-mate,
             * then it is invalid and must be undone.
             */
            bool beingChecked = IsKingBeingChecked(piece.Color);
            bool areKingsFacing = IsKingFaceKing();

            UndoMove(move, captured);

            if (beingChecked || areKingsFacing)
            {
                return false;  // Not a good move. Process the next move.
            }

            return true;  // It is a valid Move.
        }

        private void UnsetPiece(Piece piece)
        {
            Position pos = piece.Position;
            m_board[pos.X,pos.Y] = null;
            //m_cells[pos.getX(),pos.getY()].setPosition(new Position());
        }

        public bool HasPieceAt(Position pos)
        {
            return (null != GetPieceAt(pos));
        }

        public Piece GetPieceAt(Position pos)
        {
            if (!pos.IsValid())
                return null;

            return m_board[pos.X,pos.Y];
        }

        private PieceColor GetOtherColor()
        {
            if (m_color == PieceColor.Red)
                return PieceColor.Black;
            if (m_color == PieceColor.Black)
                return PieceColor.Red;
            return PieceColor.None;
        }

    }

}
