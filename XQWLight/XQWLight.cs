// Properties and Windows Form Designer generated code
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

// NOTE: This AI is ported from C/C++. The original source code was obtained
// as part of HOXChess source code. HOXChess, in-turn, obtained the source
// from http://www.elephantbase.net/computer/stepbystep1.htm. The following
// are excepts from HOXChess source code:

// Start quote =============================================================
/***************************************************************************
 *  Copyright 2007-2009 Huy Phan  <huyphan@playxiangqi.com>                *
 *                      Bharatendra Boddu (bharathendra at yahoo dot com)  *
 *                                                                         * 
 *  This file is part of HOXChess.                                         *
 *                                                                         *
 *  HOXChess is free software: you can redistribute it and/or modify       *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  HOXChess is distributed in the hope that it will be useful,            *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with HOXChess.  If not, see <http://www.gnu.org/licenses/>.      *
 ***************************************************************************/

/////////////////////////////////////////////////////////////////////////////
// Name:            XQWLight.h
// Created:         10/11/2008
//
// Description:     This is 'XQWLight' Engine to interface with HOXChess.
//                  XQWLight is an open-source Xiangqi AI Engine
//                  written by Huang Chen at www.elephantbase.net
//
//  (Original Chinese URL)
//        http://www.elephantbase.net/computer/stepbystep1.htm
//
//  (Translated English URL using Goold Translate)
//       http://74.125.93.104/translate_c?hl=en&langpair=
//         zh-CN|en&u=http://www.elephantbase.net/computer/stepbystep1.htm&
//         usg=ALkJrhj7W0v3J1P-xmbufsWzYq7uKciL1w
/////////////////////////////////////////////////////////////////////////////
// End quote ===============================================================
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace HoxMobile.AI
{

    public class XQWLight
    {
        public XQWLight()
        {
        }

        static Random rand;

        static int m_searchDepth = 5;
        public void init_engine(int searchDepth)
        {
            m_searchDepth = searchDepth;
        }

        TimeSpan m_searchTime = new TimeSpan(0, 0, 1);
        public void set_search_time(int seconds)
        {
            m_searchTime = new TimeSpan(0, 0, seconds);
        }
        /* Only approximately... */

        public void init_game(sbyte[,] board, char side /* = 'w' */ )
        {
            rand = new Random();
           InitZobrist();
            //LoadBook();
            Startup(board);

            if (side == 'b')
            {
                pos.ChangeSide();
            }
        }

        private void InitZobrist()
        {
            int i, j;
            RC4Struct rc4 = new RC4Struct();

            rc4.InitZero();
            m_zobrist.Player.InitRC4(rc4);
            for (i = 0; i < 14; i++)
            {
                for (j = 0; j < 256; j++)
                {
                    m_zobrist.Table[i, j].InitRC4(rc4);
                }
            }
        }

        static void InitializeIntArray(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new int();
            }
        }

        static void ClearIntArray(int[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 0;
            }
        }

        private void LoadBook()
        {
            using (FileStream fileStream = File.Open("book.dat", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                m_search.nBookSize = Convert.ToInt32(reader.BaseStream.Length) / 8;
                for (uint book = 0; book < m_search.nBookSize; book++)
                {
                    m_search.BookTable[book].dwLock = reader.ReadUInt32();
                    m_search.BookTable[book].wmv = reader.ReadUInt16();
                    m_search.BookTable[book].wvl = reader.ReadUInt16();
                }
            }
        }

        private void Startup(sbyte[,] board)
        {
            pos.Startup(board);
        }

        public string generate_move()
        {
            SearchMain();
            string stdMove = _xqwlight2hox(m_search.mvResult);
            pos.MakeMove(m_search.mvResult);
            return stdMove;
        }

        public void on_human_move(string sMove)
        {
            string stdMove = sMove;
            int nMove = _hox2xqwlight(stdMove);
            m_search.mvResult = nMove;
            pos.MakeMove(m_search.mvResult);
        }

        void SearchMain()
        {
            int i, vl, nGenMoves;
            int[] mvs = new int[MAX_GEN_MOVES];
            InitializeIntArray(mvs);
            pos.nDistance = 0; // 初始步数

            DateTime startSearch = DateTime.Now;

            // 搜索开局库
            m_search.mvResult = SearchBook();
            if (m_search.mvResult != 0)
            {
                pos.MakeMove(m_search.mvResult);
                if (pos.RepStatus(3) == 0)
                {
                    pos.UndoMakeMove();
                    return;
                }
                pos.UndoMakeMove();
            }

            // 检查是否只有唯一走法
            vl = 0;
            nGenMoves = pos.GenerateMoves(mvs, false);
            for (i = 0; i < nGenMoves; i++)
            {
                if (pos.MakeMove(mvs[i]))
                {
                    pos.UndoMakeMove();
                    m_search.mvResult = mvs[i];
                    vl++;
                }
            }
            if (vl == 1)
            {
                return;
            }

            // 迭代加深过程
            for (i = 1; i <= m_searchDepth; i++)
            {
                vl = SearchRoot(i);
                // 搜索到杀棋，就终止搜索
                if (vl > WIN_VALUE || vl < -WIN_VALUE)
                {
                    break;
                }

                if (DateTime.Now.CompareTo(startSearch.Add(m_searchTime)) > 0)
                {
                    break;
                }
                //Console.WriteLine( "SearchMain: Search depth START = {0}.\n",  i+1);
            }
            //Console.WriteLine( "SearchMain: Search depth = *** {0}.\n", i);
        }

        int SearchBook()
        {
            int i, vl, nBookMoves, mv;
            int[] mvs = new int[MAX_GEN_MOVES];
            InitializeIntArray(mvs);
            int[] vls = new int[MAX_GEN_MOVES];
            InitializeIntArray(vls);
            bool bMirror;
            BookItem bkToSearch = new BookItem();
            int lpbk = -1;
            PositionStruct posMirror = new PositionStruct();
            // 搜索开局库的过程有以下几个步骤

            // 1. 如果没有开局库，则立即返回
            if (m_search.nBookSize == 0)
            {
                return 0;
            }

            // 2. 搜索当前局面
            bMirror = false;
            bkToSearch.dwLock = pos.zobr.dwLock1;
            lpbk = m_search.BinarySearch(bkToSearch);
            //lpbk = (BookItem *) bsearch(&bkToSearch, Search.BookTable, Search.nBookSize, sizeof(BookItem), CompareBook);
            // 3. 如果没有找到，那么搜索当前局面的镜像局面
            if (lpbk == -1)
            {
                bMirror = true;
                pos.Mirror(posMirror);
                bkToSearch.dwLock = posMirror.zobr.dwLock1;
                lpbk = m_search.BinarySearch(bkToSearch);
                //lpbk = (BookItem *) bsearch(&bkToSearch, Search.BookTable, Search.nBookSize, sizeof(BookItem), CompareBook);
            }
            // 4. 如果镜像局面也没找到，则立即返回
            if (lpbk == -1)
            {
                return 0;
            }

            // 5. 如果找到，则向前查第一个开局库项
            while (lpbk >= 0 && m_search.BookTable[lpbk].dwLock == bkToSearch.dwLock)
            {
                lpbk--;
            }
            lpbk++;
            // 6. 把走法和分值写入到"mvs"和"vls"数组中
            vl = nBookMoves = 0;
            while (lpbk < m_search.nBookSize && m_search.BookTable[lpbk].dwLock == bkToSearch.dwLock)
            {
                mv = (bMirror ? MIRROR_MOVE(m_search.BookTable[lpbk].wmv) : m_search.BookTable[lpbk].wmv);
                if (pos.LegalMove(mv))
                {
                    mvs[nBookMoves] = mv;
                    vls[nBookMoves] = m_search.BookTable[lpbk].wvl;
                    vl += vls[nBookMoves];
                    nBookMoves++;
                    if (nBookMoves == MAX_GEN_MOVES)
                    {
                        break; // 防止"BOOK.DAT"中含有异常数据
                    }
                }
                lpbk++;
            }
            if (vl == 0)
            {
                return 0; // 防止"BOOK.DAT"中含有异常数据
            }
            // 7. 根据权重随机选择一个走法
            vl = rand.Next() % vl;
            for (i = 0; i < nBookMoves; i++)
            {
                vl -= vls[i];
                if (vl < 0)
                {
                    break;
                }
            }
            return mvs[i];
        }

        int SearchRoot(int nDepth)
        {
            int vl, vlBest, mv, nNewDepth;
            SortStruct Sort = new SortStruct();

            vlBest = -MATE_VALUE;
            Sort.Init(m_search.mvResult);
            while ((mv = Sort.Next()) != 0)
            {
                if (pos.MakeMove(mv))
                {
                    nNewDepth = pos.InCheck() ? nDepth : nDepth - 1;
                    if (vlBest == -MATE_VALUE)
                    {
                        vl = -SearchFull(-MATE_VALUE, MATE_VALUE, nNewDepth, true);
                    }
                    else
                    {
                        vl = -SearchFull(-vlBest - 1, -vlBest, nNewDepth, false);
                        if (vl > vlBest)
                        {
                            vl = -SearchFull(-MATE_VALUE, -vlBest, nNewDepth, true);
                        }
                    }
                    pos.UndoMakeMove();
                    if (vl > vlBest)
                    {
                        vlBest = vl;
                        m_search.mvResult = mv;
                        if (vlBest > -WIN_VALUE && vlBest < WIN_VALUE)
                        {
                            vlBest += (rand.Next() & RANDOM_MASK) - (rand.Next() & RANDOM_MASK);
                        }
                    }
                }
            }
            RecordHash(HASH_PV, vlBest, nDepth, m_search.mvResult);
            SetBestMove(m_search.mvResult, nDepth);
            return vlBest;
        }

        int SearchFull(int vlAlpha, int vlBeta, int nDepth, bool bNoNull /* false */ )
        {
            int nHashFlag, vl, vlBest;
            int mv;
            int mvBest;
            int mvHash = 0;
            int nNewDepth;
            SortStruct Sort = new SortStruct();
            // 一个Alpha-Beta完全搜索分为以下几个阶段

            // 1. 到达水平线，则调用静态搜索(注意：由于空步裁剪，深度可能小于零)
            if (nDepth <= 0)
            {
                return SearchQuiesc(vlAlpha, vlBeta);
            }

            // 1-1. 检查重复局面(注意：不要在根节点检查，否则就没有走法了)
            vl = pos.RepStatus(1);
            if (vl != 0)
            {
                return pos.RepValue(vl);
            }

            // 1-2. 到达极限深度就返回局面评价
            if (pos.nDistance == LIMIT_DEPTH)
            {
                return pos.Evaluate();
            }

            // 1-3. 尝试置换表裁剪，并得到置换表走法
            vl = ProbeHash(vlAlpha, vlBeta, nDepth, ref mvHash);
            if (vl > -MATE_VALUE)
            {
                return vl;
            }

            // 1-4. 尝试空步裁剪(根节点的Beta值是"MATE_VALUE"，所以不可能发生空步裁剪)
            if (!bNoNull && !pos.InCheck() && pos.NullOkay())
            {
                pos.NullMove();
                vl = -SearchFull(-vlBeta, 1 - vlBeta, nDepth - NULL_DEPTH - 1, true);
                pos.UndoNullMove();
                if (vl >= vlBeta)
                {
                    return vl;
                }
            }

            // 2. 初始化最佳值和最佳走法
            nHashFlag = HASH_ALPHA;
            vlBest = -MATE_VALUE; // 这样可以知道，是否一个走法都没走过(杀棋)
            mvBest = 0;           // 这样可以知道，是否搜索到了Beta走法或PV走法，以便保存到历史表

            // 3. 初始化走法排序结构
            Sort.Init(mvHash);

            // 4. 逐一走这些走法，并进行递归
            while ((mv = Sort.Next()) != 0)
            {
                if (pos.MakeMove(mv))
                {
                    // 将军延伸
                    nNewDepth = pos.InCheck() ? nDepth : nDepth - 1;
                    // PVS
                    if (vlBest == -MATE_VALUE)
                    {
                        vl = -SearchFull(-vlBeta, -vlAlpha, nNewDepth, false);
                    }
                    else
                    {
                        vl = -SearchFull(-vlAlpha - 1, -vlAlpha, nNewDepth, false);
                        if (vl > vlAlpha && vl < vlBeta)
                        {
                            vl = -SearchFull(-vlBeta, -vlAlpha, nNewDepth, false);
                        }
                    }
                    pos.UndoMakeMove();

                    // 5. 进行Alpha-Beta大小判断和截断
                    if (vl > vlBest)
                    {    // 找到最佳值(但不能确定是Alpha、PV还是Beta走法)
                        vlBest = vl;        // "vlBest"就是目前要返回的最佳值，可能超出Alpha-Beta边界
                        if (vl >= vlBeta)
                        { // 找到一个Beta走法
                            nHashFlag = HASH_BETA;
                            mvBest = mv;      // Beta走法要保存到历史表
                            break;            // Beta截断
                        }
                        if (vl > vlAlpha)
                        { // 找到一个PV走法
                            nHashFlag = HASH_PV;
                            mvBest = mv;      // PV走法要保存到历史表
                            vlAlpha = vl;     // 缩小Alpha-Beta边界
                        }
                    }
                }
            }

            // 5. 所有走法都搜索完了，把最佳走法(不能是Alpha走法)保存到历史表，返回最佳值
            if (vlBest == -MATE_VALUE)
            {
                // 如果是杀棋，就根据杀棋步数给出评价
                return pos.nDistance - MATE_VALUE;
            }
            // 记录到置换表
            RecordHash(nHashFlag, vlBest, nDepth, mvBest);
            if (mvBest != 0)
            {
                // 如果不是Alpha走法，就将最佳走法保存到历史表
                SetBestMove(mvBest, nDepth);
            }
            return vlBest;
        }

        int ProbeHash(int vlAlpha, int vlBeta, int nDepth, ref int mv)
        {
            bool bMate; // 杀棋标志：如果是杀棋，那么不需要满足深度条件
            HashItem hsh = m_search.HashTable[pos.zobr.dwKey & (HASH_SIZE - 1)];
            if (hsh.dwLock0 != pos.zobr.dwLock0 || hsh.dwLock1 != pos.zobr.dwLock1)
            {
                mv = 0;
                return -MATE_VALUE;
            }
            mv = hsh.wmv;
            bMate = false;
            if (hsh.svl > WIN_VALUE)
            {
                if (hsh.svl < BAN_VALUE)
                {
                    return -MATE_VALUE; // 可能导致搜索的不稳定性，立刻退出，但最佳着法可能拿到
                }
                hsh.svl -= Convert.ToInt16(pos.nDistance);
                bMate = true;
            }
            else if (hsh.svl < -WIN_VALUE)
            {
                if (hsh.svl > -BAN_VALUE)
                {
                    return -MATE_VALUE; // 同上
                }
                hsh.svl += Convert.ToInt16(pos.nDistance);
                bMate = true;
            }
            if (hsh.ucDepth >= nDepth || bMate)
            {
                if (hsh.ucFlag == HASH_BETA)
                {
                    return (hsh.svl >= vlBeta ? hsh.svl : Convert.ToInt16(-MATE_VALUE));
                }
                else if (hsh.ucFlag == HASH_ALPHA)
                {
                    return (hsh.svl <= vlAlpha ? hsh.svl : Convert.ToInt16(-MATE_VALUE));
                }
                return hsh.svl;
            }
            return -MATE_VALUE;
        }

        int SearchQuiesc(int vlAlpha, int vlBeta)
        {
            int i, nGenMoves;
            int vl, vlBest;
            int[] mvs = new int[MAX_GEN_MOVES];
            InitializeIntArray(mvs);
            // 一个静态搜索分为以下几个阶段

            // 1. 检查重复局面
            vl = pos.RepStatus(1);
            if (vl != 0)
            {
                return pos.RepValue(vl);
            }

            // 2. 到达极限深度就返回局面评价
            if (pos.nDistance == LIMIT_DEPTH)
            {
                return pos.Evaluate();
            }

            // 3. 初始化最佳值
            vlBest = -MATE_VALUE; // 这样可以知道，是否一个走法都没走过(杀棋)

            if (pos.InCheck())
            {
                // 4. 如果被将军，则生成全部走法
                nGenMoves = pos.GenerateMoves(mvs, false);
                Array.Sort<int>(mvs, 0, 1, new CompareHistory());
            }
            else
            {
                // 5. 如果不被将军，先做局面评价
                vl = pos.Evaluate();
                if (vl > vlBest)
                {
                    vlBest = vl;
                    if (vl >= vlBeta)
                    {
                        return vl;
                    }
                    if (vl > vlAlpha)
                    {
                        vlAlpha = vl;
                    }
                }

                // 6. 如果局面评价没有截断，再生成吃子走法
                nGenMoves = pos.GenerateMoves(mvs, true);
                Array.Sort<int>(mvs, 0, nGenMoves, new CompareMvvLva());
            }

            // 7. 逐一走这些走法，并进行递归
            for (i = 0; i < nGenMoves; i++)
            {
                if (pos.MakeMove(mvs[i]))
                {
                    vl = -SearchQuiesc(-vlBeta, -vlAlpha);
                    pos.UndoMakeMove();

                    // 8. 进行Alpha-Beta大小判断和截断
                    if (vl > vlBest)
                    {    // 找到最佳值(但不能确定是Alpha、PV还是Beta走法)
                        vlBest = vl;        // "vlBest"就是目前要返回的最佳值，可能超出Alpha-Beta边界
                        if (vl >= vlBeta)
                        { // 找到一个Beta走法
                            return vl;        // Beta截断
                        }
                        if (vl > vlAlpha)
                        { // 找到一个PV走法
                            vlAlpha = vl;     // 缩小Alpha-Beta边界
                        }
                    }
                }
            }

            // 9. 所有走法都搜索完了，返回最佳值
            return vlBest == -MATE_VALUE ? pos.nDistance - MATE_VALUE : vlBest;
        }

        void RecordHash(int nFlag, int vl, int nDepth, int mv)
        {
            HashItem hsh;
            hsh = m_search.HashTable[pos.zobr.dwKey & (HASH_SIZE - 1)];
            if (hsh.ucDepth > nDepth)
            {
                return;
            }
            hsh.ucFlag = Convert.ToSByte(nFlag);
            hsh.ucDepth = Convert.ToSByte(nDepth);
            if (vl > WIN_VALUE)
            {
                if (mv == 0 && vl <= BAN_VALUE)
                {
                    return; // 可能导致搜索的不稳定性，并且没有最佳着法，立刻退出
                }
                hsh.svl = Convert.ToInt16(vl + pos.nDistance);
            }
            else if (vl < -WIN_VALUE)
            {
                if (mv == 0 && vl >= -BAN_VALUE)
                {
                    return; // 同上
                }
                hsh.svl = Convert.ToInt16(vl - pos.nDistance);
            }
            else
            {
                hsh.svl = Convert.ToInt16(vl);
            }
            hsh.wmv = Convert.ToUInt16(mv);
            hsh.dwLock0 = pos.zobr.dwLock0;
            hsh.dwLock1 = pos.zobr.dwLock1;
            m_search.HashTable[pos.zobr.dwKey & (HASH_SIZE - 1)] = hsh;
        }

        void SetBestMove(int mv, int nDepth)
        {
            int lpmvKiller0;
            m_search.nHistoryTable[mv] += nDepth * nDepth;
            lpmvKiller0 = m_search.mvKillers[pos.nDistance, 0];
            if (lpmvKiller0 != mv)
            {
                m_search.mvKillers[pos.nDistance, 1] = lpmvKiller0;
                m_search.mvKillers[pos.nDistance, 0] = mv;
            }
        }

        /* PRIVATE API (declared here for documentation purpose) */

        int _hox2xqwlight(string sMove)
        {
            int sx = sMove[0] - '0';
            int sy = sMove[1] - '0';
            int dx = sMove[2] - '0';
            int dy = sMove[3] - '0';
            int src = (3 + sx) + (3 + sy) * 16;
            int dst = (3 + dx) + (3 + dy) * 16;
            return src | (dst << 8);
        }

        string _xqwlight2hox(int move)
        {
            int src = move & 255;
            int dst = move >> 8;
            int sx = (src % 16) - 3;
            int sy = (src / 16) - 3;
            int dx = (dst % 16) - 3;
            int dy = (dst / 16) - 3;
            return string.Format("{0}{1}{2}{3}", sx, sy, dx, dy);
        }

        const int SQUARE_SIZE = 56;
        const int BOARD_EDGE = 8;
        const int BOARD_WIDTH = BOARD_EDGE + SQUARE_SIZE * 9 + BOARD_EDGE;
        const int BOARD_HEIGHT = BOARD_EDGE + SQUARE_SIZE * 10 + BOARD_EDGE;

        // 棋盘范围
        const int RANK_TOP = 3;
        const int RANK_BOTTOM = 12;
        const int FILE_LEFT = 3;
        const int FILE_RIGHT = 11;

        // 棋子编号
        const int PIECE_KING = 0;
        const int PIECE_ADVISOR = 1;
        const int PIECE_BISHOP = 2;
        const int PIECE_KNIGHT = 3;
        const int PIECE_ROOK = 4;
        const int PIECE_CANNON = 5;
        const int PIECE_PAWN = 6;

        const int MAX_GEN_MOVES = 128; // 最大的生成走法数
        const int MAX_MOVES = 256;     // 最大的历史走法数
        const int LIMIT_DEPTH = 64;    // 最大的搜索深度
        const int MATE_VALUE = 10000;  // 最高分值，即将死的分值
        const int BAN_VALUE = MATE_VALUE - 100; // 长将判负的分值，低于该值将不写入置换表
        const int WIN_VALUE = MATE_VALUE - 200; // 搜索出胜负的分值界限，超出此值就说明已经搜索出杀棋了
        const int DRAW_VALUE = 20;     // 和棋时返回的分数(取负值)
        const int ADVANCED_VALUE = 3;  // 先行权分值
        const int RANDOM_MASK = 7;     // 随机性分值
        const int NULL_MARGIN = 400;   // 空步裁剪的子力边界
        const int NULL_DEPTH = 2;      // 空步裁剪的裁剪深度
        const int HISTORY_SIZE = 65536;
        //const int HASH_SIZE = 1 << 20; // 置换表大小
        const int HASH_SIZE = 1 << 10;
        const int HASH_ALPHA = 1;      // ALPHA节点的置换表项
        const int HASH_BETA = 2;       // BETA节点的置换表项
        const int HASH_PV = 3;         // PV节点的置换表项
        const int BOOK_SIZE = 16384;   // 开局库大小

        static sbyte[] cucpcStartup = new sbyte[256]  {
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0, 20, 19, 18, 17, 16, 17, 18, 19, 20,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0, 21,  0,  0,  0,  0,  0, 21,  0,  0,  0,  0,  0,
  0,  0,  0, 22,  0, 22,  0, 22,  0, 22,  0, 22,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0, 14,  0, 14,  0, 14,  0, 14,  0, 14,  0,  0,  0,  0,
  0,  0,  0,  0, 13,  0,  0,  0,  0,  0, 13,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0, 12, 11, 10,  9,  8,  9, 10, 11, 12,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
        };

        static byte[,] cucvlPiecePos = new byte[7, 256] {
  { // 帅(将)
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  1,  1,  1,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  2,  2,  2,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0, 11, 15, 11,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }, { // 仕(士)
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0, 20,  0, 20,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0, 23,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0, 20,  0, 20,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }, { // 相(象)
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0, 20,  0,  0,  0, 20,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0, 18,  0,  0,  0, 23,  0,  0,  0, 18,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0, 20,  0,  0,  0, 20,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }, { // 马
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0, 90, 90, 90, 96, 90, 96, 90, 90, 90,  0,  0,  0,  0,
    0,  0,  0, 90, 96,103, 97, 94, 97,103, 96, 90,  0,  0,  0,  0,
    0,  0,  0, 92, 98, 99,103, 99,103, 99, 98, 92,  0,  0,  0,  0,
    0,  0,  0, 93,108,100,107,100,107,100,108, 93,  0,  0,  0,  0,
    0,  0,  0, 90,100, 99,103,104,103, 99,100, 90,  0,  0,  0,  0,
    0,  0,  0, 90, 98,101,102,103,102,101, 98, 90,  0,  0,  0,  0,
    0,  0,  0, 92, 94, 98, 95, 98, 95, 98, 94, 92,  0,  0,  0,  0,
    0,  0,  0, 93, 92, 94, 95, 92, 95, 94, 92, 93,  0,  0,  0,  0,
    0,  0,  0, 85, 90, 92, 93, 78, 93, 92, 90, 85,  0,  0,  0,  0,
    0,  0,  0, 88, 85, 90, 88, 90, 88, 90, 85, 88,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }, { // 车
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,206,208,207,213,214,213,207,208,206,  0,  0,  0,  0,
    0,  0,  0,206,212,209,216,233,216,209,212,206,  0,  0,  0,  0,
    0,  0,  0,206,208,207,214,216,214,207,208,206,  0,  0,  0,  0,
    0,  0,  0,206,213,213,216,216,216,213,213,206,  0,  0,  0,  0,
    0,  0,  0,208,211,211,214,215,214,211,211,208,  0,  0,  0,  0,
    0,  0,  0,208,212,212,214,215,214,212,212,208,  0,  0,  0,  0,
    0,  0,  0,204,209,204,212,214,212,204,209,204,  0,  0,  0,  0,
    0,  0,  0,198,208,204,212,212,212,204,208,198,  0,  0,  0,  0,
    0,  0,  0,200,208,206,212,200,212,206,208,200,  0,  0,  0,  0,
    0,  0,  0,194,206,204,212,200,212,204,206,194,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }, { // 炮
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,100,100, 96, 91, 90, 91, 96,100,100,  0,  0,  0,  0,
    0,  0,  0, 98, 98, 96, 92, 89, 92, 96, 98, 98,  0,  0,  0,  0,
    0,  0,  0, 97, 97, 96, 91, 92, 91, 96, 97, 97,  0,  0,  0,  0,
    0,  0,  0, 96, 99, 99, 98,100, 98, 99, 99, 96,  0,  0,  0,  0,
    0,  0,  0, 96, 96, 96, 96,100, 96, 96, 96, 96,  0,  0,  0,  0,
    0,  0,  0, 95, 96, 99, 96,100, 96, 99, 96, 95,  0,  0,  0,  0,
    0,  0,  0, 96, 96, 96, 96, 96, 96, 96, 96, 96,  0,  0,  0,  0,
    0,  0,  0, 97, 96,100, 99,101, 99,100, 96, 97,  0,  0,  0,  0,
    0,  0,  0, 96, 97, 98, 98, 98, 98, 98, 97, 96,  0,  0,  0,  0,
    0,  0,  0, 96, 96, 97, 99, 99, 99, 97, 96, 96,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }, { // 兵(卒)
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  9,  9,  9, 11, 13, 11,  9,  9,  9,  0,  0,  0,  0,
    0,  0,  0, 19, 24, 34, 42, 44, 42, 34, 24, 19,  0,  0,  0,  0,
    0,  0,  0, 19, 24, 32, 37, 37, 37, 32, 24, 19,  0,  0,  0,  0,
    0,  0,  0, 19, 23, 27, 29, 30, 29, 27, 23, 19,  0,  0,  0,  0,
    0,  0,  0, 14, 18, 20, 27, 29, 27, 20, 18, 14,  0,  0,  0,  0,
    0,  0,  0,  7,  0, 13,  0, 16,  0, 13,  0,  7,  0,  0,  0,  0,
    0,  0,  0,  7,  0,  7,  0, 15,  0,  7,  0,  7,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
    0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0
  }
};
        static readonly sbyte[] ccInBoard = new sbyte[256] {
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};

        static readonly sbyte[] ccInFort = new sbyte[256] {
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
};

        static readonly sbyte[] ccLegalSpan = new sbyte[512] {
                       0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 3, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 2, 1, 2, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 2, 1, 2, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 3, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
  0, 0, 0, 0, 0, 0, 0
};


        // 根据步长判断马是否蹩腿的数组
        static readonly sbyte[] ccKnightPin = new sbyte[512] {
                              0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,-16,  0,-16,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0, -1,  0,  0,  0,  1,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0, -1,  0,  0,  0,  1,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0, 16,  0, 16,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
  0,  0,  0,  0,  0,  0,  0
};

        // 帅(将)的步长
        static readonly sbyte[] ccKingDelta = new sbyte[4] { -16, -1, 1, 16 };
        // 仕(士)的步长
        static readonly sbyte[] ccAdvisorDelta = new sbyte[4] { -17, -15, 15, 17 };
        // 马的步长，以帅(将)的步长作为马腿
        static readonly sbyte[,] ccKnightDelta = new sbyte[4, 2] { { -33, -31 }, { -18, 14 }, { -14, 18 }, { 31, 33 } };
        // 马被将军的步长，以仕(士)的步长作为马腿
        static readonly sbyte[,] ccKnightCheckDelta = new sbyte[4, 2] { { -33, -18 }, { -31, -14 }, { 14, 31 }, { 18, 33 } };


        // 判断棋子是否在棋盘中
        static bool IN_BOARD(int sq)
        {
            return ccInBoard[sq] != 0;
        }

        // 判断棋子是否在九宫中
        static bool IN_FORT(int sq)
        {
            return ccInFort[sq] != 0;
        }

        // 获得格子的横坐标
        static int RANK_Y(int sq)
        {
            return sq >> 4;
        }

        // 获得格子的纵坐标
        static int FILE_X(int sq)
        {
            return sq & 15;
        }

        // 根据纵坐标和横坐标获得格子
        static int COORD_XY(int x, int y)
        {
            return x + (y << 4);
        }

        // 翻转格子
        static int SQUARE_FLIP(int sq)
        {
            return 254 - sq;
        }

        // 纵坐标水平镜像
        static int FILE_FLIP(int x)
        {
            return 14 - x;
        }

        // 横坐标垂直镜像
        static int RANK_FLIP(int y)
        {
            return 15 - y;
        }

        // 格子水平镜像
        static int MIRROR_SQUARE(int sq)
        {
            return COORD_XY(FILE_FLIP(FILE_X(sq)), RANK_Y(sq));
        }

        // 格子水平镜像
        static int SQUARE_FORWARD(int sq, int sd)
        {
            return sq - 16 + (sd << 5);
        }

        // 走法是否符合帅(将)的步长
        static bool KING_SPAN(int sqSrc, int sqDst)
        {
            return ccLegalSpan[sqDst - sqSrc + 256] == 1;
        }

        // 走法是否符合仕(士)的步长
        static bool ADVISOR_SPAN(int sqSrc, int sqDst)
        {
            return ccLegalSpan[sqDst - sqSrc + 256] == 2;
        }

        // 走法是否符合相(象)的步长
        static bool BISHOP_SPAN(int sqSrc, int sqDst)
        {
            return ccLegalSpan[sqDst - sqSrc + 256] == 3;
        }

        // 相(象)眼的位置
        static int BISHOP_PIN(int sqSrc, int sqDst)
        {
            return (sqSrc + sqDst) >> 1;
        }

        // 马腿的位置
        static int KNIGHT_PIN(int sqSrc, int sqDst)
        {
            return sqSrc + ccKnightPin[sqDst - sqSrc + 256];
        }

        // 是否未过河
        static bool HOME_HALF(int sq, int sd)
        {
            return (sq & 0x80) != (sd << 7);
        }

        // 是否已过河
        static bool AWAY_HALF(int sq, int sd)
        {
            return (sq & 0x80) == (sd << 7);
        }

        // 是否在河的同一边
        static bool SAME_HALF(int sqSrc, int sqDst)
        {
            return ((sqSrc ^ sqDst) & 0x80) == 0;
        }

        // 是否在同一行
        static bool SAME_RANK(int sqSrc, int sqDst)
        {
            return ((sqSrc ^ sqDst) & 0xf0) == 0;
        }

        // 是否在同一列
        static bool SAME_FILE(int sqSrc, int sqDst)
        {
            return ((sqSrc ^ sqDst) & 0x0f) == 0;
        }

        // 获得红黑标记(红子是8，黑子是16)
        static int SIDE_TAG(int sd)
        {
            return 8 + (sd << 3);
        }

        // 获得对方红黑标记
        static int OPP_SIDE_TAG(int sd)
        {
            return 16 - (sd << 3);
        }

        // 获得走法的起点
        static int SRC(int mv)
        {
            return mv & 255;
        }

        // 获得走法的终点
        static int DST(int mv)
        {
            return mv >> 8;
        }

        // 根据起点和终点获得走法
        static int MOVE(int sqSrc, int sqDst)
        {
            return sqSrc + sqDst * 256;
        }

        // 走法水平镜像
        static int MIRROR_MOVE(int mv)
        {
            return MOVE(MIRROR_SQUARE(SRC(mv)), MIRROR_SQUARE(DST(mv)));
        }

        struct HashItem
        {
            public sbyte ucDepth;
            public sbyte ucFlag;
            public short svl;
            public ushort wmv;
            //public ushort wReserved;
            public uint dwLock0;
            public uint dwLock1;
        }

        // 开局库项结构
        struct BookItem
        {
            public uint dwLock;
            public ushort wmv;
            public ushort wvl;
        }

        // 与搜索有关的全局变量
        class Search
        {
            public int mvResult = 0;                  // 电脑走的棋
            public int[] nHistoryTable = new int[HISTORY_SIZE];      // 历史表
            public int[,] mvKillers = new int[LIMIT_DEPTH, 2]; // 杀手走法表
            public HashItem[] HashTable = new HashItem[HASH_SIZE]; // 置换表
            public int nBookSize = 0;                 // 开局库大小
            public BookItem[] BookTable = new BookItem[BOOK_SIZE]; // 开局库

            public Search()
            {
                InitializeIntArray(nHistoryTable);
                for (int i = 0; i < LIMIT_DEPTH; i++)
                {
                    mvKillers[i, 0] = new int();
                    mvKillers[i, 1] = new int();
                }
                for (int hashItem = 0; hashItem < HashTable.Length; hashItem++)
                {
                    HashTable[hashItem] = new HashItem();
                }

                for (int bookItem = 0; bookItem < BookTable.Length; bookItem++)
                {
                    BookTable[bookItem] = new BookItem();
                }
            }

            public int BinarySearch(BookItem item)
            {
                return Array.BinarySearch<BookItem>(BookTable, 0, nBookSize, item, new CompareBookItem());
            }
        }

        static Search m_search = new Search();

        // RC4密码流生成器
        class RC4Struct
        {
            byte[] s = new byte[256];
            int x, y;

            public RC4Struct()
            {
                for (int i = 0; i < s.Length; i++)
                {
                    s[i] = new byte();
                }
            }

            public void InitZero()   // 用空密钥初始化密码流生成器
            {
                int i, j;
                byte uc;

                x = y = j = 0;
                for (i = 0; i < 256; i++)
                {
                    s[i] = Convert.ToByte(i);
                }

                for (i = 0; i < 256; i++)
                {
                    j = (j + s[i]) & 255;
                    uc = s[i];
                    s[i] = s[j];
                    s[j] = uc;
                }
            }

            public byte NextByte()
            {  // 生成密码流的下一个字节
                byte uc;
                x = (x + 1) & 255;
                y = (y + s[x]) & 255;
                uc = s[x];
                s[x] = s[y];
                s[y] = uc;
                return s[(s[x] + s[y]) & 255];
            }

            public uint NextLong()
            { // 生成密码流的下四个字节
                byte uc0, uc1, uc2, uc3;
                uc0 = NextByte();
                uc1 = NextByte();
                uc2 = NextByte();
                uc3 = NextByte();
                return Convert.ToUInt32(uc0) + (Convert.ToUInt32(uc1) << 8) + (Convert.ToUInt32(uc2) << 16) + (Convert.ToUInt32(uc3) << 24);
            }
        }


        // Zobrist结构
        class ZobristStruct
        {
            public uint dwKey;
            public uint dwLock0;
            public uint dwLock1;

            public void InitZero()
            {                 // 用零填充Zobrist
                dwKey = dwLock0 = dwLock1 = 0;
            }

            public void InitRC4(RC4Struct rc4)
            {        // 用密码流填充Zobrist
                dwKey = rc4.NextLong();
                dwLock0 = rc4.NextLong();
                dwLock1 = rc4.NextLong();
            }

            public void Xor(ZobristStruct zobr)
            { // 执行XOR操作
                dwKey ^= zobr.dwKey;
                dwLock0 ^= zobr.dwLock0;
                dwLock1 ^= zobr.dwLock1;
            }

            public void Xor(ZobristStruct zobr1, ZobristStruct zobr2)
            {
                dwKey ^= zobr1.dwKey ^ zobr2.dwKey;
                dwLock0 ^= zobr1.dwLock0 ^ zobr2.dwLock0;
                dwLock1 ^= zobr1.dwLock1 ^ zobr2.dwLock1;
            }
        };

        // Zobrist表
        class Zobrist
        {
            public ZobristStruct Player = new ZobristStruct();
            public ZobristStruct[,] Table = new ZobristStruct[14, 256];

            public Zobrist()
            {
                //Table.Initialize();
                for (int i = 0; i < 14; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        Table[i, j] = new ZobristStruct();
                    }
                }

            }
        }

        static Zobrist m_zobrist = new Zobrist();

        // 历史走法信息(占4字节)
        class MoveStruct
        {
            public UInt16 wmv;
            public sbyte ucpcCaptured;
            public bool ucbCheck;
            public uint dwKey;

            public void Set(int mv, sbyte pcCaptured, bool bCheck, uint dwKey_)
            {
                wmv = Convert.ToUInt16(mv);
                ucpcCaptured = pcCaptured;
                ucbCheck = bCheck;
                dwKey = dwKey_;
            }
        } // mvs

        // 局面结构

        class PositionStruct
        {
            public int sdPlayer;                   // 轮到谁走，0=红方，1=黑方
            public sbyte[] ucpcSquares = new sbyte[MAX_MOVES];          // 棋盘上的棋子
            public int vlWhite;
            public int vlBlack;           // 红、黑双方的子力价值
            public int nDistance;
            public int nMoveNum;        // 距离根节点的步数，历史走法数
            public MoveStruct[] mvsList = new MoveStruct[MAX_MOVES];  // 历史走法信息列表

            public ZobristStruct zobr = new ZobristStruct();             // Zobrist

            public PositionStruct()
            {
                for (int i = 0; i < MAX_MOVES; i++)
                {
                    ucpcSquares[i] = 0;
                    mvsList[i] = new MoveStruct();
                }
            }

            public void ClearBoard()
            {         // 清空棋盘
                sdPlayer = vlWhite = vlBlack = nDistance = 0;
                for (int i = 0; i < ucpcSquares.Length; i++)
                    ucpcSquares[i] = 0;
                zobr.InitZero();
            }

            public void SetIrrev()
            {           // 清空(初始化)历史走法信息
                mvsList[0].Set(0, 0, Checked(), zobr.dwKey);
                nMoveNum = 1;
            }

            public void Startup(sbyte[,] board) //             // 初始化棋盘
            {
                int sq;
                ClearBoard();
                if (board != null)
                {
                    for (int i = 0; i < 10; i++)
                        for (int j = 0; j < 9; j++)
                        {
                            if (board[i, j] > 0)
                            {
                                sq = (3 + i) * 16 + 3 + j;
                                AddPiece(sq, board[i, j]);
                            }
                        }
                }
                else
                {
                    int pc;
                    for (sq = 0; sq < 256; sq++)
                    {
                        pc = cucpcStartup[sq];
                        if (pc != 0)
                        {
                            AddPiece(sq, pc);
                        }
                    }
                }
                SetIrrev();
            }

            public void ChangeSide()
            {         // 交换走子方
                sdPlayer = 1 - sdPlayer;
                zobr.Xor(m_zobrist.Player);
            }

            public void AddPiece(int sq, int pc)
            { // 在棋盘上放一枚棋子
                ucpcSquares[sq] = Convert.ToSByte(pc);
                // 红方加分，黑方(注意"cucvlPiecePos"取值要颠倒)减分
                if (pc < 16)
                {
                    vlWhite += cucvlPiecePos[pc - 8, sq];
                    zobr.Xor(m_zobrist.Table[pc - 8, sq]);
                }
                else
                {
                    vlBlack += cucvlPiecePos[pc - 16, SQUARE_FLIP(sq)];
                    zobr.Xor(m_zobrist.Table[pc - 9, sq]);
                }
            }

            public void DelPiece(int sq, int pc)
            { // 从棋盘上拿走一枚棋子
                ucpcSquares[sq] = 0;
                // 红方减分，黑方(注意"cucvlPiecePos"取值要颠倒)加分
                if (pc < 16)
                {
                    vlWhite -= cucvlPiecePos[pc - 8, sq];
                    zobr.Xor(m_zobrist.Table[pc - 8, sq]);
                }
                else
                {
                    vlBlack -= cucvlPiecePos[pc - 16, SQUARE_FLIP(sq)];
                    zobr.Xor(m_zobrist.Table[pc - 9, sq]);
                }
            }

            public int Evaluate()
            {      // 局面评价函数
                return (sdPlayer == 0 ? vlWhite - vlBlack : vlBlack - vlWhite) + ADVANCED_VALUE;
            }

            public bool InCheck()
            {      // 是否被将军
                return mvsList[nMoveNum - 1].ucbCheck;
            }

            public bool Captured()
            {     // 上一步是否吃子
                return mvsList[nMoveNum - 1].ucpcCaptured != 0;
            }

            public sbyte MovePiece(int mv)                      // 搬一步棋的棋子
            {
                int sqSrc, sqDst, pc;
                sbyte pcCaptured;
                sqSrc = SRC(mv);
                sqDst = DST(mv);
                pcCaptured = ucpcSquares[sqDst];
                if (pcCaptured != 0)
                {
                    DelPiece(sqDst, pcCaptured);
                }
                pc = ucpcSquares[sqSrc];
                DelPiece(sqSrc, pc);
                AddPiece(sqDst, pc);
                return pcCaptured;
            }

            public void UndoMovePiece(int mv, sbyte pcCaptured) // 撤消搬一步棋的棋子
            {
                int sqSrc, sqDst, pc;
                sqSrc = SRC(mv);
                sqDst = DST(mv);
                pc = ucpcSquares[sqDst];
                DelPiece(sqDst, pc);
                AddPiece(sqSrc, pc);
                if (pcCaptured != 0)
                {
                    AddPiece(sqDst, pcCaptured);
                }
            }

            public bool MakeMove(int mv)                      // 走一步棋
            {
                sbyte pcCaptured;
                uint dwKey;

                dwKey = zobr.dwKey;
                pcCaptured = MovePiece(mv);
                if (Checked())
                {
                    UndoMovePiece(mv, pcCaptured);
                    return false;
                }
                ChangeSide();
                mvsList[nMoveNum].Set(mv, pcCaptured, Checked(), dwKey);
                nMoveNum++;
                nDistance++;
                return true;
            }

            public void UndoMakeMove()
            {                   // 撤消走一步棋
                nDistance--;
                nMoveNum--;
                ChangeSide();
                UndoMovePiece(mvsList[nMoveNum].wmv, mvsList[nMoveNum].ucpcCaptured);
            }

            public void NullMove()
            {                       // 走一步空步
                uint dwKey;
                dwKey = zobr.dwKey;
                ChangeSide();
                mvsList[nMoveNum].Set(0, 0, false, dwKey);
                nMoveNum++;
                nDistance++;
            }

            public void UndoNullMove()
            {                   // 撤消走一步空步
                nDistance--;
                nMoveNum--;
                ChangeSide();
            }

            // 生成所有走法，如果"bCapture"为"TRUE"则只生成吃子走法
            public int GenerateMoves(int[] mvs, bool bCapture) //= false) ;
            {
                int i, j, nGenMoves, nDelta, sqSrc, sqDst;
                int pcSelfSide, pcOppSide, pcSrc, pcDst;
                // 生成所有走法，需要经过以下几个步骤：

                nGenMoves = 0;
                pcSelfSide = SIDE_TAG(sdPlayer);
                pcOppSide = OPP_SIDE_TAG(sdPlayer);
                for (sqSrc = 0; sqSrc < 256; sqSrc++)
                {

                    // 1. 找到一个本方棋子，再做以下判断：
                    pcSrc = ucpcSquares[sqSrc];
                    if ((pcSrc & pcSelfSide) == 0)
                    {
                        continue;
                    }

                    // 2. 根据棋子确定走法
                    switch (pcSrc - pcSelfSide)
                    {
                        case PIECE_KING:
                            for (i = 0; i < 4; i++)
                            {
                                sqDst = sqSrc + ccKingDelta[i];
                                if (!IN_FORT(sqDst))
                                {
                                    continue;
                                }
                                pcDst = ucpcSquares[sqDst];
                                if (bCapture ? (pcDst & pcOppSide) != 0 : (pcDst & pcSelfSide) == 0)
                                {
                                    mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                    nGenMoves++;
                                }
                            }
                            break;
                        case PIECE_ADVISOR:
                            for (i = 0; i < 4; i++)
                            {
                                sqDst = sqSrc + ccAdvisorDelta[i];
                                if (!IN_FORT(sqDst))
                                {
                                    continue;
                                }
                                pcDst = ucpcSquares[sqDst];
                                if (bCapture ? (pcDst & pcOppSide) != 0 : (pcDst & pcSelfSide) == 0)
                                {
                                    mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                    nGenMoves++;
                                }
                            }
                            break;
                        case PIECE_BISHOP:
                            for (i = 0; i < 4; i++)
                            {
                                sqDst = sqSrc + ccAdvisorDelta[i];
                                if (!(IN_BOARD(sqDst) && HOME_HALF(sqDst, sdPlayer) && ucpcSquares[sqDst] == 0))
                                {
                                    continue;
                                }
                                sqDst += ccAdvisorDelta[i];
                                pcDst = ucpcSquares[sqDst];
                                if (bCapture ? (pcDst & pcOppSide) != 0 : (pcDst & pcSelfSide) == 0)
                                {
                                    mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                    nGenMoves++;
                                }
                            }
                            break;
                        case PIECE_KNIGHT:
                            for (i = 0; i < 4; i++)
                            {
                                sqDst = sqSrc + ccKingDelta[i];
                                if (ucpcSquares[sqDst] != 0)
                                {
                                    continue;
                                }
                                for (j = 0; j < 2; j++)
                                {
                                    sqDst = sqSrc + ccKnightDelta[i, j];
                                    if (!IN_BOARD(sqDst))
                                    {
                                        continue;
                                    }
                                    pcDst = ucpcSquares[sqDst];
                                    if (bCapture ? (pcDst & pcOppSide) != 0 : (pcDst & pcSelfSide) == 0)
                                    {
                                        mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                        nGenMoves++;
                                    }
                                }
                            }
                            break;
                        case PIECE_ROOK:
                            for (i = 0; i < 4; i++)
                            {
                                nDelta = ccKingDelta[i];
                                sqDst = sqSrc + nDelta;
                                while (IN_BOARD(sqDst))
                                {
                                    pcDst = ucpcSquares[sqDst];
                                    if (pcDst == 0)
                                    {
                                        if (!bCapture)
                                        {
                                            mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                            nGenMoves++;
                                        }
                                    }
                                    else
                                    {
                                        if ((pcDst & pcOppSide) != 0)
                                        {
                                            mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                            nGenMoves++;
                                        }
                                        break;
                                    }
                                    sqDst += nDelta;
                                }
                            }
                            break;
                        case PIECE_CANNON:
                            for (i = 0; i < 4; i++)
                            {
                                nDelta = ccKingDelta[i];
                                sqDst = sqSrc + nDelta;
                                while (IN_BOARD(sqDst))
                                {
                                    pcDst = ucpcSquares[sqDst];
                                    if (pcDst == 0)
                                    {
                                        if (!bCapture)
                                        {
                                            mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                            nGenMoves++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    sqDst += nDelta;
                                }
                                sqDst += nDelta;
                                while (IN_BOARD(sqDst))
                                {
                                    pcDst = ucpcSquares[sqDst];
                                    if (pcDst != 0)
                                    {
                                        if ((pcDst & pcOppSide) != 0)
                                        {
                                            mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                            nGenMoves++;
                                        }
                                        break;
                                    }
                                    sqDst += nDelta;
                                }
                            }
                            break;
                        case PIECE_PAWN:
                            sqDst = SQUARE_FORWARD(sqSrc, sdPlayer);
                            if (IN_BOARD(sqDst))
                            {
                                pcDst = ucpcSquares[sqDst];
                                if (bCapture ? (pcDst & pcOppSide) != 0 : (pcDst & pcSelfSide) == 0)
                                {
                                    mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                    nGenMoves++;
                                }
                            }
                            if (AWAY_HALF(sqSrc, sdPlayer))
                            {
                                for (nDelta = -1; nDelta <= 1; nDelta += 2)
                                {
                                    sqDst = sqSrc + nDelta;
                                    if (IN_BOARD(sqDst))
                                    {
                                        pcDst = ucpcSquares[sqDst];
                                        if (bCapture ? (pcDst & pcOppSide) != 0 : (pcDst & pcSelfSide) == 0)
                                        {
                                            mvs[nGenMoves] = MOVE(sqSrc, sqDst);
                                            nGenMoves++;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                return nGenMoves;
            }

            public bool LegalMove(int mv)               // 判断走法是否合理
            {
                int sqSrc, sqDst, sqPin;
                int pcSelfSide, pcSrc, pcDst, nDelta;
                // 判断走法是否合法，需要经过以下的判断过程：

                // 1. 判断起始格是否有自己的棋子
                sqSrc = SRC(mv);
                pcSrc = ucpcSquares[sqSrc];
                pcSelfSide = SIDE_TAG(sdPlayer);
                if ((pcSrc & pcSelfSide) == 0)
                {
                    return false;
                }

                // 2. 判断目标格是否有自己的棋子
                sqDst = DST(mv);
                pcDst = ucpcSquares[sqDst];
                if ((pcDst & pcSelfSide) != 0)
                {
                    return false;
                }

                // 3. 根据棋子的类型检查走法是否合理
                switch (pcSrc - pcSelfSide)
                {
                    case PIECE_KING:
                        return IN_FORT(sqDst) && KING_SPAN(sqSrc, sqDst);
                    case PIECE_ADVISOR:
                        return IN_FORT(sqDst) && ADVISOR_SPAN(sqSrc, sqDst);
                    case PIECE_BISHOP:
                        return SAME_HALF(sqSrc, sqDst) && BISHOP_SPAN(sqSrc, sqDst) &&
                            ucpcSquares[BISHOP_PIN(sqSrc, sqDst)] == 0;
                    case PIECE_KNIGHT:
                        sqPin = KNIGHT_PIN(sqSrc, sqDst);
                        return sqPin != sqSrc && ucpcSquares[sqPin] == 0;
                    case PIECE_ROOK:
                    case PIECE_CANNON:
                        if (SAME_RANK(sqSrc, sqDst))
                        {
                            nDelta = (sqDst < sqSrc ? -1 : 1);
                        }
                        else if (SAME_FILE(sqSrc, sqDst))
                        {
                            nDelta = (sqDst < sqSrc ? -16 : 16);
                        }
                        else
                        {
                            return false;
                        }
                        sqPin = sqSrc + nDelta;
                        while (sqPin != sqDst && ucpcSquares[sqPin] == 0)
                        {
                            sqPin += nDelta;
                        }
                        if (sqPin == sqDst)
                        {
                            return pcDst == 0 || pcSrc - pcSelfSide == PIECE_ROOK;
                        }
                        else if (pcDst != 0 && pcSrc - pcSelfSide == PIECE_CANNON)
                        {
                            sqPin += nDelta;
                            while (sqPin != sqDst && ucpcSquares[sqPin] == 0)
                            {
                                sqPin += nDelta;
                            }
                            return sqPin == sqDst;
                        }
                        else
                        {
                            return false;
                        }
                    case PIECE_PAWN:
                        if (AWAY_HALF(sqDst, sdPlayer) && (sqDst == sqSrc - 1 || sqDst == sqSrc + 1))
                        {
                            return false;
                        }
                        return sqDst == SQUARE_FORWARD(sqSrc, sdPlayer);
                    default:
                        return false;
                }
            }

            public bool Checked()                   // 判断是否被将军
            {
                int i, j, sqSrc, sqDst;
                int pcSelfSide, pcOppSide, pcDst, nDelta;
                pcSelfSide = SIDE_TAG(sdPlayer);
                pcOppSide = OPP_SIDE_TAG(sdPlayer);
                // 找到棋盘上的帅(将)，再做以下判断：

                for (sqSrc = 0; sqSrc < 256; sqSrc++)
                {
                    if (ucpcSquares[sqSrc] != pcSelfSide + PIECE_KING)
                    {
                        continue;
                    }

                    // 1. 判断是否被对方的兵(卒)将军
                    if (ucpcSquares[SQUARE_FORWARD(sqSrc, sdPlayer)] == pcOppSide + PIECE_PAWN)
                    {
                        return true;
                    }
                    for (nDelta = -1; nDelta <= 1; nDelta += 2)
                    {
                        if (ucpcSquares[sqSrc + nDelta] == pcOppSide + PIECE_PAWN)
                        {
                            return true;
                        }
                    }

                    // 2. 判断是否被对方的马将军(以仕(士)的步长当作马腿)
                    for (i = 0; i < 4; i++)
                    {
                        if (ucpcSquares[sqSrc + ccAdvisorDelta[i]] != 0)
                        {
                            continue;
                        }
                        for (j = 0; j < 2; j++)
                        {
                            pcDst = ucpcSquares[sqSrc + ccKnightCheckDelta[i, j]];
                            if (pcDst == pcOppSide + PIECE_KNIGHT)
                            {
                                return true;
                            }
                        }
                    }

                    // 3. 判断是否被对方的车或炮将军(包括将帅对脸)
                    for (i = 0; i < 4; i++)
                    {
                        nDelta = ccKingDelta[i];
                        sqDst = sqSrc + nDelta;
                        while (IN_BOARD(sqDst))
                        {
                            pcDst = ucpcSquares[sqDst];
                            if (pcDst != 0)
                            {
                                if (pcDst == pcOppSide + PIECE_ROOK || pcDst == pcOppSide + PIECE_KING)
                                {
                                    return true;
                                }
                                break;
                            }
                            sqDst += nDelta;
                        }
                        sqDst += nDelta;
                        while (IN_BOARD(sqDst))
                        {
                            pcDst = ucpcSquares[sqDst];
                            if (pcDst != 0)
                            {
                                if (pcDst == pcOppSide + PIECE_CANNON)
                                {
                                    return true;
                                }
                                break;
                            }
                            sqDst += nDelta;
                        }
                    }
                    return false;
                }
                return false;
            }

            public bool IsMate()                          // 判断是否被杀
            {
                int i, nGenMoveNum;
                sbyte pcCaptured;
                int[] mvs = new int[MAX_GEN_MOVES];
                InitializeIntArray(mvs);

                nGenMoveNum = GenerateMoves(mvs, false);
                for (i = 0; i < nGenMoveNum; i++)
                {
                    pcCaptured = MovePiece(mvs[i]);
                    if (!Checked())
                    {
                        UndoMovePiece(mvs[i], pcCaptured);
                        return false;
                    }
                    else
                    {
                        UndoMovePiece(mvs[i], pcCaptured);
                    }
                }
                return true;
            }

            public int DrawValue()
            {                 // 和棋分值
                return (nDistance & 1) == 0 ? -DRAW_VALUE : DRAW_VALUE;
            }

            public int RepStatus(int nRecur) // = 1) ;        // 检测重复局面
            {
                bool bSelfSide, bPerpCheck, bOppPerpCheck;
                MoveStruct lpmvs;

                bSelfSide = false;
                bPerpCheck = bOppPerpCheck = true;
                int index = nMoveNum - 1;
                while (index >= 0 && mvsList[index].wmv != 0 && mvsList[index].ucpcCaptured == 0)
                {
                    lpmvs = mvsList[index];
                    if (bSelfSide)
                    {
                        bPerpCheck = bPerpCheck && lpmvs.ucbCheck;
                        if (lpmvs.dwKey == zobr.dwKey)
                        {
                            nRecur--;
                            if (nRecur == 0)
                            {
                                return 1 + (bPerpCheck ? 2 : 0) + (bOppPerpCheck ? 4 : 0);
                            }
                        }
                    }
                    else
                    {
                        bOppPerpCheck = bOppPerpCheck && lpmvs.ucbCheck;
                    }
                    bSelfSide = !bSelfSide;
                    index--;
                }
                return 0;
            }

            public int RepValue(int nRepStatus)
            {        // 重复局面分值
                int vlReturn;
                vlReturn = ((nRepStatus & 2) == 0 ? 0 : nDistance - BAN_VALUE) +
                    ((nRepStatus & 4) == 0 ? 0 : BAN_VALUE - nDistance);
                return vlReturn == 0 ? DrawValue() : vlReturn;
            }

            public bool NullOkay()
            {                 // 判断是否允许空步裁剪
                return (sdPlayer == 0 ? vlWhite : vlBlack) > NULL_MARGIN;
            }

            public void Mirror(PositionStruct posMirror) // 对局面镜像
            {
                int sq, pc;
                posMirror.ClearBoard();
                for (sq = 0; sq < 256; sq++)
                {
                    pc = ucpcSquares[sq];
                    if (pc != 0)
                    {
                        posMirror.AddPiece(MIRROR_SQUARE(sq), pc);
                    }
                }
                if (sdPlayer == 1)
                {
                    posMirror.ChangeSide();
                }
                posMirror.SetIrrev();
            }
        };

        static PositionStruct pos = new PositionStruct();

        const int PHASE_HASH = 0;
        const int PHASE_KILLER_1 = 1;
        const int PHASE_KILLER_2 = 2;
        const int PHASE_GEN_MOVES = 3;
        const int PHASE_REST = 4;

        class CompareBookItem : System.Collections.Generic.IComparer<BookItem>
        {
            public int Compare(BookItem lhs, BookItem rhs)
            {
                if (lhs.dwLock > rhs.dwLock)
                    return 1;
                else if (lhs.dwLock == rhs.dwLock)
                    return 0;
                else
                    return -1;
            }
        }

        class CompareHistory : System.Collections.Generic.IComparer<int>
        {
            public int Compare(int lhs, int rhs)
            {
                return m_search.nHistoryTable[lhs] - m_search.nHistoryTable[rhs];
            }
        }

        class CompareMvvLva : System.Collections.Generic.IComparer<int>
        {
            static sbyte[] cucMvvLva = new sbyte[24] {
              0, 0, 0, 0, 0, 0, 0, 0,
              5, 1, 1, 3, 4, 3, 2, 0,
              5, 1, 1, 3, 4, 3, 2, 0
            };

            public int Compare(int mv1, int mv2)
            {
                return MvvLva(mv1) - MvvLva(mv2);
            }

            int MvvLva(int mv)
            {
                return (cucMvvLva[pos.ucpcSquares[DST(mv)]] << 3) - cucMvvLva[pos.ucpcSquares[SRC(mv)]];
            }

        }

        class SortStruct
        {
            int mvHash, mvKiller1, mvKiller2; // 置换表走法和两个杀手走法
            int nPhase, nIndex, nGenMoves;    // 当前阶段，当前采用第几个走法，总共有几个走法
            int[] mvs = new int[MAX_GEN_MOVES];           // 所有的走法

            public SortStruct()
            {
                InitializeIntArray(mvs);
            }

            public void Init(int mvHash_)
            { // 初始化，设定置换表走法和两个杀手走法
                mvHash = mvHash_;
                mvKiller1 = m_search.mvKillers[pos.nDistance, 0];
                mvKiller2 = m_search.mvKillers[pos.nDistance, 1];
                nPhase = PHASE_HASH;
            }

            public int Next() // 得到下一个走法
            {
                int mv;
                switch (nPhase)
                {
                    // "nPhase"表示着法启发的若干阶段，依次为：

                    // 0. 置换表着法启发，完成后立即进入下一阶段；
                    case PHASE_HASH:
                        nPhase = PHASE_KILLER_1;
                        if (mvHash != 0)
                        {
                            return mvHash;
                        }
                        goto case PHASE_KILLER_1;
                    // 技巧：这里没有"break"，表示"switch"的上一个"case"执行完后紧接着做下一个"case"，下同

                    // 1. 杀手着法启发(第一个杀手着法)，完成后立即进入下一阶段；
                    case PHASE_KILLER_1:
                        nPhase = PHASE_KILLER_2;
                        if (mvKiller1 != mvHash && mvKiller1 != 0 && pos.LegalMove(mvKiller1))
                        {
                            return mvKiller1;
                        }
                        goto case PHASE_KILLER_2;

                    // 2. 杀手着法启发(第二个杀手着法)，完成后立即进入下一阶段；
                    case PHASE_KILLER_2:
                        nPhase = PHASE_GEN_MOVES;
                        if (mvKiller2 != mvHash && mvKiller2 != 0 && pos.LegalMove(mvKiller2))
                        {
                            return mvKiller2;
                        }
                        goto case PHASE_GEN_MOVES;

                    // 3. 生成所有着法，完成后立即进入下一阶段；
                    case PHASE_GEN_MOVES:
                        nPhase = PHASE_REST;
                        nGenMoves = pos.GenerateMoves(mvs, false);
                        Array.Sort<int>(mvs, 0, nGenMoves, new CompareHistory());
                        //qsort(mvs, nGenMoves, sizeof(int), CompareHistory);
                        nIndex = 0;
                        goto case PHASE_REST;

                    // 4. 对剩余着法做历史表启发；
                    case PHASE_REST:
                        while (nIndex < nGenMoves)
                        {
                            mv = mvs[nIndex];
                            nIndex++;
                            if (mv != mvHash && mv != mvKiller1 && mv != mvKiller2)
                            {
                                return mv;
                            }
                        }
                        return 0;

                    // 5. 没有着法了，返回零。
                    default:
                        return 0;
                }
            }
        }

    }

}
