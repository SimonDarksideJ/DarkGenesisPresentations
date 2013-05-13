using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Lightning.Engine
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
