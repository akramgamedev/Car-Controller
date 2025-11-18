using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CarSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class CarData
    {
        public string carName;
        public GameObject displayCar;
        public GameObject playerCarChild;
        public Button selectionButton;
    }

    [Header("Player Car Reference")]
    public GameObject playerCarParent;

    [Header("Available Cars")]
    public List<CarData> availableCars = new List<CarData>();

    private int currentSelectedIndex = 0;

    void Start()
    {
        // Assign button listeners
        for (int i = 0; i < availableCars.Count; i++)
        {
            int index = i;
            if (availableCars[i].selectionButton != null)
            {
                availableCars[i].selectionButton.onClick.AddListener(() => OnCarButtonClicked(index));
            }
        }
        SelectCar(0);

        // Load saved selection or default to first car
        // int savedCarIndex = DataManager.Instance != null ? DataManager.Instance.GetSelectedCarIndex() : 0;
        
        // Select the saved car if unlocked, otherwise first car
        // if (CarUnlockManager.Instance.IsCarUnlocked(savedCarIndex))
        // {
        //     SelectCar(savedCarIndex);
        // }
        // else
        // {
        //     SelectCar(0); // Fallback to first car
        // }
    }

    public void OnCarButtonClicked(int carIndex)
    {
        if (carIndex < 0 || carIndex >= availableCars.Count)
            return;

        // Check if car is unlocked
        if (!CarUnlockManager.Instance.IsCarUnlocked(carIndex))
        {
            LogHelper.LogWarning($"Car {carIndex} is locked! Need to unlock first.");
            // You can show a popup here saying "Car locked! Collect more coins"
            return;
        }

        currentSelectedIndex = carIndex;
        SelectCar(carIndex);
        
        // Save selection
        // DataManager.Instance?.SetSelectedCarIndex(carIndex);
    }

    void SelectCar(int carIndex)
    {
        // Disable all display cars
        foreach (var car in availableCars)
        {
            if (car.displayCar != null)
                car.displayCar.SetActive(false);
        }

        // Enable selected display car
        CarData selectedCar = availableCars[carIndex];
        if (selectedCar.displayCar != null)
            selectedCar.displayCar.SetActive(true);

        // Activate corresponding player car
        ActivatePlayerCar(carIndex);
        
        currentSelectedIndex = carIndex;
    }

    void ActivatePlayerCar(int carIndex)
    {
        LogHelper.Log($"=== ActivatePlayerCar called for index: {carIndex} ===");

        // Deactivate all car children
        foreach (var car in availableCars)
        {
            if (car.playerCarChild != null)
            {
                car.playerCarChild.SetActive(false);
            }
        }

        // Activate only the selected child
        CarData selectedCar = availableCars[carIndex];
        if (selectedCar.playerCarChild != null)
        {
            LogHelper.Log($"Activating child: {selectedCar.playerCarChild.name}");
            selectedCar.playerCarChild.SetActive(true);

            // Refresh the parent's scripts
            StartCoroutine(RefreshCarComponents());
        }
    }

    private IEnumerator RefreshCarComponents()
    {
        yield return null; // Wait one frame

        SplineCarController splineController = playerCarParent.GetComponent<SplineCarController>();
        CarCollision carCollision = playerCarParent.GetComponent<CarCollision>();

        if (splineController != null)
        {
            splineController.RefreshCarChild();
            LogHelper.Log("✓ Called RefreshCarChild");
        }

        if (carCollision != null)
        {
            carCollision.RefreshCarRigidbody();
            LogHelper.Log("✓ Called RefreshCarRigidbody");
        }
    }
}





// ************ Working Code *****************
// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using System.Collections;
// using TMPro;
// using Unity.Mathematics.Geometry;
// using Unity.Mathematics;

// public class CarSelectionManager : MonoBehaviour
// {
//     [System.Serializable]
//     public class CarData
//     {
//         public string carName;
//         public GameObject displayCar;  // Car shown in selection screen
//         public GameObject playerCarChild;   // The actual car used in gameplay
//         public Button selectionButton; // UI button for selecting the car

//     }
//     [Header("Player Car Reference")]
//     public GameObject playerCarParent;

