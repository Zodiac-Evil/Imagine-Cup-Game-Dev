using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CatchingFish
{
    class MenuEntry
    {
        public string Text
        {
            get { return Text; }
            set { Text = value; }
        }

        public Vector2 Position
        {
            get { return Position; }
            set { Position = value; }
        }

        public event EventHandler Selected;

        internal void OnSelectEntry()
        {
            if (Selected != null)
            {
                //
            }
        }
    }
}
