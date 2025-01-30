using PlayerMake.Api;
using PlayerMake.V1;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerLoginSample {
    public class UIManager : MonoBehaviour
    {
        // Prefab references
        public Button buttonPrefab;

        // Scene object references
        public GameObject contentContainer;
        public InputField codeInput;
        public Text errorText;
        public Text playerNameText;
        public Text noAssetsText;
        public Text loadingText;
        public GameObject loggedOutPanel;
        public GameObject loggedInPanel;

        // Script variables
        public string currentPlayerId;
        public GameObject current;

        async void Start()
        {
            var developer = await PlayerMakeSdk.GetDeveloperAsync();

            if (!string.IsNullOrEmpty(developer.ShortId))
            {
                noAssetsText.text = $"You have no skins yet, go to <b>https://gamer.playermake.com/creator/{developer.ShortId}</b> to create your first skin.";
            }
            else
            {
                noAssetsText.text = $"You have no skins yet, go to <b>https://gamer.playermake.com/creator/{developer.Id}</b> to create your first skin.";
            }
        }


        void Update()
        {
            if (!string.IsNullOrEmpty(currentPlayerId))
            {
                loggedInPanel.SetActive(true);
                loggedOutPanel.SetActive(false);
            } else
            {
                loggedInPanel.SetActive(false);
                loggedOutPanel.SetActive(true);
            }
        }

        public async void TryLoadCreations()
        {
            foreach (Transform child in contentContainer.transform)
            {
                Destroy(child.gameObject);
            }

            if (string.IsNullOrEmpty(currentPlayerId))
            {
                var loginResponse = await PlayerMakeSdk.LoginPlayerWithCodeAsync(codeInput.text);

                errorText.gameObject.SetActive(false);

                if (!loginResponse.IsSuccess)
                {
                    errorText.gameObject.SetActive(true);
                    return;
                }

                codeInput.text = string.Empty;
                currentPlayerId = loginResponse.Id;
                playerNameText.text = "Hey, " + loginResponse.Name;
            }

            (var creations, var pagination) = await PlayerMakeSdk.ListCreationsAsync(
                statuses: new string[] { "APPROVED", "PENDING_APPROVAL" },
                playerId: currentPlayerId,
                callbacks: new RequestCallbacks()
            {
                OnProgress = (progress, passCount) =>
                {
                    switch ((passCount / 5) % 3)
                    {
                        case 0:
                            loadingText.text = ".";
                            break;
                        case 1:
                            loadingText.text = "..";
                            break;
                        case 2:
                            loadingText.text = "...";
                            break;
                    }
                }
            });

            loadingText.text = "Refresh";

            if (creations.Count == 0)
            {
                noAssetsText.gameObject.SetActive(true);
            }
            else
            {
                noAssetsText.gameObject.SetActive(false);
            }


            var icons = await PlayerMakeSdk.LoadIconsAsync(creations);

            foreach (var icon in icons)
            {
                var button = Instantiate(buttonPrefab, contentContainer.transform);

                button.onClick.AddListener(async () =>
                {
                    var creation = creations.Find(p => p.Id == icon.Id);

                    if (current != null)
                        Destroy(current);

                    current = await PlayerMakeSdk.InstantiateAsync(creation, "UGC Asset: " + icon.Id, transform);
                    current.transform.localPosition = new Vector3(0, 0.5f, 0);
                    current.transform.localEulerAngles = new Vector3(0, 180, 0);
                });

                var image = button.GetComponent<RawImage>();
                image.texture = icon.Image;
            }
        }

        public void LogOut()
        {
            currentPlayerId = null;
            playerNameText.text = string.Empty;

            foreach (Transform child in contentContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}