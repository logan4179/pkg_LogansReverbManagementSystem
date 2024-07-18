using UnityEngine;

namespace LogansReverbManagementSystem
{
    [System.Serializable]
    public class RZinfo
    {
		public AudioReverbZone MyReverbZone;
        private Transform trans;

		//[Header("-----[[ CALCULATED ]]-----")]
		private float cachedInitialReflectionsDelay = 0f;


		[TextArea(1, 10)] public string dbgInit = "";
		public void InitializeMe()
		{
			trans = MyReverbZone.GetComponent<Transform>();
			cachedInitialReflectionsDelay = MyReverbZone.reflectionsDelay;

			dbgInit = $"{nameof(cachedInitialReflectionsDelay)}: '{cachedInitialReflectionsDelay}'\n" +
				$"{nameof(cachedInitialReflectionsDelay)}: '{cachedInitialReflectionsDelay}'\n" +
				$"";
		}

		[SerializeField, TextArea(1, 10)] private string DBG_calculated;
		public void UpdateMe( Vector3 listenerPosition )
		{
			float dist = Vector3.Distance( trans.position, listenerPosition );
			if ( dist > MyReverbZone.maxDistance )
			{
				return;
			}

			MyReverbZone.reflectionsDelay = cachedInitialReflectionsDelay * (dist / MyReverbZone.maxDistance);

			DBG_calculated = $"{nameof(dist)}: '{dist}'\n" +
				$"delay: ' {MyReverbZone.reflectionsDelay} '";
		}

		public void EnterAction()
		{
			MyReverbZone.enabled = true;
		}

		public void ExitAction()
		{
			MyReverbZone.enabled = false;
		}
	}
}