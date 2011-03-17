using System.Drawing;

namespace MSreinator
{
    internal struct Frame
    {
        public int Delay;
        public Bitmap Image;
        public int Number;
        public Point Offset;

        public Frame(int no, Bitmap image, Point offset, int delay)
        {
            Number = no;
            Image = image;
            Offset = offset;
            Delay = delay;
        }
    }
}