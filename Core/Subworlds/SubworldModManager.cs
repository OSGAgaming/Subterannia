using Subterannia.Core.Mechanics.Interfaces;
using Terraria.ModLoader;

namespace Subterannia
{
	public class SubworldInstance : ILoad
	{
		public bool IsSaving;

		public void Load() { }

		public void Unload() 
		{
			IsSaving = false;
		}
	}
}