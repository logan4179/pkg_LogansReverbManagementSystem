using UnityEditor;
using UnityEngine;

namespace LogansReverbManagementSystem
{
	//Todo: this should enable/disable adjacent reverb followers as the player travels across them
	//todo: should be able to disable logic that keeps sphere bounds inside box bounds.
	//todo: should be able to restrict a reverb zone along an axis

	public class ReverbFollower : ReverbSystemBase
	{
		[SerializeField] private BoxCollider myBoxCollider;
		private Transform trans_TriggerVolume;
		private MeshRenderer meshRenderer_volume;

		[Tooltip("The reverb zone that this trigger volume controls.")]
		[SerializeField] private AudioReverbZone myReverbZone;
		private Transform trans_reverbZone;

		[SerializeField, Tooltip("When enabled, restricts the reverb zone outer bounds to always be within trigger volume. Creates the most gradual volume fade. Usually not wanted when surrounded by walls on all sides like rooms and hallways")] 
		private bool restrictZoneBounds = true;

		//[Header("---------[[ LEVEL BIASING ]]----------")]
		//[SerializeField, Tooltip("Makes the reverbzone relative positioning biased towards how 'centered' the player is in the box collider. This typically results in more realistic reverb falloff."), Range(0f, 1f)]
		//private float centerWeightBias = 1f;

		[Header("-----[[ REFLECT DELAY BIASING ]]-----")]
		[Tooltip("Uses the ensuing values to dynamically decide how to bias the positioning of the reverb zone and affect the delay time.")]
		public bool UseBiasing = true;
		[SerializeField, Range(0f, 1f)] float centerBiasing = 1f;

		[SerializeField] private LRFS_Direction wallDirX;
		[SerializeField] private LRFS_Direction wallDirY;
		[SerializeField] private LRFS_Direction wallDirZ;

		[Header("-----[[ POSITION RESTRICT ]]-----")]
		[SerializeField] private bool lockZoneOnX = false;
		[SerializeField] private bool lockZoneOnY = false;
		[SerializeField] private bool lockZoneOnZ = false;

		[Header("-----[[ DEBUG ]]-----")]
		[SerializeField, TextArea(1, 10)] private string DBG_calculated;

		//[Header("-----[[ CALCULATED ]]-----")]
		private float cachedInitialReflections = 0f;
		private float cachedInitialReflectionsDelay = 0f;

		private float cachedReverbZoneRadius;

		private Vector3 followerBounds_min;
		private Vector3 followerBounds_max;

		private float boxHalfScaleX = 0f;
		private float boxHalfScaleY = 0f;
		private float boxHalfScaleZ = 0f;

		private float xReflectPercentage = 0f;
		private float yReflectPercentage = 0f;
		private float zReflectPercentage = 0f;

		private void Awake()
		{
			trans_TriggerVolume = myBoxCollider.GetComponent<Transform>();
			trans_reverbZone = myReverbZone.GetComponent<Transform>();
			myBoxCollider.TryGetComponent<MeshRenderer>(out meshRenderer_volume);
		}

		private void Start()
		{
			InitializeMe();

			if (meshRenderer_volume != null)
			{
				meshRenderer_volume.enabled = false;
			}
		}

