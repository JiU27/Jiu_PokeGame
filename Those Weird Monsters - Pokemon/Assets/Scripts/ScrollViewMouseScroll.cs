using UnityEngine;
using UnityEngine.UI;

public class ScrollViewMouseScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 10f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector2 scrollPosition = scrollRect.normalizedPosition;
            scrollPosition.y += scroll * scrollSpeed * Time.deltaTime;
            scrollRect.normalizedPosition = scrollPosition;
        }
    }
}