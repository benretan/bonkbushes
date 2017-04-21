namespace UI.UIAnim
{
    using UnityEngine;
    using Extensions;
    using System;
    using UIAnims;
    using System.Collections.Generic;

    public  class UIAnim : MonoBehaviour
    {
        public string AnimName;
        public float AnimDuration;
        public bool AutoPlay = false;
        public bool LoopingAutoPlay = false;
        public UIAnimItem[] Item = EmptyArray<UIAnimItem>.Inst;
        public int BundleFixer;

        [NonSerialized]
        public bool IsPlaying = false;

        [NonSerialized]
        public bool IsLooping = false;

        private Action AnimEndAction;
        private float AnimTime;
        private float Lerp;
        //private HashSet<int> AlreadyLoggedDestroyedAnimObjs = new HashSet<int>();

        private bool Initialized = false;

        void Awake()
        {
            EnsureInit();
        }

        public void EnsureInit()
        {
            if (Initialized)
                return;

            foreach (var item in Item)
            {
                if (item != null)
                {
                    item.Initialize();
                }
            }
            Initialized = true;
        }

        void OnEnable()
        {
            if (AutoPlay)
            {
                if (!LoopingAutoPlay)
                    Animate();
                else if (!IsPlaying)
                    AnimateLooping();
            }
        }

        void OnDisable()
        {
            if (IsPlaying && !IsLooping)
            {
                OnAnimateEnd();
            }
        }

        void Update()
        {
            if (IsPlaying)
            {
                AnimTime += Time.unscaledDeltaTime;
                if (AnimTime >= AnimDuration)
                {
                    if (IsLooping)
                    {
                        AnimTime = 0f;
                    }
                    else
                    {
                        OnAnimateEnd();
                        return;
                    }
                }

                UpdateItems();
            }
        }

        public void SetTime(float time)
        {
            if (!AnimTime.Approximately(time))
            {
                AnimTime = Mathf.Clamp(time, 0, AnimDuration);
                UpdateItems();
            }
        }

        Vector3 LerpMaskedVector(UIAnimProperty mask, Vector3 value, Vector3 start, Vector3 end, UIAnimProperty x, UIAnimProperty y, UIAnimProperty z, float lerpTime)
        {
            var lerpPosition = Vector3.LerpUnclamped(start, end, lerpTime);

            if ((mask & x) != 0)
            {
                value.x = lerpPosition.x;
            }
            if ((mask & y) != 0)
            {
                value.y = lerpPosition.y;
            }
            if ((mask & z) != 0)
            {
                value.z = lerpPosition.z;
            }

            return value;
        }

        void UpdateItems(float? forcedLerp = null)
        {
            EnsureInit();

            float lastLerp = Lerp;
            Lerp = forcedLerp != null ? (float)forcedLerp : AnimTime / AnimDuration;
            for (int i = 0; i < Item.Length; i++)
            {
                UIAnimItem obj = Item[i];
                if (obj.AnimGameObject) // Sometimes objects destroy themselves during Awake, e.g.PlatformSpecific.cs
                {
                    float lerpTime = obj.AnimCurve.Evaluate(Lerp);

                    if (obj.AnimRectTransform)
                    {
                        if ((obj.Property & UIAnimProperty.Position2D) != 0)
                        {
                            if (!obj.UseRelativeSize)
                            {
                                obj.AnimRectTransform.anchoredPosition = LerpMaskedVector(obj.Property,
                                    obj.AnimRectTransform.anchoredPosition,
                                    obj.AnimStartPosition,
                                    obj.AnimEndPosition,
                                    UIAnimProperty.PositionX,
                                    UIAnimProperty.PositionY,
                                    UIAnimProperty.None,
                                    lerpTime);
                            }
                            else
                            {
                                Vector2 initialDelta = obj.AnimStartPosition - obj.InitialPosition;
                                Vector2 delta = (obj.AnimEndPosition - obj.AnimStartPosition) * lerpTime;

                                obj.AnimRectTransform.offsetMin = LerpMaskedVector(obj.Property,
                                    obj.AnimRectTransform.offsetMin,
                                    obj.InitialOffsetMin + initialDelta,
                                    obj.InitialOffsetMin + initialDelta + delta,
                                    UIAnimProperty.PositionX,
                                    UIAnimProperty.PositionY,
                                    UIAnimProperty.None,
                                    lerpTime);

                                obj.AnimRectTransform.offsetMax = LerpMaskedVector(obj.Property,
                                    obj.AnimRectTransform.offsetMax,
                                    obj.InitialOffsetMax + initialDelta,
                                    obj.InitialOffsetMax + initialDelta + delta,
                                    UIAnimProperty.PositionX,
                                    UIAnimProperty.PositionY,
                                    UIAnimProperty.None,
                                    lerpTime);
                            }
                        }

                        if ((obj.Property & UIAnimProperty.SizeDelta) != 0)
                        {
                            obj.AnimRectTransform.sizeDelta = LerpMaskedVector(obj.Property,
                                obj.AnimRectTransform.sizeDelta,
                                obj.AnimStartSizeDelta,
                                obj.AnimEndSizeDelta,
                                UIAnimProperty.SizeDeltaX,
                                UIAnimProperty.SizeDeltaY,
                                UIAnimProperty.None,
                                lerpTime);
                        }
                    }
                    else
                    {
                        if ((obj.Property & UIAnimProperty.Position3D) != 0)
                        {
                            obj.AnimTransform.localPosition = LerpMaskedVector(obj.Property,
                                obj.AnimTransform.localPosition,
                                obj.AnimStartPosition,
                                obj.AnimEndPosition,
                                UIAnimProperty.PositionX,
                                UIAnimProperty.PositionY,
                                UIAnimProperty.PositionZ,
                                lerpTime);
                        }
                    }
                    if ((obj.Property & UIAnimProperty.Scale) != 0)
                    {
                        obj.AnimTransform.localScale = LerpMaskedVector(obj.Property,
                            obj.AnimTransform.localScale,
                            obj.AnimStartScale,
                            obj.AnimEndScale,
                            UIAnimProperty.ScaleX,
                            UIAnimProperty.ScaleY,
                            UIAnimProperty.ScaleZ,
                            lerpTime);
                    }

                    if ((obj.Property & UIAnimProperty.Rotation) != 0)
                    {
                        if (Lerp == 0)
                            obj.InitialRotation = obj.UseRelativeRotation ? obj.AnimRectTransform.localEulerAngles : Vector3.zero;

                        obj.AnimTransform.localEulerAngles = LerpMaskedVector(obj.Property,
                            obj.AnimTransform.localEulerAngles,
                            obj.AnimStartRotation + obj.InitialRotation,
                            obj.AnimEndRotation + obj.InitialRotation,
                            UIAnimProperty.RotationX,
                            UIAnimProperty.RotationY,
                            UIAnimProperty.RotationZ,
                            lerpTime);
                    }


                    if ((obj.Property & UIAnimProperty.Color) != 0)
                    {
                        var lerpColor = Color.LerpUnclamped(obj.AnimStartColor, obj.AnimEndColor, lerpTime);
                        if (obj.AnimGraphic != null)
                        {
                            var startColor = obj.AnimGraphic.color;

                            if ((obj.Property & UIAnimProperty.ColorRed) != 0)
                            {
                                startColor.r = lerpColor.r;
                            }
                            if ((obj.Property & UIAnimProperty.ColorGreen) != 0)
                            {
                                startColor.g = lerpColor.g;
                            }
                            if ((obj.Property & UIAnimProperty.ColorBlue) != 0)
                            {
                                startColor.b = lerpColor.b;
                            }
                            if ((obj.Property & UIAnimProperty.Alpha) != 0)
                            {
                                startColor.a = lerpColor.a;
                            }

                            obj.AnimGraphic.color = startColor;

#if UNITY_EDITOR

                                if (obj.AnimGraphic.enabled)
                                {
                                    // dirty hack to make it update
                                    obj.AnimGraphic.enabled = false;
                                    obj.AnimGraphic.enabled = true;
                                }

#endif
                        }
                        else if (obj.AnimCanvasGroup)
                        {
                            if ((obj.Property & UIAnimProperty.Alpha) != 0)
                            {
                                obj.AnimCanvasGroup.alpha = lerpColor.a;
                            }
                        }
                    }

                    if ((obj.Property & UIAnimProperty.Effect) != 0)
                    {

#if UNITY_EDITOR
                        if (lastLerp > Lerp)
                        {
                            CleanUpPFX();
                        }
#endif
                    }

                    if ((obj.Property & UIAnimProperty.Shader) != 0)
                    {
                        Material mat = null;

                        if (obj.AnimGraphic)
                            mat = obj.AnimGraphic.materialForRendering;
                        if (obj.AnimRenderer)
                            mat = obj.AnimRenderer.material;
                    }
                }
                else
                {

                }
            }
        }

        public void OnAnimateEnd(bool shouldUpdateItems = true, bool shouldInvokeCallback = true)
        {
            AnimTime = 0f;

            // Set animation to end state.
            if (shouldUpdateItems && this)
            {
                UpdateItems(1f);
            }

            IsLooping = false;
            IsPlaying = false;

            if (shouldInvokeCallback)
            {
                if(AnimEndAction != null)
                    AnimEndAction.SafeInvoke();
            }
        }

        public void AnimateLooping(Action OnEnd = null)
        {
            IsLooping = true;
            Animate(OnEnd);
        }

        public void Animate(Action OnEnd = null)
        {
            if (IsPlaying)
            {

                return;
            }
            AnimEndAction = OnEnd;
            IsPlaying = true;

            // Set animation to start state.
            UpdateItems(0f);
        }

        public void ForceLerp(float value)
        {
            UpdateItems(value);
        }

        public void SetItemsActive(bool active)
        {
            for (int i = 0; i < Item.Length; i++)
            {
                if (Item[i].AnimGameObject)
                {
                    Item[i].AnimGameObject.SetActive(active);
                }
            }
        }

#if UNITY_EDITOR
        private Dictionary<string, ParticleSystem> Effects = new Dictionary<string, ParticleSystem>();

        public static string GetPFXName(string name, Transform parent)
        {
            string parentStr = string.Empty;
            if (parent)
                parentStr = parent.name + "_";

            return "pfx_" + parentStr + name;
        }

        public static ParticleSystem CreateEffect(string assetName, string creatorName, Transform parent, bool destroyWhenDone)
        {
            ParticleSystem pfx = null;
            if (!string.IsNullOrEmpty(assetName) && assetName != "No Effect")
            {
                var possible_paths = new string[]
                {
                    "Assets/Bundled/vfx/{0}/{0}.prefab",
                    "Assets/Bundled/VFX/{0}/{0}.prefab",
                    "Assets/Resources/vfx/{0}/{0}.prefab",
                    "Assets/Resources/VFX/{0}/{0}.prefab",
                };
                string path = null;
                for (int i = 0; i < possible_paths.Length; ++i)
                {
                    path = string.Format(possible_paths[i], assetName);
                    if (System.IO.File.Exists(path))
                        break;
                }
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj)
                {
                    pfx = Instantiate(obj).GetComponent<ParticleSystem>();
                    pfx.transform.localScale = Vector3.one;
                    pfx.name = GetPFXName(assetName, parent);
                }
            }

            return pfx;
        }

        public void CleanUpPFX()
        {
            if (Effects.Count > 0)
            {
                foreach (var kvp in Effects)
                {
                    if (kvp.Value != null)
                        DestroyImmediate(kvp.Value.gameObject);
                }

                Effects.Clear();
            }
        }

#endif


    }
}
