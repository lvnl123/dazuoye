using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labyrinth.Common
{
    /// <summary>
    /// 迷宫
    /// </summary>
    internal class Maze : IDisposable
    {
        private MazeCell[,] cells;
        private readonly Stack<MazeCell> stack = new Stack<MazeCell>();
        private readonly Random rand = new Random();
        private int _width, _height;
        private Bitmap mazeBitmap; // 用于保存迷宫的位图
        private float cellWidth;
        private float cellHeight;
        private Point playerPosition;
        private float playerRadius;
        private bool _isMove = true;
        private int _canvasWidth;
        private int _canvasHeight;
        private MazeType _mazeType = MazeType.Default;
        public Maze()
        {

        }
        public Bitmap MazeBitmap => mazeBitmap;
        public bool IsMove => _isMove;
        public MazeType MazeType => _mazeType;
        public int CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                _canvasWidth = value;
            }
        }
        public int CanvasHeight
        {
            get => _canvasHeight;
            set { _canvasHeight = value; }
        }
        private void GenerateMaze(MazeType mazeType)
        {
            switch(mazeType)
            {
                case MazeType.Default:
                    GenerateMaze_RecursiveBacktracking();
                    break;
                case MazeType.DFS:
                    GenerateMaze_DFS();
                    break;
                case MazeType.Prim:
                    GenerateMaze_Prim();
                    break;
                case MazeType.RecursiveDivision:
                    GenerateMaze_RecursiveDivision();
                    break;
                case MazeType.RecursiveBacktracking:
                    GenerateMaze_RecursiveBacktracking();
                    break;
            }
        }

        /// <summary>
        /// 获取方向
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tuple<int, int>> GetDirections()
        {
            yield return Tuple.Create(-1, 0);
            yield return Tuple.Create(1, 0);
            yield return Tuple.Create(0, -1);
            yield return Tuple.Create(0, 1);
        }

        #region 深度优先搜索算法
        private void GenerateMaze_DFS()
        {
            // 选择迷宫的左上角的点作为起始点
            int startX = 0;
            int startY = 0;

            // 使用DFS生成迷宫
            GenerateMaze(startX, startY);

            // 将起始点的左面的墙壁设为入口
            cells[startX, startY].LeftWall = false;

            // 找到迷宫的一个最远的边缘点，将它的边缘的墙壁设为出口
            int maxDist = 0;
            int endX = 0, endY = 0;
            bool isBottomEdge = false;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int dist = Math.Abs(x - startX) + Math.Abs(y - startY);
                    if (dist > maxDist && (x == 0 || y == 0 || x == _width - 1 || y == _height - 1))
                    {
                        maxDist = dist;
                        endX = x;
                        endY = y;
                        isBottomEdge = (y == _height - 1);
                    }
                }
            }
            if (isBottomEdge)
                cells[endX, endY].BottomWall = false;
            else
                cells[endX, endY].RightWall = false;
        }
        private void GenerateMaze(int x, int y)
        {
            // 标记当前点已被访问
            cells[x, y].Visited = true;

            var tempData = GetDirections().OrderBy(_ => rand.Next());
            // 随机访问四个方向
            foreach (var dir in tempData)
            {
                int newX = x + dir.Item1, newY = y + dir.Item2;
                if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && !cells[newX, newY].Visited)
                {
                    // 移除两个单元格之间的墙壁
                    if (dir.Item1 == -1)
                    {
                        cells[x, y].LeftWall = false;
                        cells[newX, newY].RightWall = false;
                    }
                    else if (dir.Item1 == 1)
                    {
                        cells[x, y].RightWall = false;
                        cells[newX, newY].LeftWall = false;
                    }
                    else if (dir.Item2 == -1)
                    {
                        cells[x, y].TopWall = false;
                        cells[newX, newY].BottomWall = false;
                    }
                    else if (dir.Item2 == 1)
                    {
                        cells[x, y].BottomWall = false;
                        cells[newX, newY].TopWall = false;
                    }

                    // 递归访问下一个点
                    GenerateMaze(newX, newY);
                }
            }
        }

        #endregion

        #region 普里姆算法
        private void GenerateMaze_Prim()
        {
            // 选择迷宫的一个随机点作为起始点
            int startX = rand.Next(_width);
            int startY = rand.Next(_height);

            cells[startX, startY].Visited = true;

            // 初始化边缘列表，包含起始点的所有邻居
            Queue<MazeCell> frontier = new Queue<MazeCell>();
            AddUnvisitedNeighborsToFrontier(cells[startX, startY], frontier);

            // 使用Prim算法生成迷宫
            while (frontier.Count > 0)
            {
                // 从边缘列表中选择一个单元格，更倾向于选择最早添加的单元格
                var cell = frontier.Dequeue();

                // 找到与这个单元格相邻的已访问的单元格
                var neighbors = GetVisitedNeighbors(cell);

                if (neighbors.Count > 0)
                {
                    // 随机选择一个已访问的邻居
                    var neighbor = neighbors[rand.Next(neighbors.Count)];

                    // 移除两个单元格之间的墙壁
                    if (cell.X > neighbor.Item2.X) // 如果邻居在当前单元格的左侧
                    {
                        cell.LeftWall = false;
                        neighbor.Item2.RightWall = false;
                    }
                    else if (cell.X < neighbor.Item2.X) // 如果邻居在当前单元格的右侧
                    {
                        cell.RightWall = false;
                        neighbor.Item2.LeftWall = false;
                    }
                    else if (cell.Y > neighbor.Item2.Y) // 如果邻居在当前单元格的上方
                    {
                        cell.TopWall = false;
                        neighbor.Item2.BottomWall = false;
                    }
                    else if (cell.Y < neighbor.Item2.Y) // 如果邻居在当前单元格的下方
                    {
                        cell.BottomWall = false;
                        neighbor.Item2.TopWall = false;
                    }

                    // 将这个单元格标记为已访问，并将它的所有未访问的邻居添加到边缘列表中
                    cell.Visited = true;
                    AddUnvisitedNeighborsToFrontier(cell, frontier);
                }
            }
        }

        private void AddUnvisitedNeighborsToFrontier(MazeCell cell, Queue<MazeCell> frontier)
        {
            foreach (var dir in GetDirections())
            {
                int newX = cell.X + dir.Item1, newY = cell.Y + dir.Item2;
                if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && !cells[newX, newY].Visited && !frontier.Contains(cells[newX, newY]))
                    frontier.Enqueue(cells[newX, newY]);
            }
        }


        private List<Tuple<int, MazeCell>> GetVisitedNeighbors(MazeCell cell)
        {
            var visitedNeighbors = new List<Tuple<int, MazeCell>>();

            foreach (var dir in GetDirections())
            {
                int newX = cell.X + dir.Item1, newY = cell.Y + dir.Item2;
                if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && cells[newX, newY].Visited)
                    visitedNeighbors.Add(Tuple.Create(dir.Item1, cells[newX, newY]));
            }

            return visitedNeighbors;
        }

        #endregion

        #region 递归除法算法
        private void GenerateMaze_RecursiveDivision()
        {
            // 初始化迷宫，所有的墙都被移除
            for (int x = 0; x < _width; ++x)
            {
                for (int y = 0; y < _height; ++y)
                {
                    cells[x, y].TopWall = y == 0;
                    cells[x, y].BottomWall = y == _height - 1;
                    cells[x, y].LeftWall = x == 0;
                    cells[x, y].RightWall = x == _width - 1;
                }
            }

            // 递归分割迷宫
            Divide(0, 0, _width - 1, _height - 1);
        }
        private void Divide(int x, int y, int width, int height)
        {
            if (width < 3 || height < 3)
                return;

            bool horizontal = rand.Next(2) == 0;

            if (horizontal)
            {
                // 横向分割
                int splitY = y + 2 + rand.Next(height - 3);
                int holeX = x + rand.Next(width);

                for (int i = x; i < x + width; ++i)
                {
                    if (i != holeX)
                    {
                        cells[i, splitY].BottomWall = true;
                        if (splitY + 1 < _height)
                        {
                            cells[i, splitY + 1].TopWall = true;
                        }
                    }
                }

                Divide(x, y, width, splitY - y);
                Divide(x, splitY + 1, width, y + height - splitY - 1);
            }
            else
            {
                // 纵向分割
                int splitX = x + 2 + rand.Next(width - 3);
                int holeY = y + rand.Next(height);

                for (int i = y; i < y + height; ++i)
                {
                    if (i != holeY)
                    {
                        cells[splitX, i].RightWall = true;
                        if (splitX + 1 < _width)
                        {
                            cells[splitX + 1, i].LeftWall = true;
                        }
                    }
                }

                Divide(x, y, splitX - x, height);
                Divide(splitX + 1, y, x + width - splitX - 1, height);
            }
        }

        #endregion

        #region 时间回溯算法
        private void GenerateMaze_RecursiveBacktracking()
        {
            // 初始化迷宫，所有的墙都存在
            for (int x = 0; x < _width; ++x)
            {
                for (int y = 0; y < _height; ++y)
                {
                    cells[x, y].TopWall = true;
                    cells[x, y].BottomWall = true;
                    cells[x, y].LeftWall = true;
                    cells[x, y].RightWall = true;
                }
            }

            // 递归生成迷宫
            VisitCell(rand.Next(_width), rand.Next(_height));
        }

        private void VisitCell(int x, int y)
        {
            // 标记当前单元格为已访问
            cells[x, y].Visited = true;

            // 对邻居单元格的顺序进行随机排序
            foreach (var dir in GetDirections().OrderBy(d => rand.Next()))
            {
                int nx = x + dir.Item1;
                int ny = y + dir.Item2;

                // 如果邻居单元格在迷宫内并且未被访问过，则移除墙并递归访问邻居单元格
                if (nx >= 0 && ny >= 0 && nx < _width && ny < _height && !cells[nx, ny].Visited)
                {
                    RemoveWall(x, y, dir);
                    RemoveWall(nx, ny, Tuple.Create(-dir.Item1, -dir.Item2));
                    VisitCell(nx, ny);
                }
            }
        }

        private void RemoveWall(int x, int y, Tuple<int, int> direction)
        {
            if (direction.Equals(Tuple.Create(-1, 0))) // Left
            {
                cells[x, y].LeftWall = false;
            }
            else if (direction.Equals(Tuple.Create(1, 0))) // Right
            {
                cells[x, y].RightWall = false;
            }
            else if (direction.Equals(Tuple.Create(0, -1))) // Up
            {
                cells[x, y].TopWall = false;
            }
            else if (direction.Equals(Tuple.Create(0, 1))) // Down
            {
                cells[x, y].BottomWall = false;
            }
        }

        #endregion


        public void CreateMaze(int width, int height, int canvasWidth, int canvasHeight, MazeType mazeType= MazeType.Default,bool createOrUpdate=true)
        {
            mazeBitmap?.Dispose();
            _isMove = true;
            if (createOrUpdate)
            {
                playerPosition = new Point(0, 0); // 初始位置在迷宫的左上角
                stack.Clear();
                _width = width;
                _height = height;
                cells = new MazeCell[width, height];
                //mazeBitmap = new Bitmap(width, height);
                _mazeType = mazeType;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        cells[x, y] = new MazeCell(x, y);
                    }
                }
            }

            GenerateMaze(mazeType);

            // 生成迷宫后，将其绘制到位图上
            mazeBitmap = new Bitmap(canvasWidth, canvasHeight);
            using (var g = Graphics.FromImage(mazeBitmap))
            {
                DrawMaze(g, canvasWidth, canvasHeight);
            }
        }
     
        private void DrawMaze(Graphics g, int canvasWidth, int canvasHeight)
        {
            int tempW = canvasWidth - 1;
            _canvasWidth = tempW;
            _canvasHeight = canvasHeight - 1;
            cellWidth = (float)_canvasWidth / _width;
            cellHeight = (float)_canvasHeight / _height;
            playerRadius = Math.Min(cellWidth, cellHeight) / 4;

            float lineWidth = 1f; // 线条的宽度
            float halfLineWidth = lineWidth / 2f; // 线条宽度的一半

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            //g.SmoothingMode = SmoothingMode.AntiAlias;

            // 先绘制所有的垂直线
            for (int x = 0; x <= _width; x++)
            {
                float left = x * cellWidth;

                for (int y = 0; y < _height; y++)
                {
                    var cell = cells[Math.Min(x, _width - 1), y];

                    float top = y * cellHeight;
                    float bottom = (y + 1) * cellHeight;

                    if ((cell.LeftWall || x == _width) && !(x == _width && y == _height - 1))
                        g.DrawLine(Pens.Black, left, top - halfLineWidth, left, bottom + halfLineWidth);
                }
            }

            // 再绘制所有的水平线
            for (int y = 0; y <= _height; y++)
            {
                float top = y * cellHeight;

                for (int x = 0; x < _width; x++)
                {
                    var cell = cells[x, Math.Min(y, _height - 1)];

                    float left = x * cellWidth;
                    float right = (x + 1) * cellWidth;

                    if ((cell.TopWall || y == _height) && !(x == _width - 1 && y == _height))
                        g.DrawLine(Pens.Black, left - halfLineWidth, top, right + halfLineWidth, top);
                }
            }


        }

        public void Draw(Graphics g, int canvasWidth, int canvasHeight)
        {

            if (cells == null)
                return;

            int tempW = canvasWidth - 1;
            if (tempW != _canvasWidth)
            {
                CreateMaze(_width, _height, canvasWidth, canvasHeight, MazeType,false);
            }

            // 首先，绘制保存的迷宫位图
            g.DrawImage(mazeBitmap, 0, 0, canvasWidth, canvasHeight);



            // 在玩家位置处绘制一个小黑圆
            float playerX = (playerPosition.X + 0.5f) * cellWidth; // 玩家的X坐标
            float playerY = (playerPosition.Y + 0.5f) * cellHeight; // 玩家的Y坐标
            g.FillEllipse(Brushes.Red, playerX - playerRadius, playerY - playerRadius, 2 * playerRadius, 2 * playerRadius);


            // 在出口处写上"出口"
            Font font = new Font("Arial", 16); // 设置字体和大小
            float exitX = (_width - 2f) * cellWidth; // 出口的X坐标
            float exitY = (_height - 1f) * cellHeight; // 出口的Y坐标
            g.DrawString("出口", font, Brushes.Black, exitX, exitY);
        }


        public MoveResult Move(KeyEventArgs e)
        {
            if (cells == null || !_isMove)
                return new MoveResult();

            Point newPosition = playerPosition;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newPosition.Y--;
                    break;
                case Keys.Down:
                    newPosition.Y++;
                    break;
                case Keys.Left:
                    newPosition.X--;
                    break;
                case Keys.Right:
                    newPosition.X++;
                    break;
            }

            return Move(newPosition);
        }
        public MoveResult Move(Point newPosition)
        {
            // 计算小黑点移动前后的矩形区域
            Rectangle oldRect = GetPlayerRect(playerPosition);
            bool status = false;

            if (newPosition.X < 0 || newPosition.Y < 0)
            {
                goto Result;
            }

            int directionX = newPosition.X - playerPosition.X;
            if (directionX != 0)
            {
                if (directionX > 0)
                {
                    if (newPosition.X < _width && !cells[playerPosition.X, playerPosition.Y].RightWall && !cells[newPosition.X, newPosition.Y].LeftWall)
                    {
                        playerPosition = newPosition;
                        status = true;
                        goto Result;
                    }
                }
                else
                {
                    if (newPosition.X >= 0 && !cells[playerPosition.X, playerPosition.Y].LeftWall && !cells[newPosition.X, newPosition.Y].RightWall)
                    {
                        playerPosition = newPosition;
                        status = true;
                        goto Result;
                    }
                }
            }
            int directionY = newPosition.Y - playerPosition.Y;
            if (directionY != 0)
            {
                if (directionY > 0)
                {
                    if (newPosition.Y < _height && !cells[playerPosition.X, playerPosition.Y].BottomWall && !cells[newPosition.X, newPosition.Y].TopWall)
                    {
                        playerPosition = newPosition;
                        status = true;
                        goto Result;
                    }
                }
                else
                {
                    if (newPosition.Y >= 0 && !cells[playerPosition.X, playerPosition.Y].TopWall && !cells[newPosition.X, newPosition.Y].BottomWall)
                    {
                        playerPosition = newPosition;
                        status = true;
                        goto Result;
                    }
                }
            }

        // goto Result;
        Result:

            Rectangle newRect = GetPlayerRect(newPosition);
            bool isWin = playerPosition.X == _width - 1 && playerPosition.Y == _height - 1;
            _isMove = !isWin;
            return new MoveResult
            {
                IsInvalidate = status,
                IsWin = isWin,
                OldRect = oldRect,
                NewRect = newRect
            };
        }
        private Rectangle GetPlayerRect(Point position)
        {
            int x = (int)Math.Round(position.X * cellWidth, 0);
            int y = (int)Math.Round(position.Y * cellHeight, 0);
            return new Rectangle(x, y, (int)Math.Round(cellWidth, 0), (int)Math.Round(cellHeight, 0));
        }
        public Bitmap DrawPath(Bitmap bitmap)
        {
            if (mazeBitmap == null)
                return null;

            var path = FindPath();
            if (bitmap == null)
                bitmap = new Bitmap(_canvasWidth, _canvasHeight);

            // 创建一个Graphics对象
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // 绘制路径
                if (path != null)
                {
                    var pathPen = new Pen(Color.Red, 2);  // 使用红色画笔来绘制路径
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        float x1 = (path[i].X + 0.5f) * cellWidth;
                        float y1 = (path[i].Y + 0.5f) * cellHeight;
                        float x2 = (path[i + 1].X + 0.5f) * cellWidth;
                        float y2 = (path[i + 1].Y + 0.5f) * cellHeight;
                        g.DrawLine(pathPen, x1, y1, x2, y2);
                    }
                }
            }

            return bitmap;
        }
        public List<MazeCell> FindPath()
        {
            var start = cells[0, 0];
            var end = cells[_width - 1, _height - 1];

            var queue = new Queue<MazeCell>();
            var prev = new Dictionary<MazeCell, MazeCell>();

            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();

                if (cell == end)
                {
                    var path = new List<MazeCell>();
                    while (cell != start)
                    {
                        path.Add(cell);
                        cell = prev[cell];
                    }
                    path.Add(start);
                    path.Reverse();
                    return path;
                }

                foreach (var neighbor in GetNeighbors(cell))
                {
                    if (prev.ContainsKey(neighbor))
                        continue;

                    prev[neighbor] = cell;
                    queue.Enqueue(neighbor);
                }
            }

            return null;  // 没有找到路径
        }

        private IEnumerable<MazeCell> GetNeighbors(MazeCell cell)
        {
            var neighbors = new List<MazeCell>();

            if (cell.X > 0 && !cell.LeftWall)
                neighbors.Add(cells[cell.X - 1, cell.Y]);
            if (cell.X < _width - 1 && !cell.RightWall)
                neighbors.Add(cells[cell.X + 1, cell.Y]);
            if (cell.Y > 0 && !cell.TopWall)
                neighbors.Add(cells[cell.X, cell.Y - 1]);
            if (cell.Y < _height - 1 && !cell.BottomWall)
                neighbors.Add(cells[cell.X, cell.Y + 1]);

            return neighbors;
        }
        public void Dispose()
        {
            mazeBitmap?.Dispose();
        }

        ~Maze()
        {
            Dispose();
        }
    }
    public class MazeCell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visited { get; set; }
        public bool TopWall = true, BottomWall = true, LeftWall = true, RightWall = true;

        public MazeCell(int x, int y)
        {
            X = x;
            Y = y;
            Visited = false;
            TopWall = BottomWall = LeftWall = RightWall = true;
        }
    }
    public class MoveResult
    {
        public bool IsInvalidate { get; set; }
        public Rectangle OldRect { get; set; }
        public Rectangle NewRect { get; set; }
        public bool IsWin { get; set; }
    }
    public enum MazeType
    {
        /// <summary>
        /// 默认RecursiveBacktracking
        /// </summary>
        Default,
        /// <summary>
        /// 深度优先搜索算法
        /// </summary>
        DFS,
        /// <summary>
        /// 普里姆算法
        /// </summary>
        Prim,
        /// <summary>
        /// 递归除法算法
        /// </summary>
        RecursiveDivision,
        /// <summary>
        /// 递归回溯算法
        /// </summary>
        RecursiveBacktracking
    }
}
