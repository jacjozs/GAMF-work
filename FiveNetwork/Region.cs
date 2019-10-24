using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FiveNetwork
{
    public class Region : ICloneable
    {
        public int id { get; set; }
        public Point pos { get; set; }
        public int CoverUser { get {
                int count = 0;
                foreach (Tower tower in this.Towers)
                {
                    count += tower.users;
                }
                return count;
            } set { } }
        public int AllUser { get { return Users.Length;  } set { } }
        public User[] Users { get; set; }
        public Tower[] Towers { get; set; }
        public int[] Neighbor { get; set; }
        public int heightDown { get; set; }
        public int heightUp { get; set; }
        public int widhtDown { get; set; }
        public int widhtUp { get; set; }
        public double fitness { get; set; }
        public ArrayList parameters { get; set; }

        public Region(int id, Point pos, User[] Users, int CoverUser, int heightDown, int heightUp, int widhtDown, int widhtUp)
        {
            this.id = id;
            this.pos = pos;
            this.Users = Users;
            this.CoverUser = CoverUser;
            this.heightDown = heightDown;
            this.heightUp = heightUp;
            this.widhtDown = widhtDown;
            this.widhtUp = widhtUp;
            this.fitness = double.MaxValue;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void replaceTower(ArrayList rawTower)
        {
            this.Towers = new Tower[rawTower.Count / 3];
            for (int i = 0, o = 0; i < rawTower.Count; i += 3)
            {
                this.Towers[o++] = new Tower(new Point((double)rawTower[i], (double)rawTower[i + 1]), Network.R[int.Parse(rawTower[i + 2].ToString())], 0);
            }
        }
    }
}
