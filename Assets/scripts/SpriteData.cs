using UnityEngine;
using System.Collections;

public class SpriteData : MonoBehaviour {
	
	[System.Serializable]
	public class Frame
	{
		public float duration;
		//public Vector3 offset;
		public Sprite sprite;
	}
	[System.Serializable]
	public class Animation
	{
		public string name;
		public Frame[] frames;
	}
	
	public Animation[] animations = new Animation[1];
	
	public int GetAnimatoinIndex(string named)
	{
		for(int i = 0; i < animations.Length; ++i)
		{
			if(animations[i].name == named) {
				return i;
			}
		}
		return -1;
	}
	
	public Animation GetAnimation(string named) {
		int index = GetAnimatoinIndex (named);
		if(index < 0) return null;
		return animations [index];
	}
	
	[System.Serializable]
	public class Instance
	{
		public SpriteData data;
		public SpriteRenderer sr; //public Sprite sprite;
		//public Vector3 offset;
		public int animation, frame;
		public float timer;
		[HideInInspector]
		public int repetitionsOfThisAnimation = 0;
		float frameDuration = Mathf.Infinity;
		
		public Instance(SpriteRenderer sr, SpriteData data) { this.sr = sr; this.data = data; }
		
		public void Update(float time)
		{
			timer += time;
			if(timer >= frameDuration)
			{
				frame++;
				timer -= frameDuration;
				if(frame >= data.animations[animation].frames.Length) {
					frame = 0;
					repetitionsOfThisAnimation++;
				}
				frameDuration = Mathf.Infinity;
			}
			if(frameDuration == Mathf.Infinity)
			{
				frameDuration = data.animations[animation].frames[frame].duration;
				if(sr != null) {
					sr.sprite = GetCurrentSprite();
				}
				//offset = data.animations[animation].frames[frame].offset;
			}
		}
		
		public Sprite GetCurrentSprite() { return data.animations[animation].frames[frame].sprite; }
		
		public void RestartFrame()
		{
			frameDuration = Mathf.Infinity;
			timer = 0;
		}
		public void SetAnimation(int animationIndex)
		{
			if(animationIndex != animation) {
				repetitionsOfThisAnimation = 0;
				if(frame >= data.animations[animationIndex].frames.Length)
					frame = 0;
				animation = animationIndex;
			}
			RestartFrame ();
		}
		public bool SetAnimation(string named)
		{
			int index = data.GetAnimatoinIndex(named);
			if(index < 0 || index >= data.animations.Length)
				return false;
			SetAnimation(index);
			return true;
		}
	}
	
	public SpriteData.Instance CreateInstance(SpriteRenderer sr)
	{
		return new SpriteData.Instance(sr, this);
	}
}
