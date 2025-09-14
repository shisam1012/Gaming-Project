using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIViewPresenter : MonoBehaviour
{
    [Header("UI Toolkit")]
    public UIDocument uiDocument;
    [Tooltip("Name of the Start button in the UXML (UI Builder 'Name' field, not the text).")]
    public string startButtonName = "StartButton";

    [Header("Behaviour")]
    [Tooltip("If true, we will SetActive(false) the GameObject after hiding the start view.")]
    public bool deactivateGameObjectOnStart = false;

    [Header("Events")]
    public UnityEvent onStartPressed;
    // Cache
    private VisualElement root;
    private Button startBtn;

    private void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        BindUI();
    }

    private void OnDisable()
    {
        if (startBtn != null) startBtn.clicked -= HandleStartClicked;
    }

    private void BindUI()
    {
        if (!uiDocument)
        {
            Debug.LogWarning("[UIViewPresenter] Missing UIDocument.");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("[UIViewPresenter] UIDocument has no rootVisualElement yet.");
            return;
        }

        if (!string.IsNullOrEmpty(startButtonName))
            startBtn = root.Q<Button>(startButtonName);
        if (startBtn == null)
        {
            startBtn = root.Q<Button>();
            if (startBtn == null)
            {
                Debug.LogWarning("[UIViewPresenter] No Button found in this UIDocument. " +
                                 "Name your Start button (e.g., 'StartButton') in UI Builder or assign a different name here.");
                return;
            }
        }

        startBtn.clicked -= HandleStartClicked;
        startBtn.clicked += HandleStartClicked;
    }

    private void HandleStartClicked()
    {
        Time.timeScale = 1f;

        HideStartView();

        onStartPressed?.Invoke();
    }


    public void HideStartView()
    {
        if (root == null && uiDocument != null) root = uiDocument.rootVisualElement;
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            root.pickingMode   = PickingMode.Ignore;
        }

        if (deactivateGameObjectOnStart)
            gameObject.SetActive(false);
    }

    public void ShowStartView()
    {
        if (root == null && uiDocument != null) root = uiDocument.rootVisualElement;
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            root.pickingMode   = PickingMode.Position;
        }

        if (deactivateGameObjectOnStart && !gameObject.activeSelf)
            gameObject.SetActive(true);

        BindUI();
    }
}
