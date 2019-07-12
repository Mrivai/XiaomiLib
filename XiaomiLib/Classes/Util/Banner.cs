namespace Mrivai.Pelitabangsa
{
    public class Banner
    {
        /// <summary>
        /// Manage Banner
        /// </summary>
        private int version;
        private int length;
        private int pid;
        private int realwidth;
        private int realheight;
        private int virtualwidth;
        private int virtualheight;
        private int orientation;
        private int quirks;
        /// <summary>
        /// version
        /// </summary>
        public int Version
        {
            get
            {
                return version;
            }
            set
            {
                version = value;
            }
        }
        /// <summary>
        /// length
        /// </summary>
        public int Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
            }
        }
        /// <summary>
        /// pid
        /// </summary>
        public int Pid
        {
            get
            {
                return pid;
            }
            set
            {
                pid = value;
            }
        }
        /// <summary>
        /// realwidth
        /// </summary>
        public int RealWidth
        {
            get
            {
                return realwidth;
            }
            set
            {
                realwidth = value;
            }
        }
        /// <summary>
        /// real heigth
        /// </summary>
        public int RealHeight
        {
            get
            {
                return realheight;
            }
            set
            {
                realheight = value;
            }
        }
        /// <summary>
        /// virtual width
        /// </summary>
        public int VirtualWidth
        {
            get
            {
                return virtualwidth;
            }
            set
            {
                virtualwidth = value;
            }
        }
        /// <summary>
        /// virtual heigth
        /// </summary>
        public int VirtualHeight
        {
            get
            {
                return virtualheight;
            }
            set
            {
                virtualheight = value;
            }
        }
        /// <summary>
        /// orientiation
        /// </summary>
        public int Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation = value;
            }
        }
        /// <summary>
        /// quirks
        /// </summary>
        public int Quirks
        {
            get
            {
                return quirks;
            }
            set
            {
                quirks = value;
            }
        }
        /// <summary>
        /// get banner string
        /// </summary>
        public override string ToString()
        {
            return "Banner [version=" + version + ", length=" + length + ", pid="
                + pid + ", readWidth=" + realwidth + ", readHeight="
                + realheight + ", virtualWidth=" + virtualwidth
                + ", virtualHeight=" + virtualheight + ", orientation="
                + orientation + ", quirks=" + quirks + "]";
        }
    }
}
