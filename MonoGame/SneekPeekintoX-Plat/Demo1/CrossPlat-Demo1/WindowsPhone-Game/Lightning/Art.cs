using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace LightningDemo
{
	static class Art
	{
		public static Texture2D LightningSegment, HalfCircle, Pixel;

		public static void Load(ContentManager content)
		{
			LightningSegment = content.Load<Texture2D>("Lightning Segment");
			HalfCircle = content.Load<Texture2D>("Half Circle");
			Pixel = content.Load<Texture2D>("Pixel");
		}
	}
}
