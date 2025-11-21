using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CarSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class CarData
    {
        public string carIdentifier;
        public GameObject displayCar;
        public GameObject playerCarChild;
        public Button selectionButton;
        public Image carImage;
    }

    [Header("Player Car Reference")]
    public GameObject playerCarParent;

    [Header("Available Cars")]
    public List<CarData> availableCars = new List<CarData>();

    private int currentSelectedIndex = 0;

    void Start()
    {
        StartCoroutine(InitializeCarSelection());
    }

    IEnumerator InitializeCarSelection()
    {
        while (DataManager.Instance == null)
        {
            LogHelper.LogWarning("Waiting for DataManager to initialize...");
            yield return null;
        }

        LogHelper.Log("DataManager found! Loading car selection...");

        LoadSavedCarOnGameStart();
        SetupButtons();
        RefreshCarVisuals();
    }

    void OnEnable()
    {
        StaticEvents.CarUnlockEvents.OnCarUnlocked += OnCarUnlocked;
    }

    void OnDisable()
    {
        StaticEvents.CarUnlockEvents.OnCarUnlocked -= OnCarUnlocked;
    }

    void SetupButtons()
    {
        for (int i = 0; i < availableCars.Count; i++)
        {
            int index = i;
            if (availableCars[i].selectionButton != null)
            {
                availableCars[i].selectionButton.onClick.AddListener(() => OnCarButtonClicked(index));
            }
        }
    }

    void LoadSavedCarOnGameStart()
    {
        if (DataManager.Instance == null)
        {
            LogHelper.LogWarning("DataManager not found! Loading default car.");
            SelectCar(0);
            return;
        }

        int savedIndex = DataManager.Instance.gameData.carData.selectedCarIndex;

        LogHelper.Log($"Attempting to load saved car index: {savedIndex}");

        if (savedIndex < 0 || savedIndex >= availableCars.Count)
        {
            LogHelper.LogWarning($"Invalid saved car index {savedIndex}. Loading default car.");
            savedIndex = 0;
        }

        if (!CarUnlockManager.Instance.IsCarUnlocked(savedIndex))
        {
            LogHelper.LogWarning($"Saved car {savedIndex} is locked! Loading default car.");
            savedIndex = 0;
        }

        SelectCarWithoutSaving(savedIndex);

        LogHelper.Log($"Successfully loaded car index: {savedIndex}");
    }

    public void OnCarButtonClicked(int carIndex)
    {
        if (carIndex < 0 || carIndex >= availableCars.Count)
            return;

        if (!CarUnlockManager.Instance.IsCarUnlocked(carIndex))
        {
            LogHelper.LogWarning($"Car {carIndex} is locked!");
            return;
        }

        SelectCar(carIndex);
    }

    void SelectCarWithoutSaving(int carIndex)
    {
        foreach (var car in availableCars)
        {
            if (car.displayCar != null)
                car.displayCar.SetActive(false);
        }

        CarData selectedCar = availableCars[carIndex];
        if (selectedCar.displayCar != null)
            selectedCar.displayCar.SetActive(true);

        ActivatePlayerCar(carIndex);

        currentSelectedIndex = carIndex;
    }

    void SelectCar(int carIndex)
    {
        foreach (var car in availableCars)
        {
            if (car.displayCar != null)
                car.displayCar.SetActive(false);
        }

        CarData selectedCar = availableCars[carIndex];
        if (selectedCar.displayCar != null)
            selectedCar.displayCar.SetActive(true);

        ActivatePlayerCar(carIndex);

        currentSelectedIndex = carIndex;

        if (DataManager.Instance != null)
        {
            DataManager.Instance.gameData.carData.selectedCarIndex = carIndex;
            DataManager.Instance.SaveGameData();
            LogHelper.Log($"Car {carIndex} selected and saved!");
        }
        else
        {
            LogHelper.LogError("DataManager is null! Cannot save car selection!");
        }
    }

    void ActivatePlayerCar(int carIndex)
    {
        foreach (var car in availableCars)
        {
            if (car.playerCarChild != null)
            {
                car.playerCarChild.SetActive(false);
            }
        }

        CarData selectedCar = availableCars[carIndex];
        if (selectedCar.playerCarChild != null)
        {
            selectedCar.playerCarChild.SetActive(true);
            LogHelper.Log($"Activated player car: {selectedCar.carIdentifier}");
            StartCoroutine(RefreshCarComponents());
        }
        else
        {
            LogHelper.LogError($"Player car child is null for car index {carIndex}!");
        }
    }

    private IEnumerator RefreshCarComponents()
    {
        yield return null;

        SplineCarController splineController = playerCarParent.GetComponent<SplineCarController>();
        CarCollision carCollision = playerCarParent.GetComponent<CarCollision>();

        if (splineController != null)
        {
            splineController.RefreshCarChild();
        }

        if (carCollision != null)
        {
            carCollision.RefreshCarRigidbody();
        }
    }

    void OnCarUnlocked(int carIndex, GlobalEnums.CarUnlockType unlockType)
    {
        RefreshCarVisuals();
    }

    void RefreshCarVisuals()
    {
        for (int i = 0; i < availableCars.Count; i++)
        {
            bool isUnlocked = CarUnlockManager.Instance.IsCarUnlocked(i);

            if (availableCars[i].carImage != null)
            {
                availableCars[i].carImage.color = isUnlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            if (availableCars[i].selectionButton != null)
            {
                availableCars[i].selectionButton.interactable = isUnlocked;
            }
        }
    }

    [ContextMenu("Debug Current Car Selection")]
    void DebugCarSelection()
    {
        if (DataManager.Instance != null)
        {
            int savedIndex = DataManager.Instance.gameData.carData.selectedCarIndex;
            LogHelper.Log($"Saved Car Index: {savedIndex}");
            LogHelper.Log($"Current Selected Index: {currentSelectedIndex}");

            if (savedIndex >= 0 && savedIndex < availableCars.Count)
            {
                LogHelper.Log($"Saved Car Identifier: {availableCars[savedIndex].carIdentifier}");
            }
        }
    }
}