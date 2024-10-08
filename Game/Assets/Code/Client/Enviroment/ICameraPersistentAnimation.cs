using UnityEngine;

namespace Client.Enviroment {

	public interface ICameraPersistentAnimation {
		
		public enum CameraType {
			Main,
			Background
		}
	
		void SetCamera(CameraType type, Camera cam);
	}

}