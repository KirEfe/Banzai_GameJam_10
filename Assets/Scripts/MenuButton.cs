using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Масштаб")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 8f;

    [Header("События для художника")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    public UnityEvent onClick;

    private Vector3 _defaultScale;
    private Vector3 _targetScale;

    private void Awake()
    {
        _defaultScale = transform.localScale;
        _targetScale = _defaultScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = _defaultScale * hoverScale;
        onHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _defaultScale;
        onHoverExit?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}