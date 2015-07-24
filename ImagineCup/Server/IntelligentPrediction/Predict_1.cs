using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncSocketServer.IntelligentPrediction
{
    public class BstNode
    {
        public long Key; // 关键字(键值) 
        public int CarNumbers; // 路段上的车车
        public long LatestTime; // 最新的更新时间
        public int MaxCarNumbers; // 路段记录中最多的车辆数
        public bool isFull;

        public int blockCarNumbers;
        public double blockSpeed;
        public int isBlocked;


        public BstNode Left; // 左孩子
        public BstNode Right; // 右孩子
        public BstNode Parent; // 父结点

        public BstNode(int value, int c, long la, int max, int blockcarnum, double blockspeed, int isblocked, BstNode p,
            BstNode l, BstNode r,bool isfull)
        {
            Key = value;
            CarNumbers = c;
            LatestTime = la;
            MaxCarNumbers = max;
            blockCarNumbers = blockcarnum;
            blockSpeed = blockspeed;
            isBlocked = isblocked;
            Parent = p;
            Left = l;
            Right = r;
            isFull = isfull;

        }

    }



    public class BsTree
    {
        private BstNode _mRoot; // 根结点
        private bool _isFull;
        private int AllLevel;
        private Dictionary<int, int> _carUpdate;
        private Dictionary<int, int> _carIt;
        private Dictionary<string, int> _roadUpdate;
        private Dictionary<string, int> _roadIt;
        private Dictionary<int, double> carSpeed;
        private const int MaxRoadNum = 100000;
        private const int Maxn = 10000007;
        private const double InUsePercentage = 0.8;



        /// <summary>
        /// 构造函数
        /// </summary>
        public BsTree()
        {
            _mRoot = null;
            _carUpdate = new Dictionary<int, int>();
            _carIt = new Dictionary<int, int>();
            _roadUpdate = new Dictionary<string, int>();
            _roadIt = new Dictionary<string, int>();
            carSpeed = new Dictionary<int, double>();

        }

        /// <summary>
        /// 计算系统时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private long ConvertDateTimeInt(DateTime time)
        {
            long intResult;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime());
            intResult = (long) (time - startTime).TotalSeconds;
            return intResult;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roadId"></param>
        /// <returns></returns>
        private int Hash(string roadId)
        {
            int res = 0;
            char[] road = roadId.ToCharArray();
            for (int i = 0; i < roadId.Length; ++i)
            {
                res = res*23333 + road[i];
                res %= Maxn;
            }

            return res;

        }

        /// <summary>
        /// 查找最小结点：返回tree为根结点的二叉树的最小结点
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private BstNode Minimum(BstNode tree)
        {
            if (tree == null)
                return null;
            while (tree.Left != null)
            {
                tree = tree.Left;
            }
            return tree;
        }

        private long Munimum()
        {
            BstNode p = Minimum(_mRoot);
            if (p != null)
                return p.Key;
            return 0;
        }

        /// <summary>
        /// (递归实现)查找"二叉树x"中键值为key的节点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private BstNode Search(ref BstNode x, long key)
        {
            if (x == null || x.Key == key)
                return x;
            if (key < x.Key)
                return Search(ref x.Left, key);
            return Search(ref x.Right, key);
        }

        private BstNode Search(int key)
        {
            return Search(ref _mRoot, key);
        }

        /// <summary>
        /// (递归实现)查找"二叉树x"中latestTime为key的节点
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private BstNode SearchTime(BstNode x, long time)
        {
            if (x == null || x.LatestTime == time)
                return SearchTime(x.Left, time);

            if (x.LatestTime == time)
            {
                return x;
            }

            if (!(x == null || x.LatestTime == time))
            {
                return SearchTime(x.Right, time);
            }

            return x;

        }

        private BstNode SearchTime(long time)
        {
            return Search(ref _mRoot, time);
        }

        private BstNode Successor(BstNode x)
        {
            if (x.Right != null)
                return Minimum(x.Right);
            BstNode y = x.Parent;
            while ((y != null) && (x == y.Right))
            {
                x = y;
                y = y.Parent;
            }

            return y;
        }

        /// <summary>
        /// 将结点插入到二叉树中
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="z"></param>
        private void Insert(ref BstNode tree, BstNode z)
        {
            BstNode x = tree;
            BstNode y = null;

            while (x != null)
            {
                y = x;
                if (z.Key < x.Key)
                    x = x.Left;
                else
                    x = x.Right;

            }
            z.Parent = y;
            if (y == null)
                tree = z;
            else if (z.Key < y.Key)
                y.Left = z;
            else
                y.Right = z;

        }

        /// <summary>
        /// 将结点(key为节点键值)插入到二叉树中
        /// </summary>
        /// <param name="key"></param>
        private void Insert(int key,double speed)
        {
            long timeNow = ConvertDateTimeInt(DateTime.Now);
            BstNode z = new BstNode(key, 1, timeNow, 0, 0, speed, 0, null, null, null,true);
            Insert(ref _mRoot, z);
            if (!_isFull)
            {
                if (AllLevel++ > MaxRoadNum)
                {
                    _isFull = true;
                }
            }
        }

        /// <summary>
        /// 删除结点(z)，并返回被删除的结点
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private void Remove(ref BstNode tree, BstNode z)
        {
            BstNode x;
            BstNode y;

            if ((z.Left == null) || (z.Right == null))
                y = z;
            else
                y = Successor(z);

            if (y.Left != null)
                x = y.Left;
            else
                x = y.Right;

            if (x != null)
                x.Parent = y.Parent;

            if (y.Parent == null)
                tree = x;
            else if (y == y.Parent.Left)
                y.Parent.Left = x;
            else
                y.Parent.Right = x;

            if (y != z)
                z.Key = y.Key;


        }

        private void Remove(long key)
        {
            BstNode z;

            if ((z = Search(ref _mRoot, key)) != null)
                Remove(ref _mRoot, z);

        }

        private long Maximum(BstNode tree)
        {
            if (tree == null)
                return 0;
            long oldTime = Math.Min(tree.LatestTime, Math.Max(Maximum(tree.Left), Maximum(tree.Right)));
            return oldTime;
        }

        private void Maximum()
        {
            long oldTime = Maximum(_mRoot);
            Remove(SearchTime(oldTime).Key);
        }

        /// <summary>
        /// 销毁二叉树
        /// </summary>
        /// <param name="tree"></param>
        private void Destroy(BstNode tree)
        {
            if (tree == null)
                return;

            if (tree.Left != null)
                Destroy(tree.Left);
            if (tree.Right != null)
                Destroy(tree.Right);

        }

        private void Destroy()
        {
            Destroy(_mRoot);
        }



        /// <summary>
        /// 道路信息更新函数
        /// </summary>
        /// <param name="carId"></param>
        /// <param name="roadId"></param>
        /// <returns></returns>
        private int CarUp(int carId, int roadId)
        {
            int ret = 0;
            if (!_carUpdate.ContainsKey(carId))
            {
                _carUpdate.Add(carId, roadId);
            }

            else
            {
                int old = _carUpdate[carId];
                if (old != roadId)
                {
                    _carUpdate[carId] = roadId;
                    Search(old).CarNumbers--;
                    ret = 1;
                }
                else
                {
                    ret = 2;
                }

            }
            return ret;
        }

        /// <summary>
        /// 道路信息更新函数
        /// </summary>
        /// <param name="raodName"></param>
        /// <param name="roadId"></param>
        private void RoadUp(string raodName, int roadId)
        {
            if (!_roadUpdate.ContainsKey(raodName))
                _roadUpdate.Add(raodName, roadId);

        }

        /// <summary>
        /// 唯一调用函数，更新二叉树节点用
        /// </summary>
        /// <param name="carId"></param>
        public void DropCar(int carId)
        {
            int roadId = 0;
            if (_carUpdate.ContainsKey(carId))
            {
                roadId = _carUpdate[carId];
                _carUpdate.Remove(carId);
                Search(roadId).CarNumbers--;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roadId"></param>
        /// <param name="roadName"></param>
        /// <param name="carId"></param>
        /// <returns></returns>
        public int InsertIn(string roadId, string roadName, int carId, double speed)
        {
            // ReSharper disable once InconsistentNaming
            int roadID = Hash(roadId);
            int ret = CarUp(carId, roadID);
            RoadUp(roadName, roadID);
            if (ret == 0 || ret == 1)
            {
                if (Search(roadID) == null)
                {
                    if (!_isFull)
                        Insert(roadID, speed);
                    else
                    {
                        Maximum();
                        Insert(roadID, speed);
                    }
                    Search(roadID).MaxCarNumbers = 1;
                }
                else
                {

                    long timeNow = ConvertDateTimeInt(DateTime.Now);
                    BstNode bt = Search(roadID);
                    bt.CarNumbers += 1;
                    bt.LatestTime = timeNow;

                    if (bt.CarNumbers > bt.MaxCarNumbers)
                    {
                        bt.MaxCarNumbers = bt.CarNumbers;
                    }
                    if (bt.isFull)
                    {
                        if (speed < bt.blockSpeed)
                        {

                            bt.blockCarNumbers = bt.CarNumbers;
                            bt.isFull = false;
                        }
                    }
                    

                    if (carSpeed.ContainsKey(carId))
                    {
                        carSpeed[carId] = speed;
                    }
                    else
                    {
                        carSpeed.Add(carId, speed);
                    }
                }
            }
            if (Search(roadID).isFull)
            {
                return Search(roadID).CarNumbers;
            }
            return Search(roadID).blockCarNumbers;
        }

        public int numberOnRoad(string roadName, string roadIDs)
        {
            int roadID = Hash(roadIDs);
            return Search(roadID).CarNumbers;
        }

        public void userBlock(string roadName)
        {
            int roadID = 0;
            if (_roadUpdate.ContainsKey(roadName))
                roadID = _roadUpdate[roadName];
            Search(roadID).isBlocked = 1;
        }

        public int Predict(string roadName)
        {
            int roadID = 0;
            if (_roadUpdate.ContainsKey(roadName))
                roadID = _roadUpdate[roadName];

            if (roadID == 0)
            {
                return 0;
            }
            else
            {
                BstNode bt = Search(roadID);
                int take1 = 0;
                int take2 = 0;
                int take3 = 0;
                int take4 = 0;

                int carNumbersNowAimRoad = bt.CarNumbers;
                int maxCarNumbersAimRoad = bt.MaxCarNumbers;
                int predictCarNumbersAimRoad1 = (int) (InUsePercentage*maxCarNumbersAimRoad);

                int predictCarNumbersAimRoad2 = bt.blockCarNumbers;

                int isBlockedAimRoad = bt.isBlocked;
                if (isBlockedAimRoad == 1)
                {
                    take1=7;
                }

                if (predictCarNumbersAimRoad1 < carNumbersNowAimRoad)
                {
                    take2 =  2;
                }
                if (predictCarNumbersAimRoad2*0.8 < carNumbersNowAimRoad)
                {
                    take3 =  7;
                }
                int blocked = 0, allCar = 0;
                double blockSpeed = bt.blockSpeed;


                //foreach (var temp in _carUpdate)
                //{
                //    if (temp.Value == roadID)
                //    {
                //        double thisSpeed = 0;
                //        if (carSpeed.ContainsKey(temp.Value))
                //            thisSpeed = carSpeed[temp.Value];
                //        if (thisSpeed < blockSpeed)
                //        {
                //            blocked++;
                //        }
                //        allCar++;
                //    }
                //}
                //if ((double)blocked / allCar > 0.3)
                //{
                //    take4 = 2;
                //}

                if (take1+take2+take3+take4 <= 6)
                    return 0;
                else
                    return 1;

            }

            return 0;

        }
    }
}

