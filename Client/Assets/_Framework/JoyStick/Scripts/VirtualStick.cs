using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ironcow
{
    public class VirtualStick : MonoSingleton<VirtualStick>, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private RectTransform rt;
        [SerializeField, Range(10f, 150f)] private float handleRange;

        private Vector2 inputVector
        {
            set
            {
                if (_inputVector != value)
                {
                    OnHandleChanged?.Invoke(value);
                }
                _inputVector = value;
            }
        }
        [SerializeField] private Vector2 _inputVector;
        private bool isInput;

        public UnityAction<Vector2> OnHandleChanged;

        protected override void Awake()
        {
            base.Awake();
            var size = handle.sizeDelta.x;
            var rtSize = rt.sizeDelta.x;
            handleRange = (rtSize - size) / 2;
        }

        public void ControlStickHandle(PointerEventData eventData)
        {
            var inputDir = eventData.position - rt.anchoredPosition;
            var clampedDir = inputDir.magnitude < handleRange ? inputDir
                : inputDir.normalized * handleRange;
            handle.anchoredPosition = clampedDir;
            inputVector = clampedDir / handleRange;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ControlStickHandle(eventData);
            isInput = true;
        }

        // 오브젝트를 클릭해서 드래그 하는 도중에 들어오는 이벤트
        // 하지만 클릭을 유지한 상태로 마우스를 멈추면 이벤트가 들어오지 않음    
        public void OnDrag(PointerEventData eventData)
        {
            ControlStickHandle(eventData);
            isInput = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            handle.anchoredPosition = Vector2.zero;
            inputVector = Vector2.zero;
        }
    }
}