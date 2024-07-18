using UnityEngine;

namespace LogansReverbManagementSystem
{
	public static class LRMS_Utils
	{
		/// <summary>
		/// Scales a Vector from -1 to 1 depending on how close the x, y, and z are to the scale value.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="scale">The max expected scale of the Vector x, y, and z</param>
		/// <returns></returns>
		public static Vector3 NormalScaledVector( Vector3 vector, float scale )
		{
			Vector3 v = new Vector3(
				Mathf.Clamp(vector.x / scale, -1f, 1f),
				Mathf.Clamp(vector.y / scale, -1f, 1f),
				Mathf.Clamp(vector.z / scale, -1f, 1f)
			);

			return v;
		}
	}

	public enum LRFS_Direction
	{ 
		None,
		Positive,
		Negative,
		Both
	}

	public abstract class ReverbSystemBase : MonoBehaviour
	{
		/// <summary>
		/// Call this to update the values of this system based on the listening entity's position.
		/// </summary>
		/// <param name="listenerPosition"></param>
		public abstract void UpdateMe(Vector3 listenerPosition);

		/// <summary>
		/// Optionally call this to perform actions like enabling the reverb zone components of this system
		/// when the listener perspective has entered the bounds of this system. Typically you would call 
		/// this in an OnTriggerEnter from the listening entity's script. If you don't call this, the system 
		/// will remain as-is upon entry.
		/// </summary>
		public abstract void EnterAction();

		/// <summary>
		/// Optionally call this to perform actions like disabling the reverb zone components of this system
		/// when the listener perspective has exited the bounds of this system. Typically you would call 
		/// this in an OnTriggerExit from the listening entity's script. If you don't call this, the system 
		/// will remain as-is upon exit.
		/// </summary>
		public abstract void ExitAction();

	}
}
