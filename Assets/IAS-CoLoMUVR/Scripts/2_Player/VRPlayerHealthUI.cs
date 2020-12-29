//The following code is licensed by CC BY 4.0
//at the Immersive Arts Space, Zurich University of the Arts
//By Chris Elvis Leisi - Associate Researcher at the Immersive Arts Space

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAS.CoLocationMUVR
{
	public class VRPlayerHealthUI : MonoBehaviour
	{
		public VRPlayerManager targetPlayer;
		public Vector3 positionOffset;
		public Transform positionTarget;
		
		//public Image healthBarImage;
		[Header("Health")]
		public AnimationCurve heartScaleModifire;
		public GameObject heartObject;
		public Mesh fullHeartMesh;
		public Mesh halfHeartMesh;
		
		private bool _isInitialized = false;
		public bool IsInitialized() { return this._isInitialized; }

		[System.Serializable]
		public class Heart
		{
			public MeshFilter heartMeshFilter;
			public MeshRenderer heartMeshRenderer;
		}
		
		public Heart[] hearts;
		public float heartsXPosOffset = 1.5f;
		
		public float heartScaleAnimationTime = 0.5f;
		public AnimationCurve heartsScaleAnimationCurve;
		
		public Material fullHealthMaterial;
		public Material middleHealthMaterial;
		public Material lessHealthMaterial;
		
		
		private List<Coroutine> _heartCoroutines = new List<Coroutine>();
		
		[Header("Damage Vigniette")]
		public MeshRenderer damageVigniette;
		public float damageVignietteAnimationTime;
		public AnimationCurve damageVignietteAlphaAnimationCurve;
		private Material _damageVignietteMaterial;
		private Color _baseColor;
		
		private void Start()
		{
			if (this.damageVigniette != null)
			{
				this._damageVignietteMaterial = this.damageVigniette.material;
				this._baseColor = this._damageVignietteMaterial.color;
				this._baseColor.a = 0f;
				this.damageVigniette.enabled = false;
				
				this._damageVignietteMaterial.color = this._baseColor;
			}
		}
		
		public void Reset()
		{
			this._isInitialized = false;
		}
		
		public void Initialize(float maxHealth)
		{
			if (this.hearts.Length <= 0)
			{
				this.hearts = new Heart[(int)maxHealth];
				
				for (int i = 0; i < maxHealth; i++)
				{
					this.hearts[i] = new Heart();
					GameObject newHeart = Instantiate(heartObject, this.transform, false);
					this.hearts[i].heartMeshRenderer = newHeart.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
					this.hearts[i].heartMeshFilter = newHeart.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
					
					newHeart.transform.localPosition = new Vector3((this.heartsXPosOffset * (maxHealth / 2f)) + ((this.heartsXPosOffset * (i + 0.5f)) * -1), 0, 0);
				}
			}
			
			this._isInitialized = true;
		}
		
		public void UpdateHeartLength(int length)
		{
			if (length <= this.hearts.Length)
				return;
			
			int currentHeartsCount = this.hearts.Length - 1;
			
			Heart[] newHearts = new Heart[length];
			
			for (int i = 0; i < length; i++)
			{
				if (i > currentHeartsCount)
				{
					newHearts[i] = new Heart();
					GameObject newHeart = Instantiate(heartObject, this.transform, false);
					newHearts[i].heartMeshRenderer = newHeart.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
					newHearts[i].heartMeshFilter = newHeart.transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
					
					newHeart.transform.localPosition = new Vector3((this.heartsXPosOffset * (length / 2f)) + ((this.heartsXPosOffset * (i + 0.5f)) * -1f), 0, 0);
				}
				else
				{
					newHearts[i] = new Heart();
					newHearts[i] = this.hearts[i];
					newHearts[i].heartMeshRenderer.transform.parent.localPosition = new Vector3((this.heartsXPosOffset * (length / 2f)) + ((this.heartsXPosOffset * (i + 0.5f)) * -1f), 0, 0);
				}
			}
			
			this.hearts = new Heart[length];
			newHearts.CopyTo(this.hearts, 0);
		}
		
		
		private void FixedUpdate()
		{
			this.UpdatePosition();
			this.UpdateRotation();
		}
		
		
		void UpdatePosition()
		{
			if (this.positionTarget != null)
			{
				this.gameObject.transform.position = this.positionTarget.transform.position + this.positionOffset;
			}
		}
		
		void UpdateRotation()
		{
			if (GameManager.Instance != null && GameManager.Instance.localVRPlayer != null)
				this.gameObject.transform.LookAt(GameManager.Instance.localVRPlayer.head.transform.position, Vector3.up);
		}
		
		public void UpdateHealthBar(float currentHealth)
		{
			if (this._heartCoroutines != null)
			{
				for (int i = this._heartCoroutines.Count - 1; i >= 0; i--)
				{
					if (this._heartCoroutines[i] != null)
					{
						StopCoroutine(this._heartCoroutines[i]);
						this._heartCoroutines.RemoveAt(i);
					}
				}
			}
			
			for (int i = 0; i < this.hearts.Length; i++)
			{
				if (i + 1 <= currentHealth)
					this.SetScale(true, false, this.hearts[i], i, currentHealth);
				else
				{
					if (currentHealth - (i) > 0)
						this.SetScale(true, true, this.hearts[i], i, currentHealth);
					else
						this.SetScale(false, false, this.hearts[i], i, currentHealth);
				}
				
			}
		}
		
		
		public GameObject GetFirstEmptyHeart(int currentHealth)
		{
			if (currentHealth >= this.hearts.Length)
				return null;
			else
				return this.hearts[currentHealth].heartMeshFilter.gameObject;
			
		}
		
		void SetScale(bool visibility, bool halfe, Heart target, int index, float currentHealth)
		{
			if (target != null)
			{
				if (this.fullHealthMaterial != null)
					target.heartMeshRenderer.material = this.fullHealthMaterial;
				
				if (halfe && this.halfHeartMesh != null)
					target.heartMeshFilter.mesh = this.halfHeartMesh;
				else if (!halfe && this.fullHeartMesh != null)
					target.heartMeshFilter.mesh = this.fullHeartMesh;
			}
			
			//when sichtbar und visibility false = mach kleiner;
			if (target.heartMeshRenderer.gameObject.activeInHierarchy && !visibility)
			{
				target.heartMeshRenderer.gameObject.SetActive(false);
				
				if (this.gameObject.activeInHierarchy)
					this._heartCoroutines.Add(StartCoroutine(this.ChangeScale(target, Vector3.one * 0.6f, this.heartsScaleAnimationCurve, false)));
				else
					this.ChangeScaleNoAnimation(target, Vector3.one * 0.6f, this.heartsScaleAnimationCurve, false);
			}
			
			//When nicht sichtbar und visibility true = grösser machen
			//when sichtbar und vilibility true = grösse anpassen an curve
			else if (visibility)
			{
				target.heartMeshRenderer.gameObject.SetActive(true);
				Vector3 targetScale = Vector3.one * (this.heartScaleModifire.Evaluate(currentHealth / 3) + 1);
				
				if (this.gameObject.activeInHierarchy)
					this._heartCoroutines.Add(StartCoroutine(this.ChangeScale(target, targetScale, this.heartsScaleAnimationCurve, true)));
				else
					this.ChangeScaleNoAnimation(target, targetScale, this.heartsScaleAnimationCurve, true);
				
			}
			
		}
		
		IEnumerator ChangeScale(Heart target, Vector3 targetScale, AnimationCurve targetCurve, bool visibility)
		{
			Vector3 startScale = target.heartMeshRenderer.transform.parent.localScale;
			Vector3 newTargetScale = (targetScale - startScale) * 2f + (startScale);
			float t = 0;
			while (t < this.heartScaleAnimationTime)
			{
				yield return new WaitForEndOfFrame();
				t += Time.deltaTime;
				
				target.heartMeshRenderer.transform.parent.localScale = Vector3.Lerp(startScale, newTargetScale, targetCurve.Evaluate(Mathf.InverseLerp(0, this.heartScaleAnimationTime, t)));
			}
			target.heartMeshRenderer.gameObject.SetActive(visibility);
		}
		
		void ChangeScaleNoAnimation(Heart target, Vector3 targetScale, AnimationCurve targetCurve, bool visibility)
		{
			target.heartMeshRenderer.transform.parent.localScale = (targetScale - target.heartMeshRenderer.transform.parent.localScale) * 2f + (target.heartMeshRenderer.transform.parent.localScale);
			
			target.heartMeshRenderer.gameObject.SetActive(visibility);
		}
		
		
		public void PlayDamageVigniette()
		{
			if (this.targetPlayer != null && !this.targetPlayer.isLocalPlayer)
				return;
			
			if (this._damageVignietteMaterial != null)
			{
				if (!this.damageVigniette.gameObject.activeInHierarchy)
					this.damageVigniette.gameObject.SetActive(true);
				
				StopCoroutine(this.DamageVignietteAnimation());
				StartCoroutine(this.DamageVignietteAnimation());
			}
		}
		
		IEnumerator DamageVignietteAnimation()
		{
			this.damageVigniette.enabled = true;
			this._baseColor.a = 0f;
			float t = 0;
			
			this._damageVignietteMaterial.color = this._baseColor;
			
			while (t < this.damageVignietteAnimationTime)
			{
				
				this._baseColor.a = this.heartsScaleAnimationCurve.Evaluate(1f / this.damageVignietteAnimationTime * t);
				this._damageVignietteMaterial.color = this._baseColor;
				
				yield return new WaitForEndOfFrame();
				
				t += Time.deltaTime;
			}
			
			this._baseColor.a = 0f;
			this._damageVignietteMaterial.color = this._baseColor;
			this.damageVigniette.enabled = false;
		}
		
		public void SetDamageVignietteAlpha(float a)
		{
			if (this.targetPlayer != null && !this.targetPlayer.isLocalPlayer)
				return;
			
			if (!this.damageVigniette.gameObject.activeInHierarchy)
				this.damageVigniette.gameObject.SetActive(true);
			
			if (a > 0)
				this.damageVigniette.enabled = true;
			else
				this.damageVigniette.enabled = false;
			
			this._baseColor.a = a;
			this._damageVignietteMaterial.color = this._baseColor;
		}
	}
}
