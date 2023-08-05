
using UnityEngine;
using UnityEngine.UI;


public class UIImageAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image;

    public bool isAnimating => currentAnimation != null;

    private Animation currentAnimation;


    private void Update()
    {
        if (isAnimating)
        {
            currentAnimation.Update();
            if (currentAnimation.isFinished) currentAnimation = null;
        }
    }


    [ContextMenu("AnimatePop")]
    public void _AnimatePop() { AnimatePop(); }
    public void AnimatePop(float duration=0.4f, float scale=1.1f) { SetAnimation(new PopAnimation(image, duration, scale)); }


    private void SetAnimation(Animation newAnimation)
    {
        if (isAnimating) currentAnimation.Reset();
        currentAnimation = newAnimation;
    }


    private abstract class Animation
    {
        protected Image image;
        protected float duration;
        protected float time;
        public bool isFinished => time >= duration;

        public Animation(Image image_, float duration_)
        {
            image = image_;
            duration = duration_;
            time = 0.0f;
        }

        public abstract void Reset();
        public abstract void Update();
    }

    private class PopAnimation : Animation
    {
        private const float ratio = 0.35f;
        private float scale;
        private Vector2 initialSizeDelta;

        public PopAnimation(Image image_, float duration_, float scale_) : base(image_, duration_)
        {
            scale = scale_;
            initialSizeDelta = image.rectTransform.sizeDelta;
        }

        public override void Reset()
        {
            image.rectTransform.sizeDelta = initialSizeDelta;
            time = 0.0f;
        }

        public override void Update()
        {
            if (isFinished) return;
            time += Time.deltaTime;
            if (time >= duration)
            {
                time = duration;
                return;
            }

            float t0 = time / duration;
            float t1;
            if (t0 < ratio) t1 = Util.Easing.EaseOutSine(t0 / ratio);
            else t1 = Util.Easing.EaseInSine(1.0f - (t0 - ratio) / (1.0f - ratio));
            float currentScale = 1.0f + (scale - 1.0f) * t1;
            image.rectTransform.sizeDelta = initialSizeDelta * currentScale;
        }
    }
}
