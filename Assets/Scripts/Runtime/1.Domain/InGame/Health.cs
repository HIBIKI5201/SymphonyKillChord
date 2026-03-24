using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public readonly struct Health
	{
    	public Health(float value) 
		{
			   Value = value;
    	}
   
        public readonly float Value;
    }
}
