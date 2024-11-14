using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ironcow;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ScrollRect))]
public class UIPagingViewController : MonoAutoCaching, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] protected GameObject root = null;
    [SerializeField] private float animationDuration = 0.3f;

    private List<float> inTangent = new List<float>() { 0f, 1f };
    private List<float> outTangent = new List<float>() { 1f, 0f };

    private bool isAnimating = false;
    public bool isMove;
    private Vector2 destPosition;
    private Vector2 initialPosition;
    private AnimationCurve curve;
    private int prevPageIndex = 0;
    public int selectedIdx;
    private Rect currentViewRect;

    private RectTransform rectTransform;
    private ScrollRect scrollRect;

    public UnityAction<int> OnChangeValue;
    public UnityAction OnMoveStart;
    public UnityAction OnMoveEnd;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isAnimating = false;
        OnMoveStart?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnCalcMovePos(eventData.delta);
    }

    public void OnCalcMovePos(Vector2 delta, int targetPos = -1)
    { 
        scrollRect.StopMovement();

        float pageWidth = -(gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x);
        float pageHeight = -(gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y);

        selectedIdx = Mathf.RoundToInt((scrollRect.content.anchoredPosition.x) / (scrollRect.horizontal ? pageWidth : pageHeight));
        if(targetPos >= 0)
        {
            selectedIdx = targetPos;
        }
        if(prevPageIndex != selectedIdx)
        {
            OnChangeValue?.Invoke(selectedIdx);
        }

        OnMoveEnd?.Invoke();
        if (selectedIdx == prevPageIndex)
        {
            if (scrollRect.horizontal && Mathf.Abs(delta.x) >= 4)
            {
                scrollRect.content.anchoredPosition += new Vector2(delta.x, 0.0f);
                prevPageIndex += (int)Mathf.Sign(-delta.x);
            }
            if (scrollRect.vertical && Mathf.Abs(delta.y) >= 4)
            {
                scrollRect.content.anchoredPosition += new Vector2(0.0f, delta.y);
                prevPageIndex += (int)Mathf.Sign(-delta.y);
            }
        }

        if (selectedIdx < 0)
        {
            selectedIdx = 0;
        }
        else if(selectedIdx > gridLayoutGroup.transform.childCount - 1)
        {
            selectedIdx = gridLayoutGroup.transform.childCount - 1;
        }

        prevPageIndex = selectedIdx;

        float destX = prevPageIndex * pageWidth;
        float destY = prevPageIndex * pageHeight;
        destPosition = scrollRect.horizontal ? new Vector2(destX, scrollRect.content.anchoredPosition.y) : new Vector2(scrollRect.content.anchoredPosition.x, destY);

        initialPosition = scrollRect.content.anchoredPosition;

        Keyframe keyframe1 = new Keyframe(Time.time, 0.0f, inTangent[0], outTangent[0]);
        Keyframe keyframe2 = new Keyframe(Time.time + animationDuration, 1.0f, inTangent[1], outTangent[1]);
        curve = new AnimationCurve(keyframe1, keyframe2);

        isAnimating = true;
            
    }

    void LateUpdate()
    {
        if (isAnimating)
        {
            if (Time.time >= curve.keys[curve.length - 1].time)
            {
                scrollRect.content.anchoredPosition = destPosition;
                isAnimating = false;
                return;
            }
            Vector2 newPosition = initialPosition + (destPosition - initialPosition) * curve.Evaluate(Time.time);
            scrollRect.content.anchoredPosition = newPosition;
        }
    }

    void Awake()
    {
        rectTransform = transform as RectTransform;
        scrollRect = GetComponent<ScrollRect>();
    }

    void Start()
    {
        UpdateView();
    }

    void Update()
    {
        if (rectTransform.rect.width != currentViewRect.width || rectTransform.rect.height != currentViewRect.height)
        {
            UpdateView();
        }
    }

    private void UpdateView()
    {
        currentViewRect = rectTransform.rect;

        int paddingH = Mathf.RoundToInt((currentViewRect.width - gridLayoutGroup.cellSize.x) / 2.0f);
        int paddingV = Mathf.RoundToInt((currentViewRect.height - gridLayoutGroup.cellSize.y) / 2.0f);
        gridLayoutGroup.padding = new RectOffset(paddingH, paddingH, paddingV, paddingV);
    }
}