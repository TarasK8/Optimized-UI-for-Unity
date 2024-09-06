using TarasK8.UI.Animations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Selectable")]
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class Selectable : UnityEngine.UI.Selectable, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler
    {
        [SerializeField] private TransitionType _transitionType;
        [SerializeField] private StateMachine _stateMachine;
        [SerializeField] private int _normal, _hover, _pressed, _selected, _disabled;

        private bool _isHover, _isPressed, _isSelected;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (transition != Transition.None)
            {
                transition = Transition.None;
                //Debug.Log("The classic transition cannot be used here");
            }
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            UpdateState();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            _isPressed = true;
            UpdateState();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            _isPressed = false;
            UpdateState();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            _isHover = true;
            UpdateState();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            _isHover = false;
            UpdateState();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            _isSelected = true;
            UpdateState();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            _isSelected = false;
            UpdateState();
        }

        public StateMachine GetStateMachine()
        {
            return _stateMachine;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            
        }

        private void UpdateState()
        {
            if (gameObject.activeSelf == false ||
                _stateMachine == null ||
                _transitionType != TransitionType.StateMachine ||
                Application.isPlaying == false)
                return;

            if (base.interactable == false)
            {
                _stateMachine.SetState(_disabled);
                return;
            }

            if (_isPressed)
            {
                _stateMachine.SetState(_pressed);
            }
            else if (_isHover)
            {
                _stateMachine.SetState(_hover);
            }
            else if (_isSelected)
            {
                _stateMachine.SetState(_selected);
            }
            else
            {
                _stateMachine.SetState(_normal);
            }
        }

        public enum TransitionType
        {
            None = 0,
            StateMachine = 1
        }
    }
}