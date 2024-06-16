using UnityEngine;

namespace TarasK8.UI.Animations
{
    public partial class Easing
    {
        public enum Type
        {
            Linear,
            Custom,
            [InspectorName("Quad/In")]      InQuad,
            [InspectorName("Quad/Out")]     OutQuad,
            [InspectorName("Quad/In Out")]  InOutQuad,
            [InspectorName("Cubic/In")]     InCubic,
            [InspectorName("Cubic/Out")]    OutCubic,
            [InspectorName("Cubic/In Out")] InOutCubic,
            [InspectorName("Quart/In")]     InQuart,
            [InspectorName("Quart/Out")]    OutQuart,
            [InspectorName("Quart/In Out")] InOutQuart,
            [InspectorName("Quint/In")]     InQuint,
            [InspectorName("Quint/Out")]    OutQuint,
            [InspectorName("Quint/In Out")] InOutQuint,
            [InspectorName("Sine/In")]      InSine,
            [InspectorName("Sine/Out")]     OutSine,
            [InspectorName("Sine/In Out")]  InOutSine,
            [InspectorName("Expo/In")]      InExpo,
            [InspectorName("Expo/Out")]     OutExpo,
            [InspectorName("Expo/In Out")]  InOutExpo,
            [InspectorName("Circ/In")]      InCirc,
            [InspectorName("Circ/Out")]     OutCirc,
            [InspectorName("Circ/In Out")]  InOutCirc,
            [InspectorName("Elastic/In")]       InElastic,
            [InspectorName("Elastic/Out")]      OutElastic,
            [InspectorName("Elastic/In Out")]   InOutElastic,
            [InspectorName("Back/In")]      InBack,
            [InspectorName("Back/Out")]     OutBack,
            [InspectorName("Back/In Out")]  InOutBack,
            [InspectorName("Bounce/In")]    InBounce,
            [InspectorName("Bounce/Out")]   OutBounce,
            [InspectorName("Bounce/In Out")]InOutBounce,
        }
    }
}