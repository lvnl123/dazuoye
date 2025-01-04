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
    /// 迷宫类，负责生成和管理迷宫的相关逻辑，包含迷宫生成、玩家位置等功能。
    /// </summary>
    internal class Maze : IDisposable
    {
        private MazeCell[,] cells; // 存储迷宫单元格的二维数组，每个单元格代表迷宫的一部分
        private readonly Stack<MazeCell> stack = new Stack<MazeCell>(); // 使用栈来存储迷宫生成过程中访问的单元格，便于回溯
        private readonly Random rand = new Random(); // 随机数生成器，用于迷宫生成时随机选择路径
        private int _width, _height; // 迷宫的宽度和高度（单元格数量）
        private Bitmap mazeBitmap; // 存储迷宫图像的位图对象，用于绘制和显示迷宫
        private float cellWidth; // 迷宫单元格的宽度，用于绘制时的缩放
        private float cellHeight; // 迷宫单元格的高度，用于绘制时的缩放
        private Point playerPosition; // 玩家在迷宫中的当前位置
        private float playerRadius; // 玩家占用的半径大小，表示玩家的活动范围
        private bool _isMove = true; // 控制是否允许玩家移动的标志
        private int _canvasWidth; // 迷宫绘制区域的宽度（画布宽度）
        private int _canvasHeight; // 迷宫绘制区域的高度（画布高度）
        private MazeType _mazeType = MazeType.Default; // 迷宫的类型，默认为默认类型（可能用于区分不同风格的迷宫）
        public Maze() // 构造函数，初始化迷宫对象
        {

        }
        public Bitmap MazeBitmap => mazeBitmap; // 获取生成的迷宫位图，用于绘制迷宫的图像
        public bool IsMove => _isMove; // 获取是否允许玩家移动的状态，返回 _isMove 的值
        public MazeType MazeType => _mazeType; // 获取当前迷宫的类型，返回 _mazeType 的值
        public int CanvasWidth // 获取或设置迷宫绘制区域的宽度（画布宽度）
        {
            get => _canvasWidth; // 返回当前画布的宽度
            set  // 设置画布的宽度
            {
                _canvasWidth = value; // 更新画布的宽度
            }
        }
        public int CanvasHeight // 获取或设置迷宫绘制区域的高度（画布高度）
        {
            get => _canvasHeight; // 返回当前画布的高度
            set { _canvasHeight = value; }// 更新画布的高度
        }
        private void GenerateMaze(MazeType mazeType)
        {
            // 根据传入的迷宫类型参数mazeType，选择不同的迷宫生成算法
            switch (mazeType)
            {
                case MazeType.Default:
                    GenerateMaze_RecursiveBacktracking(); // 调用递归回溯算法生成迷宫
                    break; // 退出当前case块
                case MazeType.DFS:
                    GenerateMaze_DFS(); // 调用DFS算法生成迷宫
                    break; // 退出当前case块
                case MazeType.Prim:
                    GenerateMaze_Prim(); // 调用Prim算法生成迷宫
                    break; // 退出当前case块
                case MazeType.RecursiveDivision:
                    GenerateMaze_RecursiveDivision(); // 调用递归分割算法生成迷宫
                    break; // 退出当前case块
                case MazeType.RecursiveBacktracking:
                    GenerateMaze_RecursiveBacktracking(); // 调用递归回溯算法生成迷宫
                    break; // 退出当前case块
            }
        }


        /// <summary>
        /// 获取方向
        /// </summary>
        /// <returns></returns>
        // 定义一个私有方法GetDirections，该方法返回一个IEnumerable<Tuple<int, int>>类型的迭代器
        // 这个迭代器包含了一系列的Tuple<int, int>对象，每个对象代表一个方向，其中第一个int表示横坐标的变化量，第二个int表示纵坐标的变化量
        private IEnumerable<Tuple<int, int>> GetDirections()
        {
            // 使用yield return语句返回一个元组，表示向上移动的方向（纵坐标减1，横坐标不变）
            yield return Tuple.Create(-1, 0);
            // 使用yield return语句返回一个元组，表示向下移动的方向（纵坐标加1，横坐标不变）
            yield return Tuple.Create(1, 0);
            // 使用yield return语句返回一个元组，表示向左移动的方向（横坐标减1，纵坐标不变）
            yield return Tuple.Create(0, -1);
            // 使用yield return语句返回一个元组，表示向右移动的方向（横坐标加1，纵坐标不变）
            yield return Tuple.Create(0, 1);
        }


        #region 深度优先搜索算法
        // 定义一个私有方法GenerateMaze_DFS，用于使用深度优先搜索算法生成迷宫
        private void GenerateMaze_DFS()
        {
            // 选择迷宫的左上角的点作为起始点
            int startX = 0;
            int startY = 0;

            // 使用DFS生成迷宫，从起始点开始
            GenerateMaze(startX, startY);

            // 将起始点的左面的墙壁设为入口，允许进入迷宫
            cells[startX, startY].LeftWall = false;

            // 找到迷宫的一个最远的边缘点，将它的边缘的墙壁设为出口，允许离开迷宫
            int maxDist = 0; // 初始化最大距离
            int endX = 0, endY = 0; // 初始化迷宫最远边缘点的坐标
            bool isBottomEdge = false; // 标记是否是最底部的边缘
                                       // 遍历迷宫的每个单元格
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // 计算当前单元格与起始点的曼哈顿距离
                    int dist = Math.Abs(x - startX) + Math.Abs(y - startY);
                    // 如果当前单元格的距离大于最大距离，并且是迷宫的边缘单元格
                    if (dist > maxDist && (x == 0 || y == 0 || x == _width - 1 || y == _height - 1))
                    {
                        maxDist = dist; // 更新最大距离
                        endX = x; // 更新最远边缘点的横坐标
                        endY = y; // 更新最远边缘点的纵坐标
                        isBottomEdge = (y == _height - 1); // 如果是最底部的边缘，则标记为true
                    }
                }
            }
            // 根据最远边缘点的位置，将相应的墙壁设为出口
            if (isBottomEdge)
                cells[endX, endY].BottomWall = false; // 如果是底部边缘，则移除底部墙壁
            else
                cells[endX, endY].RightWall = false; // 否则，移除右侧墙壁
        }

        // 定义一个私有方法GenerateMaze，使用递归深度优先搜索算法生成迷宫
        private void GenerateMaze(int x, int y)
        {
            // 标记当前点已被访问
            cells[x, y].Visited = true;

            // 获取所有可能的移动方向，并随机打乱顺序
            var tempData = GetDirections().OrderBy(_ => rand.Next());
            // 随机访问四个方向
            foreach (var dir in tempData)
            {
                int newX = x + dir.Item1, newY = y + dir.Item2; // 计算新的坐标
                                                                // 检查新坐标是否在迷宫范围内，并且未被访问过
                if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && !cells[newX, newY].Visited)
                {
                    // 移除两个单元格之间的墙壁，以便创建路径
                    if (dir.Item1 == -1)
                    {
                        cells[x, y].LeftWall = false; // 移除当前单元格的左侧墙壁
                        cells[newX, newY].RightWall = false; // 移除新单元格的右侧墙壁
                    }
                    else if (dir.Item1 == 1)
                    {
                        cells[x, y].RightWall = false; // 移除当前单元格的右侧墙壁
                        cells[newX, newY].LeftWall = false; // 移除新单元格的左侧墙壁
                    }
                    else if (dir.Item2 == -1)
                    {
                        cells[x, y].TopWall = false; // 移除当前单元格的顶部墙壁
                        cells[newX, newY].BottomWall = false; // 移除新单元格的底部墙壁
                    }
                    else if (dir.Item2 == 1)
                    {
                        cells[x, y].BottomWall = false; // 移除当前单元格的底部墙壁
                        cells[newX, newY].TopWall = false; // 移除新单元格的顶部墙壁
                    }

                    // 递归访问下一个点，继续生成迷宫
                    GenerateMaze(newX, newY);
                }
            }
        }


        #endregion

        #region 普里姆算法
        // 定义一个私有方法GenerateMaze_Prim，使用普里姆算法生成迷宫
        private void GenerateMaze_Prim()
        {
            // 在迷宫中随机选择一个点作为起始点
            int startX = rand.Next(_width);
            int startY = rand.Next(_height);

            // 标记起始点为已访问
            cells[startX, startY].Visited = true;

            // 初始化边缘列表，将起始点的所有未访问邻居添加到边缘列表中
            Queue<MazeCell> frontier = new Queue<MazeCell>();
            AddUnvisitedNeighborsToFrontier(cells[startX, startY], frontier);

            // 使用普里姆算法生成迷宫，直到边缘列表为空
            while (frontier.Count > 0)
            {
                // 从边缘列表中取出一个单元格，这里使用的是先进先出的队列，所以更倾向于选择最早添加的单元格
                var cell = frontier.Dequeue();

                // 获取与当前单元格相邻的已访问单元格列表
                var neighbors = GetVisitedNeighbors(cell);

                // 如果存在已访问的邻居
                if (neighbors.Count > 0)
                {
                    // 随机选择一个已访问的邻居
                    var neighbor = neighbors[rand.Next(neighbors.Count)];

                    // 移除两个单元格之间的墙壁，创建一条路径
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

                    // 标记当前单元格为已访问，并将其所有未访问的邻居添加到边缘列表中
                    cell.Visited = true;
                    AddUnvisitedNeighborsToFrontier(cell, frontier);
                }
            }
        }

        // 定义一个私有方法AddUnvisitedNeighborsToFrontier，将单元格的未访问邻居添加到边缘列表中
        private void AddUnvisitedNeighborsToFrontier(MazeCell cell, Queue<MazeCell> frontier)
        {
            // 遍历所有可能的移动方向
            foreach (var dir in GetDirections())
            {
                int newX = cell.X + dir.Item1, newY = cell.Y + dir.Item2;
                // 检查新坐标是否在迷宫范围内，并且未被访问过，且不在边缘列表中
                if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && !cells[newX, newY].Visited && !frontier.Contains(cells[newX, newY]))
                    frontier.Enqueue(cells[newX, newY]); // 将未访问的邻居添加到边缘列表中
            }
        }

        // 定义一个私有方法GetVisitedNeighbors，获取单元格的所有已访问邻居
        private List<Tuple<int, MazeCell>> GetVisitedNeighbors(MazeCell cell)
        {
            var visitedNeighbors = new List<Tuple<int, MazeCell>>(); // 初始化已访问邻居列表

            // 遍历所有可能的移动方向
            foreach (var dir in GetDirections())
            {
                int newX = cell.X + dir.Item1, newY = cell.Y + dir.Item2;
                // 检查新坐标是否在迷宫范围内，并且已访问
                if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && cells[newX, newY].Visited)
                    visitedNeighbors.Add(Tuple.Create(dir.Item1, cells[newX, newY])); // 将已访问的邻居添加到列表中
            }

            return visitedNeighbors; // 返回已访问邻居列表
        }


        #endregion

        #region 递归除法算法
        // 定义一个私有方法GenerateMaze_RecursiveDivision，使用递归除法算法生成迷宫
        private void GenerateMaze_RecursiveDivision()
        {
            // 初始化迷宫，设置所有边缘墙壁，内部墙壁默认移除
            for (int x = 0; x < _width; ++x)
            {
                for (int y = 0; y < _height; ++y)
                {
                    // 设置上边缘墙壁
                    cells[x, y].TopWall = y == 0;
                    // 设置下边缘墙壁
                    cells[x, y].BottomWall = y == _height - 1;
                    // 设置左边缘墙壁
                    cells[x, y].LeftWall = x == 0;
                    // 设置右边缘墙壁
                    cells[x, y].RightWall = x == _width - 1;
                }
            }

            // 开始递归分割迷宫，从整个迷宫区域开始
            Divide(0, 0, _width - 1, _height - 1);
        }

        // 定义一个私有方法Divide，递归地分割迷宫区域
        private void Divide(int x, int y, int width, int height)
        {
            // 如果区域太小，无法进一步分割，则返回
            if (width < 3 || height < 3)
                return;

            // 随机选择分割方向，true为横向分割，false为纵向分割
            bool horizontal = rand.Next(2) == 0;

            if (horizontal)
            {
                // 横向分割迷宫区域
                int splitY = y + 2 + rand.Next(height - 3); // 随机选择分割线的位置
                int holeX = x + rand.Next(width); // 随机选择一个洞的位置

                // 在分割线上设置墙壁，除了洞的位置
                for (int i = x; i < x + width; ++i)
                {
                    if (i != holeX)
                    {
                        cells[i, splitY].BottomWall = true; // 设置分割线下方的墙壁
                        if (splitY + 1 < _height)
                        {
                            cells[i, splitY + 1].TopWall = true; // 设置分割线上方的墙壁
                        }
                    }
                }

                // 递归分割上方区域
                Divide(x, y, width, splitY - y);
                // 递归分割下方区域
                Divide(x, splitY + 1, width, y + height - splitY - 1);
            }
            else
            {
                // 纵向分割迷宫区域
                int splitX = x + 2 + rand.Next(width - 3); // 随机选择分割线的位置
                int holeY = y + rand.Next(height); // 随机选择一个洞的位置

                // 在分割线上设置墙壁，除了洞的位置
                for (int i = y; i < y + height; ++i)
                {
                    if (i != holeY)
                    {
                        cells[splitX, i].RightWall = true; // 设置分割线右侧的墙壁
                        if (splitX + 1 < _width)
                        {
                            cells[splitX + 1, i].LeftWall = true; // 设置分割线左侧的墙壁
                        }
                    }
                }

                // 递归分割左侧区域
                Divide(x, y, splitX - x, height);
                // 递归分割右侧区域
                Divide(splitX + 1, y, x + width - splitX - 1, height);
            }
        }

        #endregion

        #region 时间回溯算法
        // 定义一个私有方法GenerateMaze_RecursiveBacktracking，用于实现递归回溯算法生成迷宫
        private void GenerateMaze_RecursiveBacktracking()
        {
            // 初始化迷宫，设置所有单元格的四周墙壁都存在
            // 遍历迷宫的每个单元格
            for (int x = 0; x < _width; ++x) // 外层循环，遍历迷宫的宽度
            {
                for (int y = 0; y < _height; ++y) // 内层循环，遍历迷宫的高度
                {
                    // 为每个单元格设置初始墙壁状态，确保每个单元格都被墙壁包围
                    cells[x, y].TopWall = true; // 上墙壁存在
                    cells[x, y].BottomWall = true; // 下墙壁存在
                    cells[x, y].LeftWall = true; // 左墙壁存在
                    cells[x, y].RightWall = true; // 右墙壁存在
                }
            }

            // 随机选择迷宫中的一个起始点，调用VisitCell方法开始递归生成迷宫
            // rand是随机数生成器，Next方法返回一个指定范围内的随机整数
            // 随机选择起始单元格的x和y坐标
            VisitCell(rand.Next(_width), rand.Next(_height));
        }

        // 定义一个私有方法VisitCell，用于递归访问单元格并生成迷宫路径
        private void VisitCell(int x, int y)
        {
            // 标记当前单元格为已访问，防止在生成过程中重复访问
            cells[x, y].Visited = true;

            // 获取所有可能的移动方向，并使用随机顺序来选择方向，增加迷宫的随机性
            // GetDirections返回所有可能的移动方向，OrderBy和rand.Next()用于随机排序方向
            foreach (var dir in GetDirections().OrderBy(d => rand.Next()))
            {
                // 计算移动方向后的邻居单元格坐标
                int nx = x + dir.Item1; // 邻居单元格的x坐标
                int ny = y + dir.Item2; // 邻居单元格的y坐标

                // 检查邻居单元格是否在迷宫边界内，并且尚未被访问
                if (nx >= 0 && ny >= 0 && nx < _width && ny < _height && !cells[nx, ny].Visited)
                {
                    // 移除当前单元格与邻居单元格之间的墙壁，以便创建一条路径
                    RemoveWall(x, y, dir); // 移除当前单元格的墙壁
                    RemoveWall(nx, ny, Tuple.Create(-dir.Item1, -dir.Item2)); // 移除邻居单元格相对的墙壁

                    // 递归调用VisitCell，从邻居单元格继续生成迷宫路径
                    VisitCell(nx, ny);
                }
            }
        }

        // 定义一个私有方法RemoveWall，用于移除两个单元格之间的墙壁
        private void RemoveWall(int x, int y, Tuple<int, int> direction)
        {
            // 根据传入的方向参数，移除相应的墙壁
            if (direction.Equals(Tuple.Create(-1, 0))) // 如果方向是向左
            {
                cells[x, y].LeftWall = false; // 移除当前单元格的左墙壁
            }
            else if (direction.Equals(Tuple.Create(1, 0))) // 如果方向是向右
            {
                cells[x, y].RightWall = false; // 移除当前单元格的右墙壁
            }
            else if (direction.Equals(Tuple.Create(0, -1))) // 如果方向是向上
            {
                cells[x, y].TopWall = false; // 移除当前单元格的上墙壁
            }
            else if (direction.Equals(Tuple.Create(0, 1))) // 如果方向是向下
            {
                cells[x, y].BottomWall = false; // 移除当前单元格的下墙壁
            }
        }


        #endregion


        // 定义一个公共方法CreateMaze，用于创建或更新迷宫
        public void CreateMaze(int width, int height, int canvasWidth, int canvasHeight, MazeType mazeType = MazeType.Default, bool createOrUpdate = true)
        {
            // 如果mazeBitmap对象不为null，则释放其资源
            mazeBitmap?.Dispose();
            // 设置_isMove标志为true，表示迷宫中的移动是允许的
            _isMove = true;

            // 检查是否需要创建或更新迷宫
            if (createOrUpdate)
            {
                // 设置玩家的初始位置在迷宫的左上角
                playerPosition = new Point(0, 0);
                // 清空历史记录栈
                stack.Clear();
                // 设置迷宫的宽度和高度
                _width = width;
                _height = height;
                // 初始化迷宫单元格数组
                cells = new MazeCell[width, height];
                // 注释掉的代码：创建一个新的位图对象，用于绘制迷宫
                // mazeBitmap = new Bitmap(width, height);
                // 设置迷宫类型
                _mazeType = mazeType;

                // 遍历迷宫的每个单元格，初始化它们
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // 创建一个新的MazeCell对象，并将其放置在单元格数组中
                        cells[x, y] = new MazeCell(x, y);
                    }
                }
            }

            // 根据传入的mazeType参数调用相应的迷宫生成方法
            GenerateMaze(mazeType);

            // 生成迷宫后，将其绘制到位图上
            mazeBitmap = new Bitmap(canvasWidth, canvasHeight);
            using (var g = Graphics.FromImage(mazeBitmap))
            {
                // 调用DrawMaze方法，将迷宫绘制到Graphics对象g上，该对象关联到mazeBitmap位图
                DrawMaze(g, canvasWidth, canvasHeight);
            }
            // using语句块结束，Graphics对象g将被自动释放资源
        }

        // 定义一个私有方法DrawMaze，用于将迷宫绘制到Graphics对象上
        private void DrawMaze(Graphics g, int canvasWidth, int canvasHeight)
        {
            // 调整画布尺寸，减去1以避免超出边界
            int tempW = canvasWidth - 1;
            _canvasWidth = tempW;
            _canvasHeight = canvasHeight - 1;

            // 计算每个单元格的宽度和高度
            cellWidth = (float)_canvasWidth / _width;
            cellHeight = (float)_canvasHeight / _height;

            // 计算玩家半径，取单元格宽度和高度中的最小值除以4
            playerRadius = Math.Min(cellWidth, cellHeight) / 4;

            // 设置线条宽度
            float lineWidth = 1f; // 线条的宽度
            float halfLineWidth = lineWidth / 2f; // 线条宽度的一半

            // 设置Graphics对象的绘制质量
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            // g.SmoothingMode = SmoothingMode.AntiAlias; // 注释掉的代码，可能用于开启抗锯齿

            // 绘制所有的垂直线
            for (int x = 0; x <= _width; x++) // 遍历所有列
            {
                float left = x * cellWidth; // 计算垂直线的左侧位置

                for (int y = 0; y < _height; y++) // 遍历所有行
                {
                    // 获取当前单元格，对于最右侧的单元格，使用_width - 1作为索引
                    var cell = cells[Math.Min(x, _width - 1), y];

                    float top = y * cellHeight; // 计算垂直线的顶部位置
                    float bottom = (y + 1) * cellHeight; // 计算垂直线的底部位置

                    // 如果单元格有左侧墙壁，或者是最右侧的单元格（除了最底部的单元格），则绘制垂直线
                    if ((cell.LeftWall || x == _width) && !(x == _width && y == _height - 1))
                        g.DrawLine(Pens.Black, left, top - halfLineWidth, left, bottom + halfLineWidth);
                }
            }

            // 绘制所有的水平线
            for (int y = 0; y <= _height; y++) // 遍历所有行
            {
                float top = y * cellHeight; // 计算水平线的顶部位置

                for (int x = 0; x < _width; x++) // 遍历所有列
                {
                    // 获取当前单元格，对于最底部的单元格，使用_height - 1作为索引
                    var cell = cells[x, Math.Min(y, _height - 1)];

                    float left = x * cellWidth; // 计算水平线的左侧位置
                    float right = (x + 1) * cellWidth; // 计算水平线的右侧位置

                    // 如果单元格有顶部墙壁，或者是最底部的单元格（除了最右侧的单元格），则绘制水平线
                    if ((cell.TopWall || y == _height) && !(x == _width - 1 && y == _height))
                        g.DrawLine(Pens.Black, left - halfLineWidth, top, right + halfLineWidth, top);
                }
            }


        }

        // 定义一个公共方法Draw，用于在指定的Graphics对象上绘制迷宫及其相关元素
        public void Draw(Graphics g, int canvasWidth, int canvasHeight)
        {
            // 如果cells数组为null，则直接返回，因为没有迷宫可以绘制
            if (cells == null)
                return;

            // 调整画布宽度，减去1以避免绘制时超出边界
            int tempW = canvasWidth - 1;

            // 如果调整后的画布宽度与当前的画布宽度不同，则重新创建迷宫
            if (tempW != _canvasWidth)
            {
                // 调用CreateMaze方法重新创建迷宫，不更新迷宫类型，且createOrUpdate参数为false
                CreateMaze(_width, _height, canvasWidth, canvasHeight, MazeType, false);
            }

            // 绘制保存的迷宫位图到Graphics对象上，从(0, 0)开始，大小为canvasWidth x canvasHeight
            g.DrawImage(mazeBitmap, 0, 0, canvasWidth, canvasHeight);

            // 在玩家位置处绘制一个小黑圆，表示玩家
            // 计算玩家在画布上的X坐标
            float playerX = (playerPosition.X + 0.5f) * cellWidth;
            // 计算玩家在画布上的Y坐标
            float playerY = (playerPosition.Y + 0.5f) * cellHeight;
            // 使用FillEllipse方法绘制一个红色圆形来表示玩家，半径为playerRadius
            g.FillEllipse(Brushes.Red, playerX - playerRadius, playerY - playerRadius, 2 * playerRadius, 2 * playerRadius);

            // 在出口处写上"出口"文字
            // 设置字体样式和大小
            Font font = new Font("Arial", 16);
            // 计算出口在画布上的X坐标
            float exitX = (_width - 2f) * cellWidth;
            // 计算出口在画布上的Y坐标
            float exitY = (_height - 1f) * cellHeight;
            // 使用DrawString方法在出口位置绘制"出口"文字
            g.DrawString("出口", font, Brushes.Black, exitX, exitY);
        }


        // 定义一个公共方法Move，用于处理玩家移动，并返回移动结果
        public MoveResult Move(KeyEventArgs e)
        {
            // 如果cells数组为null或者_isMove为false，表示无法移动，直接返回一个新的MoveResult实例
            if (cells == null || !_isMove)
                return new MoveResult();

            // 初始化新位置为玩家的当前位置
            Point newPosition = playerPosition;

            // 根据按键事件e.KeyCode判断玩家移动的方向
            switch (e.KeyCode)
            {
                case Keys.Up:    // 如果按键是向上箭头
                    newPosition.Y--; // 玩家的Y坐标减1，向上移动
                    break;
                case Keys.Down:  // 如果按键是向下箭头
                    newPosition.Y++; // 玩家的Y坐标加1，向下移动
                    break;
                case Keys.Left:  // 如果按键是向左箭头
                    newPosition.X--; // 玩家的X坐标减1，向左移动
                    break;
                case Keys.Right: // 如果按键是向右箭头
                    newPosition.X++; // 玩家的X坐标加1，向右移动
                    break;
            }

            // 调用Move方法尝试移动到新位置，并返回移动结果
            return Move(newPosition);
        }
        // 定义一个公共方法Move，用于尝试将玩家移动到新位置，并返回移动结果
        public MoveResult Move(Point newPosition)
        {
            // 计算小黑点移动前后的矩形区域
            Rectangle oldRect = GetPlayerRect(playerPosition);
            bool status = false; // 用于标记移动是否成功

            // 如果新位置超出迷宫边界，则直接跳转到结果处理
            if (newPosition.X < 0 || newPosition.Y < 0)
            {
                goto Result;
            }

            // 计算玩家在X轴的移动方向
            int directionX = newPosition.X - playerPosition.X;
            if (directionX != 0) // 如果玩家在X轴上有移动
            {
                if (directionX > 0) // 玩家向右移动
                {
                    // 检查新位置是否在迷宫内，并且没有右侧墙壁和目标单元格的左侧墙壁
                    if (newPosition.X < _width && !cells[playerPosition.X, playerPosition.Y].RightWall && !cells[newPosition.X, newPosition.Y].LeftWall)
                    {
                        playerPosition = newPosition; // 更新玩家位置
                        status = true; // 标记移动成功
                        goto Result; // 跳转到结果处理
                    }
                }
                else // 玩家向左移动
                {
                    // 检查新位置是否在迷宫内，并且没有左侧墙壁和目标单元格的右侧墙壁
                    if (newPosition.X >= 0 && !cells[playerPosition.X, playerPosition.Y].LeftWall && !cells[newPosition.X, newPosition.Y].RightWall)
                    {
                        playerPosition = newPosition; // 更新玩家位置
                        status = true; // 标记移动成功
                        goto Result; // 跳转到结果处理
                    }
                }
            }

            // 计算玩家在Y轴的移动方向
            int directionY = newPosition.Y - playerPosition.Y;
            if (directionY != 0) // 如果玩家在Y轴上有移动
            {
                if (directionY > 0) // 玩家向下移动
                {
                    // 检查新位置是否在迷宫内，并且没有底部墙壁和目标单元格的顶部墙壁
                    if (newPosition.Y < _height && !cells[playerPosition.X, playerPosition.Y].BottomWall && !cells[newPosition.X, newPosition.Y].TopWall)
                    {
                        playerPosition = newPosition; // 更新玩家位置
                        status = true; // 标记移动成功
                        goto Result; // 跳转到结果处理
                    }
                }
                else // 玩家向上移动
                {
                    // 检查新位置是否在迷宫内，并且没有顶部墙壁和目标单元格的底部墙壁
                    if (newPosition.Y >= 0 && !cells[playerPosition.X, playerPosition.Y].TopWall && !cells[newPosition.X, newPosition.Y].BottomWall)
                    {
                        playerPosition = newPosition; // 更新玩家位置
                        status = true; // 标记移动成功
                        goto Result; // 跳转到结果处理
                    }
                }
            }

            // 使用goto语句跳转到结果处理部分
            goto Result;

        // 结果处理标签
        Result:
            // 获取玩家在新位置上的矩形区域
            Rectangle newRect = GetPlayerRect(newPosition);
            // 判断玩家是否到达迷宫的出口（通常位于迷宫的最右下角）
            bool isWin = playerPosition.X == _width - 1 && playerPosition.Y == _height - 1;
            // 如果玩家赢了，则不允许继续移动（_isMove设为false）
            _isMove = !isWin;
            // 返回移动结果，包含是否需要重绘界面（IsInvalidate），是否赢得游戏（IsWin），移动前的矩形区域（OldRect）和移动后的矩形区域（NewRect）
            return new MoveResult
            {
                IsInvalidate = status, // 表示移动是否成功，如果成功则需要重绘界面
                IsWin = isWin,         // 表示玩家是否赢得了游戏
                OldRect = oldRect,     // 玩家移动前的矩形区域
                NewRect = newRect      // 玩家移动后的矩形区域
            };
        }
        // 定义一个私有方法GetPlayerRect，用于根据玩家的位置计算其所在的矩形区域
        private Rectangle GetPlayerRect(Point position)
        {
            // 计算玩家位置在画布上的X坐标，四舍五入到最接近的整数
            int x = (int)Math.Round(position.X * cellWidth, 0);
            // 计算玩家位置在画布上的Y坐标，四舍五入到最接近的整数
            int y = (int)Math.Round(position.Y * cellHeight, 0);
            // 创建一个新的Rectangle对象，表示玩家的矩形区域
            // 宽度和高度同样四舍五入到最接近的整数
            return new Rectangle(x, y, (int)Math.Round(cellWidth, 0), (int)Math.Round(cellHeight, 0));
        }
        // 定义一个公共方法DrawPath，用于在位图上绘制迷宫的路径
        public Bitmap DrawPath(Bitmap bitmap)
        {
            // 如果mazeBitmap为null，表示没有迷宫位图可以绘制路径，直接返回null
            if (mazeBitmap == null)
                return null;

            // 寻找迷宫的路径
            var path = FindPath();
            // 如果传入的bitmap为null，则创建一个新的位图，大小与画布相同
            if (bitmap == null)
                bitmap = new Bitmap(_canvasWidth, _canvasHeight);

            // 创建一个Graphics对象，用于在位图上绘制
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // 如果找到了路径，则开始绘制
                if (path != null)
                {
                    // 创建一个红色画笔，线宽为2像素
                    var pathPen = new Pen(Color.Red, 2);
                    // 遍历路径中的每个点，除了最后一个点
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        // 计算路径上当前点和下一个点的像素坐标
                        float x1 = (path[i].X + 0.5f) * cellWidth;
                        float y1 = (path[i].Y + 0.5f) * cellHeight;
                        float x2 = (path[i + 1].X + 0.5f) * cellWidth;
                        float y2 = (path[i + 1].Y + 0.5f) * cellHeight;
                        // 使用画笔在Graphics对象上绘制一条线段，连接当前点和下一个点
                        g.DrawLine(pathPen, x1, y1, x2, y2);
                    }
                }
            }
            // 返回绘制了路径的位图
            return bitmap;
        }
        // 定义一个公共方法FindPath，用于找到从迷宫起点到终点的路径
        public List<MazeCell> FindPath()
        {
            // 获取迷宫的起点和终点
            var start = cells[0, 0];
            var end = cells[_width - 1, _height - 1];

            // 创建一个队列用于广度优先搜索（BFS）
            var queue = new Queue<MazeCell>();
            // 创建一个字典用于记录每个单元格的前一个单元格
            var prev = new Dictionary<MazeCell, MazeCell>();

            // 将起点加入队列
            queue.Enqueue(start);

            // 当队列不为空时，继续搜索
            while (queue.Count > 0)
            {
                // 从队列中取出一个单元格
                var cell = queue.Dequeue();

                // 如果找到了终点
                if (cell == end)
                {
                    // 创建一个列表用于存储路径
                    var path = new List<MazeCell>();
                    // 从终点开始，通过prev字典回溯到起点
                    while (cell != start)
                    {
                        path.Add(cell);
                        cell = prev[cell];
                    }
                    // 将起点加入路径
                    path.Add(start);
                    // 反转路径，使其从起点到终点
                    path.Reverse();
                    // 返回找到的路径
                    return path;
                }

                // 遍历当前单元格的所有相邻单元格
                foreach (var neighbor in GetNeighbors(cell))
                {
                    // 如果相邻单元格已经在prev字典中，说明它已经被访问过，跳过这个单元格
                    if (prev.ContainsKey(neighbor))
                        continue;

                    // 记录当前单元格是相邻单元格的前一个单元格
                    prev[neighbor] = cell;
                    // 将相邻单元格加入队列，以便后续访问
                    queue.Enqueue(neighbor);
                }
            }
            // 当队列为空时，广度优先搜索结束，如果没有找到终点，则返回null
            return null;  // 没有找到路径
        }

        // 定义一个私有方法GetNeighbors，用于获取一个单元格的所有相邻单元格
        private IEnumerable<MazeCell> GetNeighbors(MazeCell cell)
        {
            // 创建一个列表来存储相邻单元格
            var neighbors = new List<MazeCell>();

            // 检查左侧是否有相邻单元格，并且没有左墙
            if (cell.X > 0 && !cell.LeftWall)
                neighbors.Add(cells[cell.X - 1, cell.Y]); // 添加左侧单元格

            // 检查右侧是否有相邻单元格，并且没有右墙
            if (cell.X < _width - 1 && !cell.RightWall)
                neighbors.Add(cells[cell.X + 1, cell.Y]); // 添加右侧单元格

            // 检查上方是否有相邻单元格，并且没有上墙
            if (cell.Y > 0 && !cell.TopWall)
                neighbors.Add(cells[cell.X, cell.Y - 1]); // 添加上方单元格

            // 检查下方是否有相邻单元格，并且没有下墙
            if (cell.Y < _height - 1 && !cell.BottomWall)
                neighbors.Add(cells[cell.X, cell.Y + 1]); // 添加下方单元格

            // 返回相邻单元格的列表
            return neighbors;
        }
        // 定义一个公共方法Dispose，用于释放与对象关联的资源
        public void Dispose()
        {
            // 安全地释放mazeBitmap资源，如果mazeBitmap不为null
            // 使用null条件运算符(?.)来避免在mazeBitmap为null时调用Dispose方法引发异常
            mazeBitmap?.Dispose();
        }

        ~Maze()
        {
            Dispose();// 在对象被垃圾回收器回收之前，调用Dispose方法来释放资源
        }
    }
    // MazeCell类定义了迷宫中的单个单元格
    public class MazeCell
    {
        // X和Y属性表示单元格在迷宫中的横纵坐标
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visited { get; set; }// Visited属性表示这个单元格是否已经被访问过
        // TopWall, BottomWall, LeftWall, RightWall属性表示单元格四周的墙壁是否存在
        // 默认情况下，所有墙壁都是存在的
        public bool TopWall = true, BottomWall = true, LeftWall = true, RightWall = true;

        public MazeCell(int x, int y)// MazeCell的构造方法，用于创建一个新的单元格实例
        {
            // 初始化单元格的坐标
            X = x;
            Y = y;
            Visited = false;// 初始化单元格为未访问状态
            TopWall = BottomWall = LeftWall = RightWall = true;// 初始化单元格四周的墙壁，默认所有墙壁都存在
        }
    }
    // MoveResult类用于封装移动操作的结果信息
    public class MoveResult
    {
        // IsInvalidate属性表示移动操作是否无效
        public bool IsInvalidate { get; set; }

        // OldRect属性表示移动前的矩形区域
        public Rectangle OldRect { get; set; }

        // NewRect属性表示移动后的矩形区域
        public Rectangle NewRect { get; set; }

        // IsWin属性表示移动后是否达到了胜利条件
        public bool IsWin { get; set; }
    }
    // MazeType枚举用于定义迷宫生成算法的类型
    public enum MazeType
    {
        // 默认的迷宫生成算法是递归回溯算法
        // 这是一种高效的迷宫生成算法，它从一个起点开始，随机选择一个方向前进，
        // 如果遇到死胡同，则回溯到上一个单元格并尝试其他方向。
        Default,

        // 深度优先搜索算法（DFS）
        // 这种算法通过深度优先遍历的方式生成迷宫。从一个起点开始，随机选择一个方向前进，
        // 当到达一个单元格时，将其标记为已访问，并继续深入直到没有未访问的相邻单元格，
        // 然后回溯到上一个单元格并继续探索其他方向。
        DFS,

        // 普里姆算法（Prim's Algorithm）
        // 普里姆算法通常用于生成最小生成树，但在迷宫生成中，它可以用来生成具有较少死胡同的迷宫。
        // 算法从一个随机单元格开始，将其添加到迷宫中，并不断添加与其相邻的单元格，
        // 直到所有单元格都被添加到迷宫中。每次选择添加的单元格都是随机选择的，
        // 并且在添加时，会随机选择一个方向打破墙壁。
        Prim,

        // 递归除法算法（Recursive Division）
        // 递归除法算法通过递归地将迷宫区域划分成更小的区域来生成迷宫。
        // 算法首先选择一个方向来划分迷宫，然后在划分出的每个子区域中继续进行划分，
        // 直到达到预设的最小区域大小。在划分过程中，会在适当的位置放置墙壁。
        RecursiveDivision,

        // 递归回溯算法（Recursive Backtracking）
        // 递归回溯算法是一种基于递归的迷宫生成算法。它从一个起点开始，随机选择一个方向前进，
        // 如果前进的方向没有墙壁，则继续前进并标记路径，如果遇到死胡同或者所有方向都已探索，
        // 则回溯到上一个单元格并继续探索其他方向。这个过程一直持续到所有单元格都被探索。
        RecursiveBacktracking
    }
}