//     [Header("Available Cars")]
//     public List<CarData> availableCars = new List<CarData>();

//     private int currentSelectedIndex = 0;

//     void Start()
//     {
//         // Assign button listeners
//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             int index = i;
//             if (availableCars[i].selectionButton != null)
//             {
//                 availableCars[i].selectionButton.onClick.AddListener(() => OnCarButtonClicked(index));
//             }
//         }

//         // Show first car by default
//         if (availableCars.Count > 0)
//         {
//             SelectCar(0);
//         }
//     }

//     public void OnCarButtonClicked(int carIndex)
//     {
//         if (carIndex < 0 || carIndex >= availableCars.Count)
//             return;

//         currentSelectedIndex = carIndex;
//         SelectCar(carIndex);
//     }

//     void SelectCar(int carIndex)
//     {
//         // Disable all display cars
//         foreach (var car in availableCars)
//         {
//             if (car.displayCar != null)
//                 car.displayCar.SetActive(false);
//         }

//         // Enable selected display car
//         CarData selectedCar = availableCars[carIndex];
//         if (selectedCar.displayCar != null)
//             selectedCar.displayCar.SetActive(true);

//         // Activate corresponding player car
//         ActivatePlayerCar(carIndex);
//     }

//     void ActivatePlayerCar(int carIndex)
//     {
//         LogHelper.Log($"=== ActivatePlayerCar called for index: {carIndex} ===");

//         // Deactivate ALL children in the parent
//         foreach (var car in availableCars)
//         {
//             if (car.playerCarChild != null)
//             {
//                 car.playerCarChild.SetActive(false);
//             }
//         }

//         // Activate only the selected child
//         CarData selectedCar = availableCars[carIndex];
//         if (selectedCar.playerCarChild != null)
//         {
//             LogHelper.Log($"Activating child: {selectedCar.playerCarChild.name}");
//             selectedCar.playerCarChild.SetActive(true);

//             // Refresh the parent's scripts
//             StartCoroutine(RefreshCarComponents());
//         }
//     }

//     private IEnumerator RefreshCarComponents()
//     {
//         yield return null; // Wait one frame

//         SplineCarController splineController = playerCarParent.GetComponent<SplineCarController>();
//         CarCollision carCollision = playerCarParent.GetComponent<CarCollision>();

//         if (splineController != null)
//         {
//             splineController.RefreshCarChild();
//             LogHelper.Log("✓ Called RefreshCarChild");
//         }

//         if (carCollision != null)
//         {
//             carCollision.RefreshCarRigidbody();
//             LogHelper.Log("✓ Called RefreshCarRigidbody");
//         }
//     }
// }


//***********************************************************
// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;

// public class CarSelectionManager : MonoBehaviour
// {
//     [System.Serializable]
//     public class CarData
//     {
//         public string carName;
//         public GameObject displayCar;        // Car on the display platform
//         public GameObject playerCar;         // Player car in hierarchy
//         public Button selectionButton;       // The UI button for this car
//         public Sprite carIcon;               // Icon for button
//         public int unlockCost;              // Cost to unlock
//         public bool isUnlocked = false;
//     }

//     [Header("Car Configuration")]
//     public List<CarData> availableCars = new List<CarData>();

//     [Header("UI References")]
//     public Button unlockButton;
//     public Text unlockButtonText;
//     public Text cashText;

//     [Header("Game Settings")]
//     public int playerCash = 1255;

//     private int currentSelectedIndex = 0;

//     void Start()
//     {
//         // Initialize car buttons
//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             int index = i;
//             if (availableCars[i].selectionButton != null)
//             {
//                 availableCars[i].selectionButton.onClick.AddListener(() => OnCarButtonClicked(index));

//                 // Set icon if available
//                 if (availableCars[i].carIcon != null)
//                 {
//                     Image buttonImage = availableCars[i].selectionButton.GetComponent<Image>();
//                     if (buttonImage != null)
//                     {
//                         buttonImage.sprite = availableCars[i].carIcon;
//                     }
//                 }
//             }
//         }

//         // Setup unlock button
//         if (unlockButton != null)
//         {
//             unlockButton.onClick.AddListener(OnUnlockButtonClicked);
//         }

