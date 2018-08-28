using UiGenerics;
using UnityEngine.UI;

namespace SceneManagement.Presentation
{
    public class LoadingManager : CanvasGroupView
    {
        public Image ProgressBar;

        private float _progress;
        private bool _isLoading;

        void Setup()
        {
            SceneManager.OnStartLoadingScene += OnStartLoading;
            SceneManager.OnEndLoadingScene += OnEndLoading;
            SceneManager.OnLoading += OnLoading;
            
            SceneManager.AssignManager(this.transform.parent.gameObject);
            SceneManager.EnableProgressReporting(this);

            Hide();
        }

        private void Update()
        {
            if (!_isLoading) return;
            
            ProgressBar.fillAmount = _progress;
        }

        private void OnStartLoading()
        {
            _isLoading = true;
            Show();
            _progress = 0;
        }

        private void OnEndLoading()
        {
            _isLoading = false;
            Hide();
        }

        private void OnLoading(float progress)
        {
            _progress = progress;
        }
    }
}