		[TextArea(1,10)] public string dbgInit = "";
		public void InitializeMe()
		{
			cachedReverbZoneRadius = myReverbZone.maxDistance;

			boxHalfScaleX = (trans_TriggerVolume.localScale.x + trans_TriggerVolume.parent.localScale.x) / 2f;
			boxHalfScaleY = (trans_TriggerVolume.localScale.y + trans_TriggerVolume.parent.localScale.y) / 2f;
			boxHalfScaleZ = (trans_TriggerVolume.localScale.z + trans_TriggerVolume.parent.localScale.z) / 2f;

			followerBounds_min = new Vector3(
				lockZoneOnX ? trans_reverbZone.position.x : trans_TriggerVolume.position.x - (restrictZoneBounds ? (boxHalfScaleX > cachedReverbZoneRadius ? boxHalfScaleX - cachedReverbZoneRadius : 0) : boxHalfScaleX),
				lockZoneOnY ? trans_reverbZone.position.y : trans_TriggerVolume.position.y - (restrictZoneBounds ? (boxHalfScaleY > cachedReverbZoneRadius ? boxHalfScaleY - cachedReverbZoneRadius : 0) : boxHalfScaleY),
				lockZoneOnZ ? trans_reverbZone.position.z : trans_TriggerVolume.position.z - (restrictZoneBounds ? (boxHalfScaleZ > cachedReverbZoneRadius ? boxHalfScaleZ - cachedReverbZoneRadius : 0) : boxHalfScaleZ)
			);

			followerBounds_max = new Vector3(
				lockZoneOnX ? trans_reverbZone.position.x : trans_TriggerVolume.position.x + (restrictZoneBounds ? (boxHalfScaleX > cachedReverbZoneRadius ? boxHalfScaleX - cachedReverbZoneRadius : 0) : boxHalfScaleX),
				lockZoneOnY ? trans_reverbZone.position.y : trans_TriggerVolume.position.y + (restrictZoneBounds ? (boxHalfScaleY > cachedReverbZoneRadius ? boxHalfScaleY - cachedReverbZoneRadius : 0) : boxHalfScaleY),
				lockZoneOnZ ? trans_reverbZone.position.z : trans_TriggerVolume.position.z + (restrictZoneBounds ? (boxHalfScaleZ > cachedReverbZoneRadius ? boxHalfScaleZ - cachedReverbZoneRadius : 0) : boxHalfScaleZ)
			);

			#region REFLECTION BIAS CALCULATIONS ---------------------////////////
			cachedInitialReflections = myReverbZone.reflections;
			cachedInitialReflectionsDelay = myReverbZone.reflectionsDelay;

			float xReflectScale = wallDirX == LRFS_Direction.None ? 0f : boxHalfScaleX;
			float yReflectScale = wallDirY == LRFS_Direction.None ? 0f : boxHalfScaleY;
			float zReflectScale = wallDirZ == LRFS_Direction.None ? 0f : boxHalfScaleZ;
			float totalReflectScale = xReflectScale + yReflectScale + zReflectScale;

			if( wallDirX != LRFS_Direction.None )
			{
				xReflectPercentage = xReflectScale / totalReflectScale;
			}

			if( wallDirY != LRFS_Direction.None )
			{
				yReflectPercentage = yReflectScale / totalReflectScale;
			}

			if ( wallDirZ != LRFS_Direction.None )
			{
				zReflectPercentage = zReflectScale / totalReflectScale;
			}
			#endregion

			dbgInit = $"{nameof(cachedReverbZoneRadius)}: '{cachedReverbZoneRadius}'\n" +
				$"{nameof(boxHalfScaleX)}: '{boxHalfScaleX}'. scale bias: '{xReflectPercentage}'\n" +
				$"{nameof(boxHalfScaleY)}: '{boxHalfScaleY}'. scale bias: '{yReflectPercentage}'\n" +
				$"{nameof(boxHalfScaleZ)}: '{boxHalfScaleZ}'. scale bias: '{zReflectPercentage}'\n" +
				$"{nameof(followerBounds_min)}: '{followerBounds_min}'\n" +
				$"{nameof(followerBounds_max)}: '{followerBounds_max}'\n" +
				$"";
		}

