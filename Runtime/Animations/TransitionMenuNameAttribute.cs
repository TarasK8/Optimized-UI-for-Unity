using System;

namespace TarasK8.UI.Animations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TransitionMenuNameAttribute : Attribute
    {
        private readonly string _menuName;

        public TransitionMenuNameAttribute(string menuName)
        {
            this._menuName = menuName;
        }

        public string MenuName
        {
            get { return _menuName; }
        }
    }
}