using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] GameObject background;
    [SerializeField] RectTransform joystickBG;
    [SerializeField] RectTransform joystick;
    [SerializeField] GameObject joystickGameObject;
    [SerializeField] Canvas canvas;

    Vector2 dragValue = Vector2.zero;
    
    public bool normalizdValue = false;


    Vector2 lastFrameDrag;
    public Vector2 GetDelta()
    {
        Vector2 result = dragValue - lastFrameDrag;
        lastFrameDrag = dragValue;
        
        if(normalizdValue)
        {
            result = result.normalized;
        }

        return result;
    }


    public Vector2 GetValue()
    {
        if(normalizdValue)
        {
            return dragValue.normalized;
        }

        return dragValue;
    }

    bool dragging = false;

    int joystickID = -1;

    Vector2 dragStartPosition;
    public void OnDrag(PointerEventData eventData)
    {
        if(eventData.pointerId == joystickID && dragging)
        {
            HandleOnDrag(eventData);
        }
    }


    void HandleOnDrag(PointerEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Vector3 pos = Input.mousePosition;
#else
        Vector3 pos = Input.GetTouch(joystickID).position;
#endif
        currentPosition = new Vector2(pos.x, pos.y) / canvas.scaleFactor;
        Vector2 direction = currentPosition - dragStartPosition;
        //input.UpdateMove(direction);

        dragValue = direction;
        if (joystick)
        {
            joystick.anchoredPosition = Vector2.ClampMagnitude(direction, joystickBG.rect.width * 0.2f);
        }
    }

    Vector2 currentPosition;


    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.pointerPressRaycast.gameObject == background)
        {
            BeginDrag(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;

        //joystickBG.anchoredPosition = cachedBGPosition;
        //joystick.anchoredPosition = Vector2.zero;
        UpdateUI(cachedBGPosition, Vector2.zero);

        dragValue = Vector2.zero;
    }


    void UpdateUI(Vector2 bgPostion, Vector2 nubPosition)
    {
        if(joystickBG)
        {
            joystickBG.anchoredPosition = bgPostion;
        }

        if(joystick)
        {
            joystick.anchoredPosition = nubPosition;
        }
    }


    void Start()
    {
        if(joystickBG)
        {
            cachedBGPosition = joystickBG.anchoredPosition;
        }
    }

    Vector2 cachedBGPosition;

    void BeginDrag(PointerEventData eventData)
    {
        dragging = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Vector3 pos = Input.mousePosition;
#else
        joystickID = eventData.pointerId;
        Vector3 pos = Input.GetTouch(joystickID).position;
#endif  

        dragStartPosition = new Vector2(pos.x, pos.y) / canvas.scaleFactor;
        if(joystickBG)
        {
            joystickBG.anchoredPosition = dragStartPosition;
        }
    }

}