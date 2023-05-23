using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject _housesPanel;
    private bool _housesPanelEnabled;

    [SerializeField] private GameObject _farmsPanel;
    private bool _farmsPanelEnabled;

    [SerializeField] private SavingsManager savingsManager;

    private void Update()
    {
        _housesPanelEnabled = _housesPanel.activeSelf;
        _farmsPanelEnabled = _farmsPanel.activeSelf;
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _ = savingsManager.SaveGameAsync();
            Debug.Log("Game saved!");
            SceneManager.LoadScene(1);
        }
    }

    public void ToggleHousesPanel()
    {
        _housesPanelEnabled = !_housesPanelEnabled;
        _housesPanel.SetActive(_housesPanelEnabled);
    }

    public void ToggleFarmsPanel()
    {
        _farmsPanelEnabled = !_farmsPanelEnabled;
        _farmsPanel.SetActive(_farmsPanelEnabled);
    }
}
