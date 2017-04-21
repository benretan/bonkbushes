
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIAnims
{
    [Flags]
    public enum UIAnimProperty
    {
        None = 0x0000,
        PositionX = 0x0001,
        PositionY = 0x0002,
        PositionZ = 0x0004,
        Position2D = 0x0003,
        Position3D = 0x0007,

        Effect = 0x0008,

        SizeDeltaX = 0x0010,
        SizeDeltaY = 0x0020,
        SizeDelta = 0x0030,

        RotationX = 0x0040,
        RotationY = 0x0080,
        RotationZ = 0x0100,
        Rotation = 0x01C0,

        ScaleX = 0x0200,
        ScaleY = 0x0400,
        ScaleZ = 0x0800,
        Scale = 0x0E00,

        ColorRed = 0x1000,
        ColorGreen = 0x2000,
        ColorBlue = 0x4000,
        Alpha = 0x8000,
        Color = 0xF000,

        // shader can do its own things
        Shader = 0x10000,

        TransformProperties = 0x0FFF,
        GraphicProperties = 0x1F000,
        RendererProperties = 0x10000
    }

    [System.Serializable]
    public class UIAnimItem
    {
        [NonSerialized]
        public CanvasGroup AnimCanvasGroup;

        public AnimationCurve AnimCurve;
        public Color AnimEndColor;
        public Vector3 AnimEndPosition;
        public Vector3 AnimEndRotation;
        public Vector3 AnimEndScale;
        public Vector2 AnimEndSizeDelta;
        public GameObject AnimGameObject;

        [NonSerialized]
        public Graphic AnimGraphic;

        [NonSerialized]
        public RectTransform AnimRectTransform;

        [NonSerialized]
        public Renderer AnimRenderer;

        public Color AnimStartColor;
        public Vector3 AnimStartPosition;
        public Vector3 AnimStartRotation;
        public Vector3 AnimStartScale;
        public Vector2 AnimStartSizeDelta;

        [NonSerialized]
        public Transform AnimTransform;

        public Vector2 InitialOffsetMax;
        public Vector2 InitialOffsetMin;
        public Vector3 InitialPosition;

        [System.NonSerialized]
        public Vector3 InitialRotation;

        public UIAnimProperty Property;
        public string UIEffectName;
        public float UIEffectTrigger;
        public bool UseRelativeRotation;
        public bool UseRelativeSize;

        public void Initialize()
        {
            // TODO: Set up game object
            if (AnimGameObject)
            {
                AnimTransform = AnimGameObject.transform;
                AnimRectTransform = AnimTransform as RectTransform;
            }

            if (AnimRectTransform)
            {
                if ((Property & UIAnimProperty.GraphicProperties) != 0)
                {
                    AnimGraphic = AnimRectTransform.GetComponent<Graphic>();
                }

                if ((Property & UIAnimProperty.Alpha) != 0 && !AnimGraphic)
                {
                    AnimCanvasGroup = AnimRectTransform.GetComponent<CanvasGroup>();
                }
            }
            else if (AnimTransform)
            {
                if ((Property & UIAnimProperty.RendererProperties) != 0)
                {
                    AnimRenderer = AnimTransform.GetComponent<Renderer>();
                }
            }
        }
    }

    [System.Serializable]
    public class UIShaderProperty
    {
        public Color EndColor;

        public float EndFloat;

        public Vector4 EndVector;

        public ShaderParameterType ParameterType;

        public string PropertyName;

        public Color StartColor;

        public float StartFloat;

        public Vector4 StartVector;

        public enum ShaderParameterType
        {
            Color,
            Float,
            Vector
        }
    }
}