//         // Unlock first car by default
//         if (availableCars.Count > 0)
//         {
//             availableCars[0].isUnlocked = true;
//         }

//         // Show first car
//         SelectCar(0);
//         UpdateUI();
//     }

//     public void OnCarButtonClicked(int carIndex)
//     {
//         if (carIndex < 0 || carIndex >= availableCars.Count)
//             return;

//         currentSelectedIndex = carIndex;
//         SelectCar(carIndex);
//         UpdateUI();
//     }

//     void SelectCar(int carIndex)
//     {
//         CarData selectedCar = availableCars[carIndex];

//         // Disable all display cars
//         foreach (var car in availableCars)
//         {
//             if (car.displayCar != null)
//                 car.displayCar.SetActive(false);
//         }

//         // Enable selected display car
//         if (selectedCar.displayCar != null)
//             selectedCar.displayCar.SetActive(true);

//         // Handle player car in hierarchy (only if unlocked)
//         if (selectedCar.isUnlocked)
//         {
//             ActivatePlayerCar(carIndex);
//         }
//     }

//     void ActivatePlayerCar(int carIndex)
//     {
//         // Disable all player cars
//         foreach (var car in availableCars)
//         {
//             if (car.playerCar != null)
//                 car.playerCar.SetActive(false);
//         }

//         CarData selectedCar = availableCars[carIndex];
//         if (selectedCar.playerCar != null)
//             selectedCar.playerCar.SetActive(true);
//     }

//     void OnUnlockButtonClicked()
//     {
//         CarData selectedCar = availableCars[currentSelectedIndex];

//         if (selectedCar.isUnlocked)
//         {
//             ActivatePlayerCar(currentSelectedIndex);
//             return;
//         }

//         if (playerCash >= selectedCar.unlockCost)
//         {
//             playerCash -= selectedCar.unlockCost;
//             selectedCar.isUnlocked = true;
//             ActivatePlayerCar(currentSelectedIndex);
//             UpdateUI();
//             SaveProgress();
//             Debug.Log($"Unlocked {selectedCar.carName}!");
//         }
//         else
//         {
//             Debug.Log("Not enough cash!");
//         }
//     }

//     void UpdateUI()
//     {
//         if (cashText != null)
//         {
//             cashText.text = playerCash.ToString();
//         }

//         CarData selectedCar = availableCars[currentSelectedIndex];

//         if (unlockButton != null && unlockButtonText != null)
//         {
//             if (selectedCar.isUnlocked)
//             {
//                 unlockButtonText.text = "SELECTED";
//                 unlockButton.interactable = false;
//             }
//             else
//             {
//                 unlockButtonText.text = $"UNLOCK\nFOR ${selectedCar.unlockCost}";
//                 unlockButton.interactable = playerCash >= selectedCar.unlockCost;
//             }
//         }
//     }

//     // Save/Load
//     public void SaveProgress()
//     {
//         PlayerPrefs.SetInt("PlayerCash", playerCash);
//         PlayerPrefs.SetInt("SelectedCar", currentSelectedIndex);

//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             PlayerPrefs.SetInt($"Car_{i}_Unlocked", availableCars[i].isUnlocked ? 1 : 0);
//         }

//         PlayerPrefs.Save();
//     }

//     public void LoadProgress()
//     {
//         playerCash = PlayerPrefs.GetInt("PlayerCash", 1255);
//         currentSelectedIndex = PlayerPrefs.GetInt("SelectedCar", 0);

//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             availableCars[i].isUnlocked = PlayerPrefs.GetInt($"Car_{i}_Unlocked", i == 0 ? 1 : 0) == 1;
//         }

//         SelectCar(currentSelectedIndex);
//         UpdateUI();
//     }

//     public void AddCash(int amount)
//     {
//         playerCash += amount;
//         UpdateUI();
//         SaveProgress();
//     }

//     public string GetCurrentCarName()
//     {
//         if (currentSelectedIndex >= 0 && currentSelectedIndex < availableCars.Count)
//             return availableCars[currentSelectedIndex].carName;
//         return "";
//     }

//     public GameObject GetCurrentPlayerCar()
//     {
//         if (currentSelectedIndex >= 0 && currentSelectedIndex < availableCars.Count)
//             return availableCars[currentSelectedIndex].playerCar;
//         return null;
//     }

