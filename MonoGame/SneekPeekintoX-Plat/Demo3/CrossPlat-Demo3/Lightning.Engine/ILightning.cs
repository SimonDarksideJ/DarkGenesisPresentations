using Microsoft.Xna.Framework.Graphics;

namespace Lightning.Engine
{
	// A common interface for LightningBolt and BranchLightning
	interface ILightning
	{
		bool IsComplete { get; }

		void Update();
		void Draw(SpriteBatch spriteBatch);
	}
}
