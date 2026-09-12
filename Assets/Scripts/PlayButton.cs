using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPlayClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        gameManager.StartGame();
    }
}