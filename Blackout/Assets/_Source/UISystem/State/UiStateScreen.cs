using UnityEngine;

namespace UISystem
{
    public class UiStateScreen : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private GameUiState visibleState;

        public GameUiState VisibleState => visibleState;

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}