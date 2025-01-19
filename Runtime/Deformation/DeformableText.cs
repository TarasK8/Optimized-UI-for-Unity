using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI.Deformation
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Optimized UI/Deformation/Deformable Text")]
    [ExecuteAlways]
    public class DeformableText : DeformableGraphic
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            
            var text = graphic as TMP_Text;
            text.OnPreRenderText += OnPreRenderText;
            Debug.Log(text.text);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            var text = graphic as TMP_Text;
            text.OnPreRenderText -= OnPreRenderText;
        }

        private void OnPreRenderText(TMP_TextInfo obj)
        {
            //obj.meshInfo[0].
        }
    }
}
