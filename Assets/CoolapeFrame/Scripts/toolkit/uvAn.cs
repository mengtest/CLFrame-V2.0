using UnityEngine;

namespace Coolape
{
	public class uvAn:MonoBehaviour
	{
		public float scrollSpeed = 5;
		public int countX = 4;
		public int countY = 4;
		public float singleX = 0.0f;
		public float singleY = 0.0f;
		public Renderer renderer;

		private float offsetX = 0.0f;
		private float offsetY = 0.0f;
		public bool isSmooth = true;
		//private var singleTexSize;
		private bool isInit = false;

		public void Start ()
		{
			if (renderer == null) {
				renderer = GetComponent<Renderer> ();
			}
			//singleTexSize = Vector2(1.0/countX, 1.0/countY);
			//renderer.material.mainTextureScale = singleTexSize;
			renderer.material.mainTextureScale = new Vector2 (singleX, singleY);
			isInit = true;
		}
		float frame = 0;
		public void Update ()
		{
			if (!isInit)
				return;
			renderer.material.mainTextureScale = new Vector2 (singleX, singleY);
			if (isSmooth) {
				frame += Time.deltaTime * scrollSpeed;
			} else {
				frame = Mathf.Floor(Time.time * scrollSpeed);
			}
			offsetX = frame / countX;
			offsetY = -(1.0f / countY) - (frame - frame % countX) / countY / countX;
			renderer.material.SetTextureOffset ("_MainTex", new Vector2 (offsetX, offsetY));
		}
	}
}