		public override void UpdateMe( Vector3 listenerPosition )
		{
			Vector3 calculatedPosition = listenerPosition;

			if ( UseBiasing )
			{
				Vector3 v_itp = trans_TriggerVolume.InverseTransformPoint( listenerPosition ); //runs from -0.5 to 0.5
				
				//-1/1 at walls, 0 at center
				Vector3 v_relative = new Vector3( 
					Mathf.Clamp(v_itp.x * 2f, -1f, 1f),
					Mathf.Clamp(v_itp.y * 2f, -1f, 1f),
					Mathf.Clamp(v_itp.z * 2f, -1f, 1f)
				); 

				calculatedPosition = new Vector3(
					lockZoneOnX ? followerBounds_min.x : listenerPosition.x + (-v_relative.x * cachedReverbZoneRadius * centerBiasing),
					lockZoneOnY ? followerBounds_min.y : listenerPosition.y + (-v_relative.y * cachedReverbZoneRadius * centerBiasing),
					lockZoneOnZ ? followerBounds_min.z : listenerPosition.z + (-v_relative.z * cachedReverbZoneRadius * centerBiasing)
				);

				//This will run from 0 to 1...
				Vector3 v_linearTraversal = new Vector3(
					v_itp.x + 0.5f,
					v_itp.y + 0.5f,
					v_itp.z + 0.5f
				);

				float delayCalcX = 0f;
				if( wallDirX == LRFS_Direction.Positive )
				{
					delayCalcX = (1 - v_linearTraversal.x) * xReflectPercentage;
				}
				else if( wallDirX == LRFS_Direction.Negative )
				{
					delayCalcX = v_linearTraversal.x * xReflectPercentage;
				}
				else if( wallDirX == LRFS_Direction.Both )
				{
					delayCalcX = Mathf.Abs( Mathf.Abs(v_itp.x) + 0.5f ); //runs from 1 at walls, to 0.5 at center
				}

				float delayCalcY = 0f;
				if ( wallDirY == LRFS_Direction.Positive )
				{
					delayCalcY = (1 - v_linearTraversal.y) * yReflectPercentage;
				}
				else if ( wallDirY == LRFS_Direction.Negative )
				{
					delayCalcY = v_linearTraversal.y * yReflectPercentage;
				}
				else if ( wallDirY == LRFS_Direction.Both )
				{
					delayCalcY = Mathf.Abs( Mathf.Abs(v_itp.y) + 0.5f ); //runs from 1 at walls, to 0.5 at center
				}

				float delayCalcZ = 0f;
				if ( wallDirZ == LRFS_Direction.Positive )
				{
					delayCalcZ = (1 - v_linearTraversal.z) * zReflectPercentage;
				}
				else if ( wallDirZ == LRFS_Direction.Negative )
				{
					delayCalcZ = v_linearTraversal.z * zReflectPercentage;
				}
				else if ( wallDirZ == LRFS_Direction.Both )
				{
					delayCalcZ = Mathf.Abs( Mathf.Abs(v_itp.z) + 0.5f ); //runs from 1 at walls, to 0.5 at center
				}

				float calculatedReflectionDelayPercentage = delayCalcX + delayCalcY + delayCalcZ;

				myReverbZone.reflectionsDelay = cachedInitialReflectionsDelay * calculatedReflectionDelayPercentage; 

				DBG_calculated = $"{nameof(v_relative)}: '{v_relative}'\n" +
					$"{nameof(v_linearTraversal)}: '{v_linearTraversal}'\n" +
					$"{nameof(v_itp)}: '{v_itp}'\n" +
					$"{nameof(delayCalcX)}: '{delayCalcX}'\n" +
					$"{nameof(delayCalcY)}: '{delayCalcY}'\n" +
					$"{nameof(delayCalcZ)}: '{delayCalcZ}'\n" +
					$"{nameof(calculatedReflectionDelayPercentage)}: '{calculatedReflectionDelayPercentage}'\n" +
					$"";
			}

			#region CORRECT POSITIONING ----------------------------------------------
			if (calculatedPosition.x < followerBounds_min.x)
			{
				calculatedPosition.x = followerBounds_min.x;
			}
			else if (calculatedPosition.x > followerBounds_max.x)
			{
				calculatedPosition.x = followerBounds_max.x;
			}

			if (calculatedPosition.y < followerBounds_min.y)
			{
				calculatedPosition.y = followerBounds_min.y;
			}
			else if (calculatedPosition.y > followerBounds_max.y)
			{
				calculatedPosition.y = followerBounds_max.y;
			}

			if (calculatedPosition.z < followerBounds_min.z)
			{
				calculatedPosition.z = followerBounds_min.z;
			}
			else if (calculatedPosition.z > followerBounds_max.z)
			{
				calculatedPosition.z = followerBounds_max.z;
			}
			#endregion

			trans_reverbZone.position = calculatedPosition;
		}

		public override void EnterAction()
		{
			myReverbZone.enabled = true;
		}

		public override void ExitAction()
		{
			myReverbZone.enabled = false;
		}

		public void DrawDebugVisuals()
		{
			Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
			Gizmos.DrawSphere(trans_reverbZone.position, cachedReverbZoneRadius);
			Handles.Label(followerBounds_min + (Vector3.up * 2f), $"minX\n{followerBounds_min}");
			Gizmos.DrawLine(followerBounds_min, followerBounds_min + (Vector3.up * 2f));
			Handles.Label(followerBounds_max + (Vector3.up * 2f), $"maxX\n{followerBounds_max}");
			Gizmos.DrawLine(followerBounds_max, followerBounds_max + (Vector3.up * 2f));

		}

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			DrawDebugVisuals();
		}

		private void CheckIfKosher()
		{
			//bool amKosher = true;

			//if( )
		}
	}
}