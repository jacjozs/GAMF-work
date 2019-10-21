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
        public int CoverUser { get; set; }
        public int AllUser { get { return Users.Length;  } set { } }
        public Point[] Users { get; set; }
        public Tower[] Towers { get; set; }
        public int heightDown { get; set; }
        public int heightUp { get; set; }
        public int widhtDown { get; set; }
        public int widhtUp { get; set; }

        public Region(int id, Point[] Users, int CoverUser, int heightDown, int heightUp, int widhtDown, int widhtUp)
        {
            this.id = id;
            this.Users = Users;
            this.CoverUser = CoverUser;
            this.heightDown = heightDown;
            this.heightUp = heightUp;
            this.widhtDown = widhtDown;
            this.widhtUp = widhtUp;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