//     public bool IsCarUnlocked(int carIndex)
//     {
//         if (carIndex >= 0 && carIndex < availableCars.Count)
//             return availableCars[carIndex].isUnlocked;
//         return false;
//     }
// }



// // ==========================================
// // CarSelectionManager.cs - ONLY SCRIPT YOU NEED
// // ==========================================
// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;

// public class CarSelectionManager : MonoBehaviour
// {
//     [System.Serializable]
//     public class CarData
//     {
//         public string carName;
//         public GameObject displayCar;        // Car on the display platform (in hierarchy)
//         public GameObject playerCar;         // Player car in hierarchy for gameplay
//         public Button selectionButton;       // The grid button for this car
//         public Sprite carIcon;               // Icon to show on the button (optional)
//         public int unlockCost;              // Cost to unlock (0 if already unlocked)
//         public bool isUnlocked = false;
//     }

//     [Header("Car Configuration")]
//     public List<CarData> availableCars = new List<CarData>();

//     [Header("UI References")]
//     public Button unlockButton;
//     public Text unlockButtonText;
//     public Text cashText;

//     [Header("Game Settings")]
//     public int playerCash = 1255;

//     private int currentSelectedIndex = 0;

//     void Start()
//     {
//         // Initialize all car buttons
//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             int index = i; // Capture for closure
//             if (availableCars[i].selectionButton != null)
//             {
//                 availableCars[i].selectionButton.onClick.AddListener(() => OnCarButtonClicked(index));

//                 // Set car icon on button if available
//                 if (availableCars[i].carIcon != null)
//                 {
//                     Image buttonImage = availableCars[i].selectionButton.GetComponent<Image>();
//                     if (buttonImage != null)
//                     {
//                         buttonImage.sprite = availableCars[i].carIcon;
//                     }
//                 }
//             }
//         }

//         // Setup unlock button
//         if (unlockButton != null)
//         {
//             unlockButton.onClick.AddListener(OnUnlockButtonClicked);
//         }

//         // Unlock first car by default
//         if (availableCars.Count > 0)
//         {
//             availableCars[0].isUnlocked = true;
//         }

//         // Load saved progress (uncomment to enable save/load)
//         // LoadProgress();

//         // Show first car
//         SelectCar(0);
//         UpdateUI();
//     }

//     public void OnCarButtonClicked(int carIndex)
//     {
//         if (carIndex < 0 || carIndex >= availableCars.Count)
//             return;

//         currentSelectedIndex = carIndex;
//         SelectCar(carIndex);
//         UpdateUI();
//     }

//     void SelectCar(int carIndex)
//     {
//         CarData selectedCar = availableCars[carIndex];

//         // Disable all display cars
//         foreach (var car in availableCars)
//         {
//             if (car.displayCar != null)
//             {
//                 car.displayCar.SetActive(false);
//             }
//         }

//         // Enable selected display car
//         if (selectedCar.displayCar != null)
//         {
//             selectedCar.displayCar.SetActive(true);
//         }

//         // Handle player car in hierarchy (only if unlocked)
//         if (selectedCar.isUnlocked)
//         {
//             ActivatePlayerCar(carIndex);
//         }
//     }

//     void ActivatePlayerCar(int carIndex)
//     {
//         // Disable all player cars
//         foreach (var car in availableCars)
//         {
//             if (car.playerCar != null)
//             {
//                 car.playerCar.SetActive(false);
//             }
//         }

//         CarData selectedCar = availableCars[carIndex];

//         // Enable the selected player car
//         if (selectedCar.playerCar != null)
//         {
//             selectedCar.playerCar.SetActive(true);
//         }
//     }

//     void OnUnlockButtonClicked()
//     {
//         CarData selectedCar = availableCars[currentSelectedIndex];

//         // Check if already unlocked
//         if (selectedCar.isUnlocked)
//         {
//             // Car already unlocked, just activate it
//             ActivatePlayerCar(currentSelectedIndex);
//             return;
//         }

//         // Check if player has enough cash
//         if (playerCash >= selectedCar.unlockCost)
//         {
//             // Deduct cash
//             playerCash -= selectedCar.unlockCost;

