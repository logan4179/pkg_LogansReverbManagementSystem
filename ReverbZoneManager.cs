using UnityEngine;

namespace LogansReverbManagementSystem
{
    public class ReverbZoneManager : ReverbSystemBase
    {
        [SerializeField] private RZinfo[] zones;


        void Start()
        {
            if( zones != null & zones.Length > 0 )
            {
                foreach( RZinfo zone in zones )
                {
                    zone.InitializeMe();
                }
            }
        }

        public override void UpdateMe( Vector3 listenerPosition )
		{
			if ( zones != null & zones.Length > 0 )
			{
				foreach ( RZinfo zone in zones )
				{
					zone.UpdateMe( listenerPosition );
				}
			}


		}

		public override void EnterAction()
		{
			if ( zones != null & zones.Length > 0 )
			{
				foreach ( RZinfo zone in zones )
				{
					zone.EnterAction();
				}
			}
		}

		public override void ExitAction()
		{
			if ( zones != null & zones.Length > 0 )
			{
				foreach ( RZinfo zone in zones )
				{
					zone.ExitAction();
				}
			}
		}
	}
}