//             // Unlock the car
//             selectedCar.isUnlocked = true;

//             // Activate player car
//             ActivatePlayerCar(currentSelectedIndex);

//             // Update UI
//             UpdateUI();

//             // Save progress
//             SaveProgress();

//             Debug.Log($"Unlocked {selectedCar.carName}!");
//         }
//         else
//         {
//             Debug.Log("Not enough cash!");
//             // You can add a shake animation or sound effect here
//         }
//     }

//     void UpdateUI()
//     {
//         // Update cash display
//         if (cashText != null)
//         {
//             cashText.text = playerCash.ToString();
//         }

//         // Update unlock button
//         CarData selectedCar = availableCars[currentSelectedIndex];

//         if (unlockButton != null && unlockButtonText != null)
//         {
//             if (selectedCar.isUnlocked)
//             {
//                 unlockButtonText.text = "SELECTED";
//                 unlockButton.interactable = false;
//             }
//             else
//             {
//                 unlockButtonText.text = $"UNLOCK\nFOR ${selectedCar.unlockCost}";
//                 unlockButton.interactable = playerCash >= selectedCar.unlockCost;
//             }
//         }

//         // Update button visuals (highlight selected)
//         UpdateButtonVisuals();
//     }

//     void UpdateButtonVisuals()
//     {
//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             if (availableCars[i].selectionButton != null)
//             {
//                 Image buttonImage = availableCars[i].selectionButton.GetComponent<Image>();

//                 if (buttonImage != null)
//                 {
//                     if (i == currentSelectedIndex)
//                     {
//                         // Highlight selected button (Yellow border or tint)
//                         buttonImage.color = new Color(1f, 1f, 0.5f, 1f); // Light yellow
//                     }
//                     else if (availableCars[i].isUnlocked)
//                     {
//                         // Unlocked but not selected (White/Normal)
//                         buttonImage.color = Color.white;
//                     }
//                     else
//                     {
//                         // Locked (Gray/Darkened)
//                         buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
//                     }
//                 }
//             }
//         }
//     }

//     // Save/Load system
//     public void SaveProgress()
//     {
//         PlayerPrefs.SetInt("PlayerCash", playerCash);
//         PlayerPrefs.SetInt("SelectedCar", currentSelectedIndex);

//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             PlayerPrefs.SetInt($"Car_{i}_Unlocked", availableCars[i].isUnlocked ? 1 : 0);
//         }

//         PlayerPrefs.Save();
//         Debug.Log("Progress Saved!");
//     }

//     public void LoadProgress()
//     {
//         playerCash = PlayerPrefs.GetInt("PlayerCash", 1255);
//         currentSelectedIndex = PlayerPrefs.GetInt("SelectedCar", 0);

//         for (int i = 0; i < availableCars.Count; i++)
//         {
//             availableCars[i].isUnlocked = PlayerPrefs.GetInt($"Car_{i}_Unlocked", i == 0 ? 1 : 0) == 1;
//         }

//         SelectCar(currentSelectedIndex);
//         UpdateUI();
//         Debug.Log("Progress Loaded!");
//     }

//     // Public method to add cash (call this when player earns money in game)
//     public void AddCash(int amount)
//     {
//         playerCash += amount;
//         UpdateUI();
//         SaveProgress();
//     }

//     // Get currently selected car name (useful for gameplay)
//     public string GetCurrentCarName()
//     {
//         if (currentSelectedIndex >= 0 && currentSelectedIndex < availableCars.Count)
//         {
//             return availableCars[currentSelectedIndex].carName;
//         }
//         return "";
//     }

//     // Get currently active player car GameObject
//     public GameObject GetCurrentPlayerCar()
//     {
//         if (currentSelectedIndex >= 0 && currentSelectedIndex < availableCars.Count)
//         {
//             return availableCars[currentSelectedIndex].playerCar;
//         }
//         return null;
//     }

//     // Check if a specific car is unlocked
//     public bool IsCarUnlocked(int carIndex)
//     {
//         if (carIndex >= 0 && carIndex < availableCars.Count)
//         {
//             return availableCars[carIndex].isUnlocked;
//         }
//         return false;
//     }
// }